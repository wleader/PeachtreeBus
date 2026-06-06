using Microsoft.VisualStudio.TestTools.UnitTesting;
using PeachtreeBus.Core.Tests;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace PeachtreeBus.DataAccessTests
{
    /// <summary>
    /// Proves the behavior of DapperDataAccess.ExpireSubscriptionMessages
    /// </summary>
    [TestClass]
    public class SubscriptionExpireMessagesFixture : MsSqlBusDataAccessFixtureBase
    {
        [TestInitialize]
        public override void Initialize() => base.Initialize();

        [TestCleanup]
        public override void Cleanup() => base.Cleanup();

        /// <summary>
        /// Proves that messages are copied to the Failed table.
        /// </summary>
        /// <returns></returns>
        [TestMethod]
        public async Task ExpireMessages_InsertsIntoFailedTable()
        {
            Assert.AreEqual(0, CountRowsInTable(TestConfig.SubscribedPending));
            Assert.AreEqual(0, CountRowsInTable(TestConfig.SubscribedFailed));

            var expected1 = TestData.CreateSubscribedData(
                validUntil: DateTime.UtcNow.AddMinutes(-1));
            await InsertSubscribedMessage(expected1);

            var expected2 = TestData.CreateSubscribedData(
                validUntil: DateTime.UtcNow.AddMinutes(-1));
            await InsertSubscribedMessage(expected2);

            await dataAccess.ExpireSubscriptionMessages(1000);

            var failed = GetSubscribedFailed();
            Assert.AreEqual(2, failed.Count);

            var actual1 = failed.Single(s => s.Id == expected1.Id);
            Assert.IsTrue(actual1.Failed.HasValue);
            expected1.Failed = actual1.Failed;
            AssertSubscribedEquals(expected1, actual1);

            var actual2 = failed.Single(s => s.Id == expected2.Id);
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

            var expected1 = TestData.CreateSubscribedData(
                validUntil: DateTime.UtcNow.AddMinutes(-1));
            await InsertSubscribedMessage(expected1);

            var expected2 = TestData.CreateSubscribedData(
                validUntil: DateTime.UtcNow.AddMinutes(-1));
            await InsertSubscribedMessage(expected2);

            Assert.AreEqual(2, CountRowsInTable(TestConfig.SubscribedPending));

            await dataAccess.ExpireSubscriptionMessages(1000);

            Assert.AreEqual(0, CountRowsInTable(TestConfig.SubscribedPending));
        }

        [TestMethod]
        public async Task ExpireMessage_LimitsToMaxCount()
        {
            var expected1 = TestData.CreateSubscribedData(
                validUntil: DateTime.UtcNow.AddMinutes(-1));
            await InsertSubscribedMessage(expected1);

            var expected2 = TestData.CreateSubscribedData(
                validUntil: DateTime.UtcNow.AddMinutes(-1));
            await InsertSubscribedMessage(expected2);

            Assert.AreEqual(2, CountRowsInTable(TestConfig.SubscribedPending));

            await dataAccess.ExpireSubscriptionMessages(1);

            Assert.AreEqual(1, CountRowsInTable(TestConfig.SubscribedPending));

            await dataAccess.ExpireSubscriptionMessages(1);

            Assert.AreEqual(0, CountRowsInTable(TestConfig.SubscribedPending));
        }
    }
}
