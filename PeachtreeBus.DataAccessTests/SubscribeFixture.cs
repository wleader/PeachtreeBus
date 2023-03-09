using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace PeachtreeBus.DataAccessTests
{
    /// <summary>
    /// Proves the behavior of DapperDataAccess.Subscribe
    /// </summary>
    [TestClass]
    public class SubscribeFixture : DapperDataAccessFixtureBase
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
        /// Proves the row is added when a matching row does not exist.
        /// </summary>
        /// <returns></returns>
        [TestMethod]
        public async Task Subscribe_AddsRowWhenSubscriberAndCategoryDoNotExist()
        {
            var subscriptions = GetTableContent("Subscriptions").ToSubscriptions();
            Assert.AreEqual(0, subscriptions.Count);

            var subscriber = Guid.NewGuid();
            var category = "TestCategory";
            var until = DateTime.UtcNow.AddMinutes(30);

            await dataAccess.Subscribe(subscriber, category, until);

            subscriptions = GetTableContent("Subscriptions").ToSubscriptions();

            Assert.AreEqual(1, subscriptions.Count);

            Assert.AreEqual(subscriber, subscriptions[0].SubscriberId);
            Assert.AreEqual(category, subscriptions[0].Category);
            AssertSqlDbDateTime(until, subscriptions[0].ValidUntil);
        }

        /// <summary>
        /// proves the row is added when other rows for the same subscriber exists, but a row
        /// for the category does not.
        /// </summary>
        /// <returns></returns>
        [TestMethod]
        public async Task Subscribe_AddsRowWhenSubscriberExistsAndCategoryDoesNot()
        {
            var subscriptions = GetTableContent("Subscriptions").ToSubscriptions();
            Assert.AreEqual(0, subscriptions.Count);

            var subscriber = Guid.NewGuid();
            var category = "TestCategory";
            var until = DateTime.UtcNow.AddMinutes(30);

            await dataAccess.Subscribe(subscriber, category, until);

            var category2 = "TestCategory2";
            await dataAccess.Subscribe(subscriber, category2, until);

            subscriptions = GetTableContent("Subscriptions").ToSubscriptions();
            Assert.AreEqual(2, subscriptions.Count);

            subscriptions.ForEach(s => Assert.AreEqual(subscriber, s.SubscriberId));
            subscriptions.ForEach(s => AssertSqlDbDateTime(until, s.ValidUntil));

            var categores = subscriptions.Select(s => s.Category).ToList();
            Assert.IsTrue(categores.Contains(category));
            Assert.IsTrue(categores.Contains(category2));
        }

        /// <summary>
        /// Proves that the row is added when other subscribers are using the same category.
        /// </summary>
        /// <returns></returns>
        [TestMethod]
        public async Task Subscribe_AddsRowWhenSubscriberDoesNotExistAndCategoryExists()
        {
            var subscriptions = GetTableContent("Subscriptions").ToSubscriptions();
            Assert.AreEqual(0, subscriptions.Count);

            var subscriber = Guid.NewGuid();
            var category = "TestCategory";
            var until = DateTime.UtcNow.AddMinutes(30);

            await dataAccess.Subscribe(subscriber, category, until);

            var subscriber2 = Guid.NewGuid(); ;
            await dataAccess.Subscribe(subscriber2, category, until);

            subscriptions = GetTableContent("Subscriptions").ToSubscriptions();
            Assert.AreEqual(2, subscriptions.Count);

            subscriptions.ForEach(s => Assert.AreEqual(category, s.Category));
            subscriptions.ForEach(s => AssertSqlDbDateTime(until, s.ValidUntil));

            var subscribers = subscriptions.Select(s => s.SubscriberId).ToList();
            Assert.IsTrue(subscribers.Contains(subscriber));
            Assert.IsTrue(subscribers.Contains(subscriber2));
        }

        /// <summary>
        /// Proves the row is updated when a row for the subscriber and category already exists.
        /// </summary>
        /// <returns></returns>
        [TestMethod]
        public async Task Subscribe_UpdatesWhenSubscriberAndCategoryAlreadyExist()
        {
            var subscriptions = GetTableContent("Subscriptions").ToSubscriptions();
            Assert.AreEqual(0, subscriptions.Count);

            var subscriber = Guid.NewGuid();
            var category = "TestCategory";
            var until = DateTime.UtcNow.AddMinutes(30);

            await dataAccess.Subscribe(subscriber, category, until);

            var until2 = until.AddHours(1);
            await dataAccess.Subscribe(subscriber, category, until2);

            subscriptions = GetTableContent("Subscriptions").ToSubscriptions();

            Assert.AreEqual(1, subscriptions.Count);

            Assert.AreEqual(subscriber, subscriptions[0].SubscriberId);
            Assert.AreEqual(category, subscriptions[0].Category);
            AssertSqlDbDateTime(until2, subscriptions[0].ValidUntil);
        }

        /// <summary>
        /// Proves that unsafe schema is not allowed.
        /// </summary>
        [TestMethod]
        public async Task Subscribe_ThrowsIfSchemaContainsUnsafe()
        {
            var subscriber = Guid.NewGuid();
            var category = "TestCategory";
            var until = DateTime.UtcNow.AddMinutes(30);
            var action = new Func<Task>(() => dataAccess.Subscribe(subscriber, category, until));
            await ActionThrowsIfSchemaContainsPoisonChars(action);
        }

        /// <summary>
        /// proves that the subscriber ID cannot be empty.
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public async Task Subscribe_ThrowsIfSubscriberIdIsGuidEmpty()
        {
            await dataAccess.Subscribe(Guid.Empty, "TestCategory", DateTime.UtcNow.AddMinutes(30));
        }
    }
}
