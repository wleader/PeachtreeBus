using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace PeachtreeBus.DataAccessTests
{
    /// <summary>
    /// Proves the behavior of DapperDataAccess.ExpireSubscriptions
    /// </summary>
    [TestClass]
    public class ExpireSubscriptionsFixture : DapperDataAccessFixtureBase
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
        /// Proves that expired rows are deleted.
        /// </summary>
        /// <returns></returns>
        [TestMethod]
        public async Task ExpireSubscriptions_DeletesCorrectRows()
        {
            // assumes subscribe is working correctly

            var guid1 = Guid.NewGuid();
            var guid2 = Guid.NewGuid();
            var guid3 = Guid.NewGuid();
            var guid4 = Guid.NewGuid();
            var category1 = "TestCategory1";
            var category2 = "TestCategory2";
            var valid = DateTime.UtcNow.AddHours(1);
            var expired = DateTime.UtcNow.AddSeconds(-1);

            await dataAccess.Subscribe(guid1, category1, valid);
            await dataAccess.Subscribe(guid2, category2, valid);
            await dataAccess.Subscribe(guid3, category1, expired);
            await dataAccess.Subscribe(guid4, category2, expired);

            await dataAccess.ExpireSubscriptions();

            var rows = GetTableContent("Subscriptions").ToSubscriptions();

            Assert.AreEqual(2, rows.Count);
            Assert.IsNotNull(rows.Single(r => r.SubscriberId == guid1 && r.Category == category1));
            Assert.IsNotNull(rows.Single(r => r.SubscriberId == guid2 && r.Category == category2));
        }
    }
}
