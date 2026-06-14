using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Threading.Tasks;
using PeachtreeBus.Data;
using PeachtreeBus.Subscriptions;

namespace PeachtreeBus.DataAccessTests;

public abstract class CleanSubscribedFailedFixture : BusDataAccessFixtureBase
{
    private long _lastId = 1000;

    [TestInitialize]
    public override Task Initialize() => base.Initialize();

    [TestCleanup]
    public override Task Cleanup() => base.Cleanup();
    
    private Task Given_FailedMessage(DateTime failed)
    {
        return TestDataAccess.InsertSubscribedFailed(new()
        {
            Id = new(_lastId++),
            SubscriberId = SubscriberId.New(),
            ValidUntil = DateTime.UtcNow.AddDays(1),
            MessageId = UniqueIdentity.New(),
            Priority = 0,
            NotBefore = DateTime.UtcNow.AddDays(-1),
            Enqueued = DateTime.UtcNow.AddDays(-1),
            Completed = null,
            Failed = failed,
            Retries = 0,
            Headers = new(),
            Body = new("{}"),
            Topic = new("Topic"),
        });
    }

    private Task Given_CountFailedMessages(int count, DateTime failed) =>
        Repeat(() => Given_FailedMessage(failed), count);

    [TestMethod]
    [DataRow(10, 10, 0, DisplayName = "Clean All")]
    [DataRow(10, 5, 5, DisplayName = "Clean Some")]
    public async Task Given_FailedMessages_When_CleanSubscribedFailed_Then_RowsAreCleaned(
        int messageCount, int cleanCount, int remaining)
    {
        await Given_CountFailedMessages(messageCount, DateTime.UtcNow.AddDays(-1));

        // Its not clear why, but this test is unreliable without checking the created messages.
        await TestDataAccess.Then_TableHasCount(TestConfig.SubscribedFailed, messageCount);
        
        var olderThan = DateTime.UtcNow;
        var deletedCount = await BusDataAccess.CleanSubscribedFailed(olderThan, cleanCount);
        Assert.AreEqual(cleanCount, deletedCount);

        await TestDataAccess.Then_TableHasCount(TestConfig.SubscribedFailed, remaining);
    }

    [TestMethod]
    public async Task Given_RecentMessages_When_CleanSubscribedFailed_Then_RecentMessagesAreNotDeleted()
    {
        await Given_CountFailedMessages(10, DateTime.UtcNow);
        var olderThan = DateTime.UtcNow.AddMinutes(-5);
        var deletedCount = await BusDataAccess.CleanSubscribedFailed(olderThan, 10);
        Assert.AreEqual(0, deletedCount);
        await TestDataAccess.Then_TableHasCount(TestConfig.SubscribedFailed, 10);
    }

    [TestMethod]
    public async Task CleanSubscribedFailed_HandlesMix()
    {
        await Given_CountFailedMessages(3, DateTime.UtcNow.AddDays(-1));
        await Given_CountFailedMessages(7, DateTime.UtcNow);
        var olderThan = DateTime.UtcNow.AddMinutes(-5);
        var deletedCount = await BusDataAccess.CleanSubscribedFailed(olderThan, 10);
        Assert.AreEqual(3, deletedCount);
        await TestDataAccess.Then_TableHasCount(TestConfig.SubscribedFailed, 7);
    }
}