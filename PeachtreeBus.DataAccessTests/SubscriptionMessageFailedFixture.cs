using Microsoft.VisualStudio.TestTools.UnitTesting;
using PeachtreeBus.Abstractions.Tests;
using PeachtreeBus.Data;
using PeachtreeBus.Tests;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace PeachtreeBus.DataAccessTests
{
    /// <summary>
    /// Proves the behavior of DapperDataAccess.FailMessage (Subscribed)
    /// </summary>
    [TestClass]
    public class SubscriptionMessageFailedFixture : DapperDataAccessFixtureBase
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
        /// Proves that the message does not change.
        /// </summary>
        /// <returns></returns>
        [TestMethod]
        public async Task FailMessage_CantMutateFields()
        {
            var testMessage1 = TestData.CreateSubscribedData();
            await InsertSubscribedMessage(testMessage1);
            await Task.Delay(10); // wait for the rows to be ready

            // get and complete a message.
            var messageToComplete = await dataAccess.GetPendingSubscribed(testMessage1.SubscriberId);
            Assert.IsNotNull(messageToComplete);
            messageToComplete.Completed = DateTime.UtcNow;
            // screw with the fields that shouldn't change.
            messageToComplete.Body = new("NewBody");
            messageToComplete.Enqueued = messageToComplete.Enqueued.AddMinutes(1);
            messageToComplete.MessageId = UniqueIdentity.New();

            await dataAccess.FailMessage(messageToComplete);
            await Task.Delay(10); // wait for the rows to be ready

            // Check that it ended up in the completed table.
            var failed = GetTableContent(SubscribedFailed).ToMessages();
            Assert.AreEqual(1, failed.Count);
            var actual = failed.Single(m => m.Id == testMessage1.Id);

            // check the immutable fields are the oringal valules.
            Assert.AreEqual(testMessage1.MessageId, actual.MessageId, "MessageId should not change.");
            AssertSqlDbDateTime(testMessage1.Enqueued, actual.Enqueued);
            Assert.AreEqual(testMessage1.Body, actual.Body, "Body should not change.");
        }

        /// <summary>
        /// Proves the message is removed from the pending table.
        /// </summary>
        /// <returns></returns>
        [TestMethod]
        public async Task FailMessage_DeletesFromPendingTable()
        {
            var expected1 = TestData.CreateSubscribedData(
                validUntil: DateTime.UtcNow.AddMinutes(-1));
            await InsertSubscribedMessage(expected1);

            Assert.AreEqual(1, CountRowsInTable(SubscribedPending));

            await dataAccess.FailMessage(expected1);

            Assert.AreEqual(0, CountRowsInTable(SubscribedPending));
        }

        /// <summary>
        /// proves the message is copied to the failed table.
        /// </summary>
        /// <returns></returns>
        [TestMethod]
        public async Task FailMessage_InsertsIntoFailedTable()
        {
            Assert.AreEqual(0, CountRowsInTable(SubscribedPending));
            Assert.AreEqual(0, CountRowsInTable(SubscribedFailed));

            var expected1 = TestData.CreateSubscribedData(
                validUntil: DateTime.UtcNow.AddMinutes(-1));
            await InsertSubscribedMessage(expected1);

            await dataAccess.FailMessage(expected1);

            var failed = GetSubscribedFailed();

            Assert.AreEqual(1, failed.Count);

            var actual1 = failed.Single(s => s.Id == expected1.Id);
            Assert.IsTrue(actual1.Failed.HasValue);
            expected1.Failed = actual1.Failed;
            AssertSubscribedEquals(expected1, actual1);
        }
    }
}
