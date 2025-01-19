using Microsoft.VisualStudio.TestTools.UnitTesting;
using PeachtreeBus.Data;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace PeachtreeBus.DataAccessTests
{
    /// <summary>
    /// Proves the behavior of DapperDataAcess.FailMessage
    /// </summary>
    [TestClass]
    public class QueueMessageFailedFixture : DapperDataAccessFixtureBase
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
        /// Proves that the message is copied into the failed table.
        /// </summary>
        /// <returns></returns>
        [TestMethod]
        public async Task FailMessage_InsertsIntoFailedTable()
        {
            // Add two messages;
            var testMessage1 = CreateQueueMessage();
            testMessage1.Id = await dataAccess.AddMessage(testMessage1, DefaultQueue);
            var testMessage2 = CreateQueueMessage();
            testMessage2.Id = await dataAccess.AddMessage(testMessage2, DefaultQueue);
            await Task.Delay(10); // wait for the rows to be ready

            // get and Fail a message.
            var messageToFail = await dataAccess.GetPendingQueued(DefaultQueue);
            Assert.IsNotNull(messageToFail);
            messageToFail.Failed = DateTime.UtcNow;
            await dataAccess.FailMessage(messageToFail, DefaultQueue);
            await Task.Delay(10); // wait for the rows to be ready

            // Check that it ended up in the error table.
            var failed = GetTableContent(QueueFailedTable).ToMessages();
            Assert.AreEqual(1, failed.Count);
            AssertMessageEquals(messageToFail, failed[0]);
        }

        /// <summary>
        /// Proves that the message is removed from the pending table.
        /// </summary>
        /// <returns></returns>
        [TestMethod]
        public async Task FailMessage_DeletesFromPendingTable()
        {
            // Add two messages;
            var testMessage1 = CreateQueueMessage();
            testMessage1.Id = await dataAccess.AddMessage(testMessage1, DefaultQueue);
            var testMessage2 = CreateQueueMessage();
            testMessage2.Id = await dataAccess.AddMessage(testMessage2, DefaultQueue);
            await Task.Delay(10); // wait for the rows to be ready

            var messageToFail = await dataAccess.GetPendingQueued(DefaultQueue);
            Assert.IsNotNull(messageToFail);
            messageToFail.Failed = DateTime.UtcNow;
            await dataAccess.FailMessage(messageToFail, DefaultQueue);
            await Task.Delay(10); // wait for the rows to be ready

            var pending = GetTableContent(QueuePendingTable).ToMessages();
            Assert.AreEqual(1, pending.Count);
            Assert.IsFalse(pending.Any(m => m.Id == messageToFail.Id), "Failed message is still in the pending table.");
        }

        /// <summary>
        /// proves that fields that should not change can't change.
        /// </summary>
        /// <returns></returns>
        [TestMethod]
        public async Task FailMessage_CantMutateFields()
        {
            // Add two messages;
            var testMessage1 = CreateQueueMessage();
            testMessage1.Id = await dataAccess.AddMessage(testMessage1, DefaultQueue);
            await Task.Delay(10); // wait for the rows to be ready

            // get and faile a message.
            var messageToFail = await dataAccess.GetPendingQueued(DefaultQueue);
            Assert.IsNotNull(messageToFail);
            messageToFail.Failed = DateTime.UtcNow;
            // screw with the fields that shouldn't change.
            messageToFail.Body = "NewBody";
            messageToFail.Enqueued = messageToFail.Enqueued.AddMinutes(1);
            messageToFail.MessageId = Guid.NewGuid();

            await dataAccess.FailMessage(messageToFail, DefaultQueue);
            await Task.Delay(10); // wait for the rows to be ready

            // Check that it ended up in the completed table.
            var failed = GetTableContent(QueueFailedTable).ToMessages();
            Assert.AreEqual(1, failed.Count);
            var actual = failed.Single(m => m.Id == testMessage1.Id);

            // check the immutable fields are the oringal valules.
            Assert.AreEqual(testMessage1.MessageId, actual.MessageId, "MessageId should not change.");
            AssertSqlDbDateTime(testMessage1.Enqueued, actual.Enqueued);
            Assert.AreEqual(testMessage1.Body, actual.Body, "Body should not change.");
        }
    }
}
