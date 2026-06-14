using Microsoft.VisualStudio.TestTools.UnitTesting;
using PeachtreeBus.Core.Tests;
using PeachtreeBus.Data;
using PeachtreeBus.Subscriptions;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace PeachtreeBus.DataAccessTests;

public abstract class PublishFixture : BusDataAccessFixtureBase
{
    private readonly SubscriberId _subscriber1 = SubscriberId.New();
    private readonly SubscriberId _subscriber2 = SubscriberId.New();
    private readonly SubscriberId _subscriber3 = SubscriberId.New();
    private readonly UtcDateTime _until = DateTime.UtcNow.AddHours(1);
    private readonly SubscribedData _subscribedData = TestData.CreateSubscribedData();

    [TestInitialize]
    public override Task Initialize() => base.Initialize();

    [TestCleanup]
    public override Task Cleanup() => base.Cleanup();


    [TestMethod]
    public async Task Given_NoSubscribersForTopic_When_Publish_Then_NoRowsAreAdded()
    {
        // have some subscribers for another topic.
        await BusDataAccess.Subscribe(_subscriber1, TestData.DefaultTopic2, _until);
        await BusDataAccess.Subscribe(_subscriber2, TestData.DefaultTopic2, _until);
        var subscriptions = await TestDataAccess.GetSubscriptions();
        Assert.AreEqual(2, subscriptions.Count);

        var publishedCount = await BusDataAccess.Publish(_subscribedData, TestData.DefaultTopic);
        Assert.AreEqual(0, publishedCount);
        
        await TestDataAccess.Then_TableIsEmpty(TestConfig.SubscribedPending);
    }

    [TestMethod]
    public async Task Given_Subscribers_And_MultipleCategories_When_Publish_CorrectRowsAreAdded()
    {
        await BusDataAccess.Subscribe(_subscriber1, TestData.DefaultTopic, _until);
        await BusDataAccess.Subscribe(_subscriber1, TestData.DefaultTopic2, _until);
        await BusDataAccess.Subscribe(_subscriber2, TestData.DefaultTopic, _until);
        await BusDataAccess.Subscribe(_subscriber2, TestData.DefaultTopic2, _until);
        await BusDataAccess.Subscribe(_subscriber3, TestData.DefaultTopic2, _until);

        var count = await BusDataAccess.Publish(_subscribedData, TestData.DefaultTopic);
        Assert.AreEqual(2, count);

        var messages = await TestDataAccess.GetSubscribedPending();
        Assert.AreEqual(2, messages.Count);

        var actual1 = messages.Single(m => m.SubscriberId == _subscriber1);
        DataAssert.PublishedEquals(_subscribedData, actual1);
        var actual2 = messages.Single(m => m.SubscriberId == _subscriber2);
        DataAssert.PublishedEquals(_subscribedData, actual2);

        // Subscriber 3 isn't subscribed to Topic 1, so shouldn't have any messages.
        Assert.IsFalse(messages.Any(m => m.SubscriberId == _subscriber3));
    }
}
