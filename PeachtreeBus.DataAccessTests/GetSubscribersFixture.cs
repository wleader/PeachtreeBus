using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace PeachtreeBus.DataAccessTests
{
    /// <summary>
    /// Proves the behavior of DapperDataAccess.GetSubscribers
    /// </summary>
    [TestClass]
    public class GetSubscribersFixture : FixtureBase
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
        /// Proves that statements do not run when schema contains characters
        /// that are a SQL injection risk.
        /// </summary>
        [TestMethod]
        public void GetSubscribers_ThrowsWhenSchemaContainsUnsafe()
        {
            var category = "TestCategory";
            var action = new Action(() => { dataAccess.GetSubscribers(category); }); ;
            ActionThrowsIfSchemaContainsPoisonChars(action);
        }
     
        /// <summary>
        /// Proves that the correct SubscriberIds are returned.
        /// </summary>
        /// <returns></returns>
        [TestMethod]
        public async Task GetSubscribers_GetsCorrectSubscriptions()
        {
            // assumes Subscribe method behaves as intended.
            var guid1 = Guid.NewGuid();
            var guid2 = Guid.NewGuid();
            var guid3 = Guid.NewGuid();
            var category1 = "TestCategory";
            var category2 = "TestCategory2";
            var category3 = "TestCategory3";

            var future = DateTime.UtcNow.AddHours(1);
            var expired = DateTime.UtcNow.AddHours(-1);

            await dataAccess.Subscribe(guid1, category1, future); // want this 
            await dataAccess.Subscribe(guid1, category2, future); // exclude wrong category
            await dataAccess.Subscribe(guid1, category3, expired); // exclude expired

            await dataAccess.Subscribe(guid2, category1, future); // want this
            await dataAccess.Subscribe(guid2, category2, future); // exclude wrong category
            await dataAccess.Subscribe(guid2, category3, expired); // expired

            await dataAccess.Subscribe(guid3, category2, future); // wrong category
            await dataAccess.Subscribe(guid3, category3, expired); // expired


            var actual = (await dataAccess.GetSubscribers(category1)).ToArray();

            Assert.AreEqual(2, actual.Length);
            CollectionAssert.Contains(actual, guid1);
            CollectionAssert.Contains(actual, guid2);
            CollectionAssert.DoesNotContain(actual, guid3);
        }
    }
}
