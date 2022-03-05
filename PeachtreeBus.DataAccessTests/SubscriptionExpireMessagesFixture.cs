using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace PeachtreeBus.DataAccessTests
{
    /// <summary>
    /// Proves the behavior of DapperDataAccess.ExpireSubscriptionMessages
    /// </summary>
    [TestClass]
    public class SubscriptionExpireMessagesFixture : FixtureBase
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
        /// Proves that messages are copied to the Failed table.
        /// </summary>
        /// <returns></returns>
        [TestMethod]
        public async Task ExpireMessages_InsertsIntoFailedTable()
        {
            Assert.AreEqual(0, CountRowsInTable(SubscribedPendingTable));
            Assert.AreEqual(0, CountRowsInTable(SubscribedFailedTable));

            var expected1 = CreateSubscribed();
            expected1.SubscriberId = Guid.NewGuid();
            expected1.ValidUntil = DateTime.UtcNow.AddMinutes(-1);
            expected1.Id = await dataAccess.AddMessage(expected1);

            var expected2 = CreateSubscribed();
            expected2.SubscriberId = Guid.NewGuid();
            expected2.ValidUntil = DateTime.UtcNow.AddMinutes(-1);
            expected2.Id = await dataAccess.AddMessage(expected2);

            await dataAccess.ExpireSubscriptionMessages();

            var failed = GetTableContent(SubscribedFailedTable).ToSubscribed();

            Assert.AreEqual(2, failed.Count);

            var actual1 = failed.Single(s => s.SubscriberId == expected1.SubscriberId);
            Assert.IsTrue(actual1.Failed.HasValue);
            expected1.Failed = actual1.Failed;
            AssertSubscribedEquals(expected1, actual1);

            var actual2 = failed.Single(s => s.SubscriberId == expected2.SubscriberId);
            Assert.IsTrue(actual2.Failed.HasValue);
            expected2.Failed = actual2.Failed;
            AssertSubscribedEquals(expected2, actual2);
        }

        /// <summary>
        /// Proves that messages are removed from the pending table.
        /// </summary>
        /// <returns></returns>
        [TestMethod]
        public async Task ExpireMessages_DeletesFromPending()
        {
            
            var expected1 = CreateSubscribed();
            expected1.SubscriberId = Guid.NewGuid();
            expected1.ValidUntil = DateTime.UtcNow.AddMinutes(-1);
            expected1.Id = await dataAccess.AddMessage(expected1);

            var expected2 = CreateSubscribed();
            expected2.SubscriberId = Guid.NewGuid();
            expected2.ValidUntil = DateTime.UtcNow.AddMinutes(-1);
            expected2.Id = await dataAccess.AddMessage(expected2);

            Assert.AreEqual(2, CountRowsInTable(SubscribedPendingTable));

            await dataAccess.ExpireSubscriptionMessages();

            Assert.AreEqual(0, CountRowsInTable(SubscribedPendingTable));
        }

        /// <summary>
        /// Proves that unsafe schema is not allowed.
        /// </summary>
        [TestMethod]
        public void ExpireMessages_ThrowsIfSchemaContainsUnsafe()
        {
            var action = new Action(() => dataAccess.ExpireSubscriptionMessages());
            ActionThrowsIfSchemaContainsPoisonChars(action);
        }
    }
}
