using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace PeachtreeBus.DataAccessTests
{
    [TestClass]
    public class GetOnePendingMessageFixture : FixtureBase
    {
        [TestInitialize]
        public override void TestInitialize()
        {
            base.TestInitialize();
        }
        
        [TestCleanup]
        public override void TestCleanup()
        {
            base.TestCleanup();
        }

        [TestMethod]
        public async Task GetOnePendingMessage_GetsMessage()
        {
            // Add one message;
            var testMessage = CreateTestMessage();
            testMessage.Id = await dataAccess.EnqueueMessage(testMessage, DefaultQueue);

            await Task.Delay(10); // wait for the rows to be ready

            var actual = await dataAccess.GetOnePendingMessage(DefaultQueue);

            AssertMessageEquals(testMessage, actual);
        }

        [TestMethod]
        public async Task GetOnePendingMessage_LocksTheMessage()
        {
            // Add two messages;
            var testMessage1 = CreateTestMessage();
            testMessage1.Id = await dataAccess.EnqueueMessage(testMessage1, DefaultQueue);
            var testMessage2 = CreateTestMessage();
            testMessage2.Id = await dataAccess.EnqueueMessage(testMessage2, DefaultQueue);

            await Task.Delay(10); // wait for the rows to be ready

            // get a message and leave the transaction open.
            dataAccess.BeginTransaction();
            try
            {
                var actual = await dataAccess.GetOnePendingMessage(DefaultQueue);
                Assert.IsNotNull(actual, "Did not read a message back.");

                BeginSecondaryTransaction();
                try
                {
                    var unlockedMessages = GetTableContentAndLock(PendingMessagesTable).ToMessages();

                    Assert.AreEqual(1, unlockedMessages.Count, "Wrong number of unlocked messages.");
                    Assert.AreNotEqual(testMessage1.Id, testMessage2.Id, "Test Messages have the same ID.");
                    Assert.IsFalse(unlockedMessages.Any(m => m.Id == actual.Id), $"Locked message {actual.Id} found in unlocked messages {unlockedMessages[0].Id}");
                }
                finally
                {
                    RollbackSecondaryTransaction();
                }
            }
            finally
            {
                dataAccess.RollbackTransaction();
            }
        }

        [TestMethod]
        public async Task GetOnePendingMessage_DoesNotReturnLocked()
        {
            // Add one message;
            var testMessage = CreateTestMessage();
            testMessage.Id = await dataAccess.EnqueueMessage(testMessage, DefaultQueue);
            await Task.Delay(10); // wait for the rows to be ready

            // lock the row
            BeginSecondaryTransaction();
            try
            {
                var pending = GetTableContentAndLock(PendingMessagesTable);

                // check that the locked row can not be fetched.
                var actual = await dataAccess.GetOnePendingMessage(DefaultQueue);
                Assert.IsNull(actual);
            }
            finally
            {
                RollbackSecondaryTransaction();
            }
        }

        [TestMethod]
        public async Task GetOnePendingMessage_DoesNotReturnCompletedMessage()
        {
            // Add one message;
            var testMessage = CreateTestMessage();
            testMessage.Id = await dataAccess.EnqueueMessage(testMessage, DefaultQueue);
            await Task.Delay(10); // wait for the rows to be ready

            // normally EnqueueMessage can't insert a completed message so we have to maniupulate things.
            ExecuteNonQuery($"UPDATE [{DefaultSchema}].[{PendingMessagesTable}] SET [Completed] = SYSUTCDATETIME() WHERE [Id] = {testMessage.Id}");
            await Task.Delay(10); // wait for the rows to be ready

            var actual = await dataAccess.GetOnePendingMessage(DefaultQueue);
            Assert.IsNull(actual);
        }

        [TestMethod]
        public async Task GetOnePendingMessage_DoesNotReturnErrorMessage()
        {
            // Add one message;
            var testMessage = CreateTestMessage();
            testMessage.Id = await dataAccess.EnqueueMessage(testMessage, DefaultQueue);
            await Task.Delay(10); // wait for the rows to be ready

            // normally EnqueueMessage can't insert a failed message so we have to maniupulate things.
            ExecuteNonQuery($"UPDATE [{DefaultSchema}].[{PendingMessagesTable}] SET [Failed] = SYSUTCDATETIME() WHERE [Id] = {testMessage.Id}");
            await Task.Delay(10); // wait for the rows to be ready


            var actual = await dataAccess.GetOnePendingMessage(DefaultQueue);
            Assert.IsNull(actual);
        }

        [TestMethod]
        public async Task GetOnePendingMessage_DoesNotReturnDelayedMessage()
        {
            // Add one message;
            var testMessage = CreateTestMessage();
            testMessage.NotBefore = testMessage.NotBefore.AddHours(1);
            testMessage.Id = await dataAccess.EnqueueMessage(testMessage, DefaultQueue);
            await Task.Delay(10); // wait for the rows to be ready
            var actual = await dataAccess.GetOnePendingMessage(DefaultQueue);
            Assert.IsNull(actual);
        }

        [TestMethod]
        public async Task GetOnePendingMessage_DoesReturnDelayedAfterWait()
        {
            // Add one message;
            var testMessage = CreateTestMessage();
            testMessage.NotBefore = testMessage.NotBefore.AddMilliseconds(200);
            testMessage.Id = await dataAccess.EnqueueMessage(testMessage, DefaultQueue);
            await Task.Delay(10); // wait for the rows to be ready
            var actual = await dataAccess.GetOnePendingMessage(DefaultQueue);
            Assert.IsNull(actual);
            await Task.Delay(400);
            actual = await dataAccess.GetOnePendingMessage(DefaultQueue);
            AssertMessageEquals(testMessage, actual);
        }

        [TestMethod]
        public void GetOnePendingMessage_ThrowsIfSchemaContainsUnsafe()
        {
            var action = new Action(() => dataAccess.GetOnePendingMessage(DefaultQueue).Wait());
            ActionThrowsIfSchemaContainsPoisonChars(action);
        }

        [TestMethod]
        public void GetOnePendingMessage_ThrowsIfQueueNameContainsUnsafe()
        {
            var action = new Action<string>((s) =>  dataAccess.GetOnePendingMessage(s).Wait());
            ActionThrowsIfParameterContainsPoisonChars(action);
        }
    }
}
