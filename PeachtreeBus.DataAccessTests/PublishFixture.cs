using Microsoft.VisualStudio.TestTools.UnitTesting;
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

    private SubscribedMessage UserMessage = null!;

    [TestInitialize]
    public override void TestInitialize()
    {
        base.TestInitialize();
        UserMessage = TestData.CreateSubscribedMessage();
    }

    [TestCleanup]
    public override void TestCleanup()
    {
        base.TestCleanup();
    }

    [TestMethod]
    public async Task Given_NoSubscribersForCategory_When_Publish_Then_NoRowsAreAdded()
    {
        // have some subscribers for another category.
        await dataAccess.Subscribe(Subscriber1, TestData.DefaultCategory2, Until);
        await dataAccess.Subscribe(Subscriber2, TestData.DefaultCategory2, Until);
        var subscriptions = GetSubscriptions();
        Assert.AreEqual(2, subscriptions.Count);

        var count = await dataAccess.Publish(UserMessage, TestData.DefaultCategory);

        Assert.AreEqual(0, CountRowsInTable(SubscribedPendingTable));
        Assert.AreEqual(0, count);
    }

    [TestMethod]
    public async Task Given_Subscribers_And_MultipleCategories_When_Publish_CorrectRowsAreAdded()
    {
        await dataAccess.Subscribe(Subscriber1, TestData.DefaultCategory, Until);
        await dataAccess.Subscribe(Subscriber1, TestData.DefaultCategory2, Until);
        await dataAccess.Subscribe(Subscriber2, TestData.DefaultCategory, Until);
        await dataAccess.Subscribe(Subscriber2, TestData.DefaultCategory2, Until);
        await dataAccess.Subscribe(Subscriber3, TestData.DefaultCategory2, Until);

        var count = await dataAccess.Publish(UserMessage, TestData.DefaultCategory);
        Assert.AreEqual(2, count);

        var messages = GetSubscribedPending();
        Assert.AreEqual(2, messages.Count);

        var acutal1 = messages.Single(m => m.SubscriberId == Subscriber1);
        AssertPublishedEquals(UserMessage, acutal1);
        var acutal2 = messages.Single(m => m.SubscriberId == Subscriber2);
        AssertPublishedEquals(UserMessage, acutal1);

        // Subscriber 3 isn't subscribed to Category 1, so shouldn't have any messages.
        Assert.IsFalse(messages.Any(m => m.SubscriberId == Subscriber3));
    }
}
