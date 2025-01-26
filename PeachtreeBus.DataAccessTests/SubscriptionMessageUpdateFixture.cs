using Microsoft.VisualStudio.TestTools.UnitTesting;
using PeachtreeBus.Data;
using PeachtreeBus.Subscriptions;
using PeachtreeBus.Tests;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace PeachtreeBus.DataAccessTests
{
    /// <summary>
    /// Proves the behavior of DapperDataAccess.Update (subscribed)
    /// </summary>
    [TestClass]
    public class SubscriptionMessageUpdateFixture : DapperDataAccessFixtureBase
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
        /// proves that the pending table is updated.
        /// And that the fields that must not change do not change.
        /// </summary>
        /// <returns></returns>
        [TestMethod]
        public async Task Update_UpdatesPendingTable()
        {
            // Add two messages;
            var testMessage1 = TestData.CreateSubscribedMessage();
            testMessage1.SubscriberId = SubscriberId.New();
            testMessage1.Id = await dataAccess.AddMessage(testMessage1);
            var testMessage2 = TestData.CreateSubscribedMessage();
            testMessage2.SubscriberId = SubscriberId.New();
            testMessage2.Id = await dataAccess.AddMessage(testMessage2);
            await Task.Delay(10); // wait for the rows to be ready

            // get and update a message.
            var toUpdate = await dataAccess.GetPendingSubscribed(testMessage1.SubscriberId);
            Assert.IsNotNull(toUpdate);

            // set changed values
            toUpdate.SubscriberId = SubscriberId.New(); // should never change.
            toUpdate.ValidUntil = toUpdate.ValidUntil.AddDays(1); // should never change
            toUpdate.MessageId = UniqueIdentity.New(); // this should never persist a change.
            toUpdate.Enqueued = toUpdate.Enqueued.AddMinutes(-1); // this should never change.
            toUpdate.Body = new("Changed Body"); // should never change.
            toUpdate.Headers = new("Changed Headers");
            toUpdate.NotBefore = toUpdate.NotBefore.AddMinutes(1);
            toUpdate.Completed = DateTime.UtcNow;
            toUpdate.Failed = DateTime.UtcNow;
            toUpdate.Retries = 10;

            await dataAccess.Update(toUpdate);
            await Task.Delay(10); // wait for the rows to be ready

            // Check that it ended up in the error table.
            var pending = GetTableContent(SubscribedPendingTable).ToSubscribed();
            Assert.AreEqual(2, pending.Count);

            var expectUnchanged = toUpdate.Id == testMessage1.Id ? testMessage2 : testMessage1;
            var changedOriginal = toUpdate.Id != testMessage1.Id ? testMessage2 : testMessage1;

            var actualUnchanged = pending.Single(m => m.Id != toUpdate.Id);
            AssertSubscribedEquals(expectUnchanged, actualUnchanged);

            var actualChanged = pending.Single(m => m.Id == toUpdate.Id);
            // compare the unchangable fields.
            Assert.AreEqual(changedOriginal.Id, actualChanged.Id);
            Assert.AreEqual(changedOriginal.MessageId, actualChanged.MessageId);
            AssertSqlDbDateTime(changedOriginal.Enqueued, actualChanged.Enqueued);
            Assert.AreEqual(changedOriginal.Body, actualChanged.Body);
            Assert.AreEqual(changedOriginal.SubscriberId, actualChanged.SubscriberId);
            AssertSqlDbDateTime(changedOriginal.ValidUntil, actualChanged.ValidUntil);

            // compare the changeable fields.
            Assert.AreEqual(toUpdate.Headers, actualChanged.Headers);
            AssertSqlDbDateTime(toUpdate.NotBefore, actualChanged.NotBefore);
            Assert.AreEqual(toUpdate.Retries, actualChanged.Retries);

            // completed and failed will be null for pending messages.            
            AssertSqlDbDateTime(null, actualChanged.Completed);
            AssertSqlDbDateTime(null, actualChanged.Failed);
        }
    }
}
