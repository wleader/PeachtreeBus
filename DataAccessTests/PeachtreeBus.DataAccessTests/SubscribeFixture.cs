using Microsoft.VisualStudio.TestTools.UnitTesting;
using PeachtreeBus.Abstractions.Tests;
using PeachtreeBus.Core.Tests;
using PeachtreeBus.Subscriptions;
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
        public async Task Subscribe_AddsRowWhenSubscriberAndTopicDoNotExist()
        {
            var subscriptions = GetSubscriptions();
            Assert.AreEqual(0, subscriptions.Count);

            var subscriber = SubscriberId.New();
            var topic = new Topic("TestTopic");
            var until = DateTime.UtcNow.AddMinutes(30);

            await dataAccess.Subscribe(subscriber, topic, until);

            subscriptions = GetSubscriptions();

            Assert.AreEqual(1, subscriptions.Count);
            Assert.AreNotEqual(0, subscriptions[0].Id.Value);
            Assert.AreEqual(subscriber, subscriptions[0].SubscriberId);
            Assert.AreEqual(topic, subscriptions[0].Topic);
            AssertSqlDbDateTime(until, subscriptions[0].ValidUntil);
        }

        /// <summary>
        /// proves the row is added when other rows for the same subscriber exists, but a row
        /// for the topic does not.
        /// </summary>
        /// <returns></returns>
        [TestMethod]
        public async Task Subscribe_AddsRowWhenSubscriberExistsAndTopicDoesNot()
        {
            var subscriptions = GetSubscriptions();
            Assert.AreEqual(0, subscriptions.Count);

            var subscriber = SubscriberId.New();
            var topic = new Topic("TestTopic");
            var until = DateTime.UtcNow.AddMinutes(30);

            await dataAccess.Subscribe(subscriber, topic, until);

            var topic2 = new Topic("TestTopic2");
            await dataAccess.Subscribe(subscriber, topic2, until);

            subscriptions = GetSubscriptions();
            Assert.AreEqual(2, subscriptions.Count);

            subscriptions.ForEach(s => Assert.AreEqual(subscriber, s.SubscriberId));
            subscriptions.ForEach(s => AssertSqlDbDateTime(until, s.ValidUntil));

            var categores = subscriptions.Select(s => s.Topic).ToList();
            Assert.IsTrue(categores.Contains(topic));
            Assert.IsTrue(categores.Contains(topic2));
        }

        /// <summary>
        /// Proves that the row is added when other subscribers are using the same topic.
        /// </summary>
        /// <returns></returns>
        [TestMethod]
        public async Task Subscribe_AddsRowWhenSubscriberDoesNotExistAndTopicExists()
        {
            var subscriptions = GetSubscriptions();
            Assert.AreEqual(0, subscriptions.Count);

            var subscriber = SubscriberId.New();
            var topic = new Topic("TestTopic");
            var until = DateTime.UtcNow.AddMinutes(30);

            await dataAccess.Subscribe(subscriber, topic, until);

            var subscriber2 = SubscriberId.New();
            await dataAccess.Subscribe(subscriber2, topic, until);

            subscriptions = GetSubscriptions();
            Assert.AreEqual(2, subscriptions.Count);

            subscriptions.ForEach(s => Assert.AreEqual(topic, s.Topic));
            subscriptions.ForEach(s => AssertSqlDbDateTime(until, s.ValidUntil));

            var subscribers = subscriptions.Select(s => s.SubscriberId).ToList();
            Assert.IsTrue(subscribers.Contains(subscriber));
            Assert.IsTrue(subscribers.Contains(subscriber2));
        }

        /// <summary>
        /// Proves the row is updated when a row for the subscriber and topic already exists.
        /// </summary>
        /// <returns></returns>
        [TestMethod]
        public async Task Subscribe_UpdatesWhenSubscriberAndTopicAlreadyExist()
        {
            var subscriptions = GetSubscriptions();
            Assert.AreEqual(0, subscriptions.Count);

            var subscriber = SubscriberId.New();
            var topic = new Topic("TestTopic");
            var until = DateTime.UtcNow.AddMinutes(30);

            await dataAccess.Subscribe(subscriber, topic, until);

            var until2 = until.AddHours(1);
            await dataAccess.Subscribe(subscriber, topic, until2);

            subscriptions = GetSubscriptions();

            Assert.AreEqual(1, subscriptions.Count);

            Assert.AreEqual(subscriber, subscriptions[0].SubscriberId);
            Assert.AreEqual(topic, subscriptions[0].Topic);
            AssertSqlDbDateTime(until2, subscriptions[0].ValidUntil);
        }

        [TestMethod]
        public async Task Given_UninitializedSubscriberId_When_Subscribe_Then_Throws()
        {
            await Assert.ThrowsExceptionAsync<SubscriberIdException>(() =>
                dataAccess.Subscribe(TestData.UnintializedSubscriberId, TestData.DefaultTopic, DateTime.UtcNow.AddMinutes(30)));
        }
    }
}
