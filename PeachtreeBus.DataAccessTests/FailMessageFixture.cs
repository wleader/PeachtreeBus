using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace PeachtreeBus.DataAccessTests
{
    [TestClass]
    public class FailMessageFixture : FixtureBase
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
        public async Task FailMessage_InsertsIntoErrorTable()
        {
            // Add two messages;
            var testMessage1 = CreateTestMessage();
            testMessage1.Id = await dataAccess.EnqueueMessage(testMessage1, DefaultQueue);
            var testMessage2 = CreateTestMessage();
            testMessage2.Id = await dataAccess.EnqueueMessage(testMessage2, DefaultQueue);
            await Task.Delay(10); // wait for the rows to be ready

            // get and Fail a message.
            var messageToFail = await dataAccess.GetOnePendingMessage(DefaultQueue);
            messageToFail.Failed = DateTime.UtcNow;
            await dataAccess.FailMessage(messageToFail, DefaultQueue);
            await Task.Delay(10); // wait for the rows to be ready

            // Check that it ended up in the error table.
            var failed = GetTableContent(ErrorMessagesTable).ToMessages();
            Assert.AreEqual(1, failed.Count);
            AssertMessageEquals(messageToFail, failed[0]);
        }

        [TestMethod]
        public async Task FailMessage_DeletesFromPendingTable()
        {
            // Add two messages;
            var testMessage1 = CreateTestMessage();
            testMessage1.Id = await dataAccess.EnqueueMessage(testMessage1, DefaultQueue);
            var testMessage2 = CreateTestMessage();
            testMessage2.Id = await dataAccess.EnqueueMessage(testMessage2, DefaultQueue);
            await Task.Delay(10); // wait for the rows to be ready

            var messageToFail = await dataAccess.GetOnePendingMessage(DefaultQueue);
            messageToFail.Failed = DateTime.UtcNow;
            await dataAccess.FailMessage(messageToFail, DefaultQueue);
            await Task.Delay(10); // wait for the rows to be ready

            var pending = GetTableContent(PendingMessagesTable).ToMessages();
            Assert.AreEqual(1, pending.Count);
            Assert.IsFalse(pending.Any(m => m.Id == messageToFail.Id), "Faield message is still in the pending table.");
        }

        [TestMethod]
        public async Task FailMessage_CantMutateFields()
        {
            // Add two messages;
            var testMessage1 = CreateTestMessage();
            testMessage1.Id = await dataAccess.EnqueueMessage(testMessage1, DefaultQueue);
            await Task.Delay(10); // wait for the rows to be ready

            // get and faile a message.
            var messageToFail = await dataAccess.GetOnePendingMessage(DefaultQueue);
            messageToFail.Failed = DateTime.UtcNow;
            // screw with the fields that shouldn't change.
            messageToFail.Body = "NewBody";
            messageToFail.Enqueued = messageToFail.Enqueued.AddMinutes(1);
            messageToFail.MessageId = Guid.NewGuid();

            await dataAccess.FailMessage(messageToFail, DefaultQueue);
            await Task.Delay(10); // wait for the rows to be ready

            // Check that it ended up in the completed table.
            var failed = GetTableContent(ErrorMessagesTable).ToMessages();
            Assert.AreEqual(1, failed.Count);
            var actual = failed.Single(m => m.Id == testMessage1.Id);

            // check the immutable fields are the oringal valules.
            Assert.AreEqual(testMessage1.MessageId, actual.MessageId, "MessageId should not change.");
            AssertSqlDbDateTime(testMessage1.Enqueued, actual.Enqueued);
            Assert.AreEqual(testMessage1.Body, actual.Body, "Body should not change.");
        }


        [TestMethod]
        public void FailMessage_ThrowsIfDateTimeKindUnspecified()
        {
            var action = new Action<Model.QueueMessage>((m) => dataAccess.FailMessage(m, DefaultQueue));
            ActionThrowsForMessagesWithUnspecifiedDateTimeKinds(action);
        }

        [TestMethod]
        public void FailMessage_ThrowsIfSchemaContainsUnsafe()
        {
            var action = new Action(() => dataAccess.FailMessage(new Model.QueueMessage(), DefaultQueue));
            ActionThrowsIfSchemaContainsPoisonChars(action);
        }

        [TestMethod]
        public void FailMessage_ThrowsIfQueueNameContainsUnsafe()
        {
            var action = new Action<string>((s) => dataAccess.FailMessage(new Model.QueueMessage(), s));
            ActionThrowsIfParameterContainsPoisonChars(action);
        }

    }
}
