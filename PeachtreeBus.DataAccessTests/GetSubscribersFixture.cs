using Microsoft.VisualStudio.TestTools.UnitTesting;
using PeachtreeBus.Subscriptions;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace PeachtreeBus.DataAccessTests
{
    /// <summary>
    /// Proves the behavior of DapperDataAccess.GetSubscribers
    /// </summary>
    [TestClass]
    public class GetSubscribersFixture : DapperDataAccessFixtureBase
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
        /// Proves that the correct SubscriberIds are returned.
        /// </summary>
        /// <returns></returns>
        [TestMethod]
        public async Task GetSubscribers_GetsCorrectSubscriptions()
        {
            // assumes Subscribe method behaves as intended.
            var subscriber1 = SubscriberId.New();
            var subscriber2 = SubscriberId.New();
            var subscriber3 = SubscriberId.New();
            var category1 = new Category("TestCategory1");
            var category2 = new Category("TestCategory2");
            var category3 = new Category("TestCategory3");

            var future = DateTime.UtcNow.AddHours(1);
            var expired = DateTime.UtcNow.AddHours(-1);

            await dataAccess.Subscribe(subscriber1, category1, future); // want this 
            await dataAccess.Subscribe(subscriber1, category2, future); // exclude wrong category
            await dataAccess.Subscribe(subscriber1, category3, expired); // exclude expired

            await dataAccess.Subscribe(subscriber2, category1, future); // want this
            await dataAccess.Subscribe(subscriber2, category2, future); // exclude wrong category
            await dataAccess.Subscribe(subscriber2, category3, expired); // expired

            await dataAccess.Subscribe(subscriber3, category2, future); // wrong category
            await dataAccess.Subscribe(subscriber3, category3, expired); // expired


            var actual = (await dataAccess.GetSubscribers(category1)).ToArray();

            Assert.AreEqual(2, actual.Length);
            CollectionAssert.Contains(actual, subscriber1);
            CollectionAssert.Contains(actual, subscriber2);
            CollectionAssert.DoesNotContain(actual, subscriber3);
        }
    }
}
