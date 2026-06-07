using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Threading.Tasks;
using PeachtreeBus.Data;

namespace PeachtreeBus.DataAccessTests;

public abstract class CleanQueueFailedFixture : BusDataAccessFixtureBase
{
    private long _lastId = 1000;

    [TestInitialize]
    public override void Initialize() => base.Initialize();

    [TestCleanup]
    public override void Cleanup() => base.Cleanup();

    private void Given_FailedMessage(DateTime failed)
    {
        TestDataAccess.InsertQueueFailed(new()
        {
            Id = new(_lastId++),
            MessageId = UniqueIdentity.New(),
            Priority =0,
            NotBefore = DateTime.UtcNow.AddDays(-1),
            Enqueued = DateTime.UtcNow.AddDays(-1),
            Completed = null,
            Failed = failed,
            Retries = 0,
            Headers = new(),
            Body = new("{}"),
        });
    }

    private void Given_CountFailedMessage(int count, DateTime failed) =>
        Repeat(()=> Given_FailedMessage(failed), count);

    [TestMethod]
    [DataRow(10,10,0, DisplayName = "Clean All")]
    [DataRow(10,5,5, DisplayName = "Clean Some")]
    public async Task Given_CountMessages_When_CleanQueueFailed_Then_TableHasCount(
        int messageCount, int cleanCount, int remaining)
    {
        Given_CountFailedMessage(messageCount, DateTime.UtcNow.AddDays(-1));
        var olderThan = DateTime.UtcNow;
        var deletedCount = await BusDataAccess.CleanQueueFailed(TestConfig.DefaultQueue, olderThan, cleanCount);
        Assert.AreEqual(cleanCount, deletedCount);
        TestDataAccess.Then_TableHasCount(TestConfig.QueueFailed,remaining);
    }

    [TestMethod]
    public async Task Given_RecentMessages_When_CleanQueueFailed_Then_RecentNotDeleted()
    {
        Given_CountFailedMessage(10, DateTime.UtcNow);
        var olderThan = DateTime.UtcNow.AddMinutes(-5);

        var deletedCount = await BusDataAccess.CleanQueueFailed(TestConfig.DefaultQueue, olderThan, 10);
        Assert.AreEqual(0, deletedCount);
        TestDataAccess.Then_TableHasCount(TestConfig.QueueFailed, 10);
    }

    /// <summary>
    /// Proves that young rows are not deleted.
    /// </summary>
    /// <returns></returns>
    [TestMethod]
    public async Task CleanQueueFailed_HandlesMix()
    {
        Given_CountFailedMessage(3, DateTime.UtcNow.AddDays(-1));
        Given_CountFailedMessage(7, DateTime.UtcNow);
        var olderThan = DateTime.UtcNow.AddMinutes(-5);

        var deletedCount = await BusDataAccess.CleanQueueFailed(TestConfig.DefaultQueue, olderThan, 10);
        Assert.AreEqual(3, deletedCount);
        TestDataAccess.Then_TableHasCount(TestConfig.QueueFailed, 7);
    }
}