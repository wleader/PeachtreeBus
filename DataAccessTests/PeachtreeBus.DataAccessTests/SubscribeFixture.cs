using Microsoft.VisualStudio.TestTools.UnitTesting;
using PeachtreeBus.Core.Tests;
using PeachtreeBus.Subscriptions;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace PeachtreeBus.DataAccessTests;

public abstract class SubscribeFixture : BusDataAccessFixtureBase
{
    [TestInitialize]
    public override void Initialize() => base.Initialize();

    [TestCleanup]
    public override void Cleanup() => base.Cleanup();

    [TestMethod]
    public async Task Subscribe_AddsRowWhenSubscriberAndTopicDoNotExist()
    {
        var subscriptions = TestDataAccess.GetSubscriptions();
        Assert.AreEqual(0, subscriptions.Count);

        var subscriber = SubscriberId.New();
        var topic = new Topic("TestTopic");
        var until = DateTime.UtcNow.AddMinutes(30);

        await BusDataAccess.Subscribe(subscriber, topic, until);

        subscriptions = TestDataAccess.GetSubscriptions();

        Assert.AreEqual(1, subscriptions.Count);
        Assert.AreNotEqual(0, subscriptions[0].Id.Value);
        Assert.AreEqual(subscriber, subscriptions[0].SubscriberId);
        Assert.AreEqual(topic, subscriptions[0].Topic);
        DataAssert.AreEqual(until, subscriptions[0].ValidUntil);
    }

    [TestMethod]
    public async Task Subscribe_AddsRowWhenSubscriberExistsAndTopicDoesNot()
    {
        var subscriptions = TestDataAccess.GetSubscriptions();
        Assert.AreEqual(0, subscriptions.Count);

        var subscriber = SubscriberId.New();
        var topic = new Topic("TestTopic");
        var until = DateTime.UtcNow.AddMinutes(30);

        await BusDataAccess.Subscribe(subscriber, topic, until);

        var topic2 = new Topic("TestTopic2");
        await BusDataAccess.Subscribe(subscriber, topic2, until);

        subscriptions = TestDataAccess.GetSubscriptions();
        Assert.AreEqual(2, subscriptions.Count);

        subscriptions.ForEach(s => Assert.AreEqual(subscriber, s.SubscriberId));
        subscriptions.ForEach(s => DataAssert.AreEqual(until, s.ValidUntil));

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
        var subscriptions = TestDataAccess.GetSubscriptions();
        Assert.AreEqual(0, subscriptions.Count);

        var subscriber = SubscriberId.New();
        var topic = new Topic("TestTopic");
        var until = DateTime.UtcNow.AddMinutes(30);

        await BusDataAccess.Subscribe(subscriber, topic, until);

        var subscriber2 = SubscriberId.New();
        await BusDataAccess.Subscribe(subscriber2, topic, until);

        subscriptions = TestDataAccess.GetSubscriptions();
        Assert.AreEqual(2, subscriptions.Count);

        subscriptions.ForEach(s => Assert.AreEqual(topic, s.Topic));
        subscriptions.ForEach(s => DataAssert.AreEqual(until, s.ValidUntil));

        var subscribers = subscriptions.Select(s => s.SubscriberId).ToList();
        Assert.IsTrue(subscribers.Contains(subscriber));
        Assert.IsTrue(subscribers.Contains(subscriber2));
    }

    [TestMethod]
    public async Task Subscribe_UpdatesWhenSubscriberAndTopicAlreadyExist()
    {
        var subscriptions = TestDataAccess.GetSubscriptions();
        Assert.AreEqual(0, subscriptions.Count);

        var subscriber = SubscriberId.New();
        var topic = new Topic("TestTopic");
        var until = DateTime.UtcNow.AddMinutes(30);

        await BusDataAccess.Subscribe(subscriber, topic, until);

        var until2 = until.AddHours(1);
        await BusDataAccess.Subscribe(subscriber, topic, until2);

        subscriptions = TestDataAccess.GetSubscriptions();

        Assert.AreEqual(1, subscriptions.Count);

        Assert.AreEqual(subscriber, subscriptions[0].SubscriberId);
        Assert.AreEqual(topic, subscriptions[0].Topic);
        DataAssert.AreEqual(until2, subscriptions[0].ValidUntil);
    }

    [TestMethod]
    public async Task Given_UninitializedSubscriberId_When_Subscribe_Then_Throws()
    {
        await Assert.ThrowsExactlyAsync<SubscriberIdException>(() =>
            BusDataAccess.Subscribe(TestData.UnintializedSubscriberId, TestData.DefaultTopic, DateTime.UtcNow.AddMinutes(30)));
    }
}