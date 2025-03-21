using Microsoft.VisualStudio.TestTools.UnitTesting;
using PeachtreeBus.Abstractions.Tests;
using PeachtreeBus.Data;
using PeachtreeBus.Subscriptions;
using PeachtreeBus.Tests;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace PeachtreeBus.DataAccessTests;

[TestClass]
public class PublishFixture : DapperDataAccessFixtureBase
{
    private readonly SubscriberId Subscriber1 = SubscriberId.New();
    private readonly SubscriberId Subscriber2 = SubscriberId.New();
    private readonly SubscriberId Subscriber3 = SubscriberId.New();
    private readonly UtcDateTime Until = DateTime.UtcNow.AddHours(1);

    private SubscribedData SubscribedData = null!;

    [TestInitialize]
    public override void TestInitialize()
    {
        base.TestInitialize();
        SubscribedData = TestData.CreateSubscribedData();
    }

    [TestCleanup]
    public override void TestCleanup()
    {
        base.TestCleanup();
    }

    [TestMethod]
    public async Task Given_NoSubscribersForTopic_When_Publish_Then_NoRowsAreAdded()
    {
        // have some subscribers for another topic.
        await dataAccess.Subscribe(Subscriber1, TestData.DefaultTopic2, Until);
        await dataAccess.Subscribe(Subscriber2, TestData.DefaultTopic2, Until);
        var subscriptions = GetSubscriptions();
        Assert.AreEqual(2, subscriptions.Count);

        var count = await dataAccess.Publish(SubscribedData, TestData.DefaultTopic);

        Assert.AreEqual(0, CountRowsInTable(SubscribedPendingTable));
        Assert.AreEqual(0, count);
    }

    [TestMethod]
    public async Task Given_Subscribers_And_MultipleCategories_When_Publish_CorrectRowsAreAdded()
    {
        await dataAccess.Subscribe(Subscriber1, TestData.DefaultTopic, Until);
        await dataAccess.Subscribe(Subscriber1, TestData.DefaultTopic2, Until);
        await dataAccess.Subscribe(Subscriber2, TestData.DefaultTopic, Until);
        await dataAccess.Subscribe(Subscriber2, TestData.DefaultTopic2, Until);
        await dataAccess.Subscribe(Subscriber3, TestData.DefaultTopic2, Until);

        var count = await dataAccess.Publish(SubscribedData, TestData.DefaultTopic);
        Assert.AreEqual(2, count);

        var messages = GetSubscribedPending();
        Assert.AreEqual(2, messages.Count);

        var actual1 = messages.Single(m => m.SubscriberId == Subscriber1);
        AssertPublishedEquals(SubscribedData, actual1);
        var actual2 = messages.Single(m => m.SubscriberId == Subscriber2);
        AssertPublishedEquals(SubscribedData, actual2);

        // Subscriber 3 isn't subscribed to Topic 1, so shouldn't have any messages.
        Assert.IsFalse(messages.Any(m => m.SubscriberId == Subscriber3));
    }
}
