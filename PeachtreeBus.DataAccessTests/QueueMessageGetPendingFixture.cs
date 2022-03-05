using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace PeachtreeBus.DataAccessTests
{
    /// <summary>
    /// Proves the behavior of DapperDataAccess.GetPendingQueued
    /// </summary>
    [TestClass]
    public class QueueMessageGetPendingFixture : FixtureBase
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

        /// <summary>
        /// proves that a message is returned.
        /// </summary>
        /// <returns></returns>
        [TestMethod]
        public async Task GetPendingQueued_GetsMessage()
        {
            // Add one message;
            var testMessage = CreateQueueMessage();
            testMessage.Id = await dataAccess.AddMessage(testMessage, DefaultQueue);

            await Task.Delay(10); // wait for the rows to be ready

            var actual = await dataAccess.GetPendingQueued(DefaultQueue);

            AssertMessageEquals(testMessage, actual);
        }

        /// <summary>
        /// Proves that the returned message is locked so that other
        /// connections to the DB cannot get the same message.
        /// </summary>
        /// <returns></returns>
        [TestMethod]
        public async Task GetPendingQueued_LocksTheMessage()
        {
            // Add two messages;
            var testMessage1 = CreateQueueMessage();
            testMessage1.Id = await dataAccess.AddMessage(testMessage1, DefaultQueue);
            var testMessage2 = CreateQueueMessage();
            testMessage2.Id = await dataAccess.AddMessage(testMessage2, DefaultQueue);

            await Task.Delay(10); // wait for the rows to be ready

            // get a message and leave the transaction open.
            dataAccess.BeginTransaction();
            try
            {
                var actual = await dataAccess.GetPendingQueued(DefaultQueue);
                Assert.IsNotNull(actual, "Did not read a message back.");

                BeginSecondaryTransaction();
                try
                {
                    var unlockedMessages = GetTableContentAndLock(QueuePendingTable).ToMessages();

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

        /// <summary>
        /// Proves that locked messages are not returned.
        /// </summary>
        /// <returns></returns>
        [TestMethod]
        public async Task GetPendingQueued_DoesNotReturnLocked()
        {
            // Add one message;
            var testMessage = CreateQueueMessage();
            testMessage.Id = await dataAccess.AddMessage(testMessage, DefaultQueue);
            await Task.Delay(10); // wait for the rows to be ready

            // lock the row
            BeginSecondaryTransaction();
            try
            {
                var pending = GetTableContentAndLock(QueuePendingTable);

                // check that the locked row can not be fetched.
                var actual = await dataAccess.GetPendingQueued(DefaultQueue);
                Assert.IsNull(actual);
            }
            finally
            {
                RollbackSecondaryTransaction();
            }
        }

        /// <summary>
        /// Proves that a completed message cannot be returned.
        /// </summary>
        /// <remarks>The pending table really shouldn't contain a message where compelted is not null anyway.</remarks>
        /// <returns></returns>
        [TestMethod]
        public async Task GetPendingQueued_DoesNotReturnCompletedMessage()
        {
            // Add one message;
            var testMessage = CreateQueueMessage();
            testMessage.Id = await dataAccess.AddMessage(testMessage, DefaultQueue);
            await Task.Delay(10); // wait for the rows to be ready

            // normally EnqueueMessage can't insert a completed message so we have to maniupulate things.
            ExecuteNonQuery($"UPDATE [{DefaultSchema}].[{QueuePendingTable}] SET [Completed] = SYSUTCDATETIME() WHERE [Id] = {testMessage.Id}");
            await Task.Delay(10); // wait for the rows to be ready

            var actual = await dataAccess.GetPendingQueued(DefaultQueue);
            Assert.IsNull(actual);
        }

        /// <summary>
        /// Proves that a failed message cannot be returned.
        /// </summary>
        /// <remarks>The pending table really shouldn't contain a message where compelted is not null anyway.</remarks>
        /// <returns></returns>
        [TestMethod]
        public async Task GetPendingQueued_DoesNotReturnFailedMessage()
        {
            // Add one message;
            var testMessage = CreateQueueMessage();
            testMessage.Id = await dataAccess.AddMessage(testMessage, DefaultQueue);
            await Task.Delay(10); // wait for the rows to be ready

            // normally EnqueueMessage can't insert a failed message so we have to maniupulate things.
            ExecuteNonQuery($"UPDATE [{DefaultSchema}].[{QueuePendingTable}] SET [Failed] = SYSUTCDATETIME() WHERE [Id] = {testMessage.Id}");
            await Task.Delay(10); // wait for the rows to be ready


            var actual = await dataAccess.GetPendingQueued(DefaultQueue);
            Assert.IsNull(actual);
        }

        /// <summary>
        /// Proves that a message is not returned before its NotBefore time.
        /// </summary>
        /// <returns></returns>
        [TestMethod]
        public async Task GetPendingQueued_DoesNotReturnDelayedMessage()
        {
            // Add one message;
            var testMessage = CreateQueueMessage();
            testMessage.NotBefore = testMessage.NotBefore.AddHours(1);
            testMessage.Id = await dataAccess.AddMessage(testMessage, DefaultQueue);
            await Task.Delay(10); // wait for the rows to be ready
            var actual = await dataAccess.GetPendingQueued(DefaultQueue);
            Assert.IsNull(actual);
        }

        /// <summary>
        /// Proves that a message can be returned after its NotBefore time.
        /// </summary>
        /// <returns></returns>
        [TestMethod]
        public async Task GetPendingQueued_DoesReturnDelayedAfterWait()
        {
            // Add one message;
            var testMessage = CreateQueueMessage();
            testMessage.NotBefore = testMessage.NotBefore.AddMilliseconds(200);
            testMessage.Id = await dataAccess.AddMessage(testMessage, DefaultQueue);
            await Task.Delay(10); // wait for the rows to be ready
            var actual = await dataAccess.GetPendingQueued(DefaultQueue);
            Assert.IsNull(actual);
            await Task.Delay(400);
            actual = await dataAccess.GetPendingQueued(DefaultQueue);
            AssertMessageEquals(testMessage, actual);
        }

        /// <summary>
        /// Proves that unsafe schema are not allowed.
        /// </summary>
        [TestMethod]
        public void GetPendingQueued_ThrowsIfSchemaContainsUnsafe()
        {
            var action = new Action(() => dataAccess.GetPendingQueued(DefaultQueue).Wait());
            ActionThrowsIfSchemaContainsPoisonChars(action);
        }

        /// <summary>
        /// proves that unsafe queue names are not allowed.
        /// </summary>
        [TestMethod]
        public void GetPendingQueued_ThrowsIfQueueNameContainsUnsafe()
        {
            var action = new Action<string>((s) =>  dataAccess.GetPendingQueued(s).Wait());
            ActionThrowsIfParameterContainsPoisonChars(action);
        }
    }
}
