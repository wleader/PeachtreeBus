using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Threading.Tasks;
using PeachtreeBus.Data;
using PeachtreeBus.Subscriptions;

namespace PeachtreeBus.DataAccessTests;

public abstract class CleanSubscribedCompletedFixture : BusDataAccessFixtureBase
{
    private long _lastId = 1000;

    [TestInitialize]
    public override void Initialize() => base.Initialize();

    [TestCleanup]
    public override void Cleanup() => base.Cleanup();

    private void Given_CompletedMessage(DateTime completed)
    {
        TestDataAccess.InsertSubscribedCompleted(new()
        {
            Id = new(_lastId++),
            SubscriberId = SubscriberId.New(),
            ValidUntil = DateTime.UtcNow.AddDays(1),
            MessageId = UniqueIdentity.New(),
            Priority = 0,
            NotBefore = DateTime.UtcNow.AddDays(-1),
            Enqueued = DateTime.UtcNow.AddDays(-1),
            Completed = completed,
            Failed = null,
            Retries = 0,
            Headers = new(),
            Body = new("{}"),
            Topic = new("Topic"),
        });
    }

    private void Given_CountCompletedMessages(int count, DateTime completed) =>
    Repeat(() => Given_CompletedMessage(completed), count);

    [TestMethod]
    [DataRow(10,10, 0, DisplayName = "Clean All")]
    [DataRow(10,5, 5, DisplayName = "Clean Some")]
    public async Task Given_CompletedMessage_When_CleanSubscribedCompleted_Then_RowsAreCleaned(
        int messageCount, int cleanCount, int remaining)
    {
        Given_CountCompletedMessages(messageCount, DateTime.UtcNow.AddDays(-1));
        var olderThan = DateTime.UtcNow;

        var deletedCount = await BusDataAccess.CleanSubscribedCompleted(olderThan, cleanCount);
        Assert.AreEqual(cleanCount, deletedCount);

        TestDataAccess.Then_TableHasCount(TestConfig.SubscribedCompleted, remaining);
    }

    [TestMethod]
    public async Task Given_RecentMessages_When_CleanSubscribedCompleted_Then_RecentMessagesAreNotDeleted()
    {
        Given_CountCompletedMessages(10, DateTime.UtcNow);
        var olderThan = DateTime.UtcNow.AddMinutes(-5);
        var deletedCount = await BusDataAccess.CleanSubscribedCompleted(olderThan, 10);
        Assert.AreEqual(0, deletedCount);
        TestDataAccess.Then_TableHasCount(TestConfig.SubscribedCompleted, 10);
    }

    [TestMethod]
    public async Task CleanSubscribedCompleted_HandlesMix()
    {
        Given_CountCompletedMessages(3, DateTime.UtcNow.AddDays(-1));
        Given_CountCompletedMessages(7, DateTime.UtcNow);
        var olderThan = DateTime.UtcNow.AddMinutes(-5);
        var deletedCount = await BusDataAccess.CleanSubscribedCompleted(olderThan, 10);
        Assert.AreEqual(3, deletedCount);
        TestDataAccess.Then_TableHasCount(TestConfig.SubscribedCompleted, 7);
    }
}