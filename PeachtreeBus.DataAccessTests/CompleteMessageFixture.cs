using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace PeachtreeBus.DataAccessTests
{
    [TestClass]
    public class CompleteMessageFixture : FixtureBase
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
        public async Task CompleteMessage_InsertsIntoCompleteTable()
        {
            // Add two messages;
            var testMessage1 = CreateTestMessage();
            testMessage1.Id = await dataAccess.EnqueueMessage(testMessage1, DefaultQueue);
            var testMessage2 = CreateTestMessage();
            testMessage2.Id = await dataAccess.EnqueueMessage(testMessage2, DefaultQueue);
            await Task.Delay(10); // wait for the rows to be ready

            // get and complete a message.
            var messageToComplete = await dataAccess.GetOnePendingMessage(DefaultQueue);
            messageToComplete.Completed = DateTime.UtcNow;
            await dataAccess.CompleteMessage(messageToComplete, DefaultQueue);
            await Task.Delay(10); // wait for the rows to be ready

            // Check that it ended up in the completed table.
            var completed = GetTableContent(CompletedMessagesTable).ToMessages();
            Assert.AreEqual(1, completed.Count);
            AssertMessageEquals(messageToComplete, completed[0]);
        }

        [TestMethod]
        public async Task CompleteMessage_DeletesFromPendingTable()
        {
            // Add two messages;
            var testMessage1 = CreateTestMessage();
            testMessage1.Id = await dataAccess.EnqueueMessage(testMessage1, DefaultQueue);
            var testMessage2 = CreateTestMessage();
            testMessage2.Id = await dataAccess.EnqueueMessage(testMessage2, DefaultQueue);
            await Task.Delay(10); // wait for the rows to be ready

            var messageToComplete = await dataAccess.GetOnePendingMessage(DefaultQueue);
            messageToComplete.Completed = DateTime.UtcNow;
            await dataAccess.CompleteMessage(messageToComplete, DefaultQueue);
            await Task.Delay(10); // wait for the rows to be ready

            var pending = GetTableContent(PendingMessagesTable).ToMessages();
            Assert.AreEqual(1, pending.Count);
            Assert.IsFalse(pending.Any(m => m.Id == messageToComplete.Id), "Completed message is still in the pending table.");
        }

        [TestMethod]
        public async Task CompleteMessage_CantMutateFields()
        {
            // Add two messages;
            var testMessage1 = CreateTestMessage();
            testMessage1.Id = await dataAccess.EnqueueMessage(testMessage1, DefaultQueue);
            await Task.Delay(10); // wait for the rows to be ready

            // get and complete a message.
            var messageToComplete = await dataAccess.GetOnePendingMessage(DefaultQueue);
            messageToComplete.Completed = DateTime.UtcNow;
            // screw with the fields that shouldn't change.
            messageToComplete.Body = "NewBody";
            messageToComplete.Enqueued = messageToComplete.Enqueued.AddMinutes(1);
            messageToComplete.MessageId = Guid.NewGuid();

            await dataAccess.CompleteMessage(messageToComplete, DefaultQueue);
            await Task.Delay(10); // wait for the rows to be ready

            // Check that it ended up in the completed table.
            var completed = GetTableContent(CompletedMessagesTable).ToMessages();
            Assert.AreEqual(1, completed.Count);
            var actual = completed.Single(m => m.Id == testMessage1.Id);

            // check the immutable fields are the oringal valules.
            Assert.AreEqual(testMessage1.MessageId, actual.MessageId, "MessageId should not change.");
            AssertSqlDbDateTime(testMessage1.Enqueued, actual.Enqueued);
            Assert.AreEqual(testMessage1.Body, actual.Body, "Body should not change.");
        }

        [TestMethod]
        public void CompleteMessage_ThrowsIfSchemaContainsUnsafe()
        {
            var action = new Action(() => dataAccess.CompleteMessage(new Model.QueueMessage(), DefaultQueue));
            ActionThrowsIfSchemaContainsPoisonChars(action);
        }

        [TestMethod]
        public void CompleteMessage_ThrowsIfQueueNameContainsUnsafe()
        {
            var action = new Action<string>((s) => dataAccess.CompleteMessage(new Model.QueueMessage(), s));
            ActionThrowsIfParameterContainsPoisonChars(action);
        }

    }
}
