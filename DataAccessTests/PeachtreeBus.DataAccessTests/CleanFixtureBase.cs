using System;
using System.Threading.Tasks;
using Microsoft.SqlServer.Dac.Model;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace PeachtreeBus.DataAccessTests;

public abstract class CleanFixtureBase : BusDataAccessFixtureBase
{
    [TestInitialize]
    public override Task Initialize() => base.Initialize();

    [TestCleanup]
    public override Task Cleanup() => base.Cleanup();
    
    protected abstract Task Given_CountMessages(int count, DateTime finished);
    protected abstract Task<long> When_Clean(int count, DateTime before);
    
    protected abstract Task Then_TableHasCount(int count);
    
    [TestMethod]
    [DataRow(10, 10, 0, DisplayName = "Cleans All")]
    [DataRow(10, 5, 5, DisplayName = "Clean Some")]
    public async Task Given_OldMessages_When_Clean_Then_TableHasCount(
        int givenCount, int cleanupCount, int remainingCount)
    {
        var now = DateTime.UtcNow;
        await Given_CountMessages(givenCount, now.AddDays(-1));
        var deletedCount = await When_Clean(cleanupCount, now);
        Assert.AreEqual(cleanupCount, deletedCount);
        await Then_TableHasCount(remainingCount);
    }

    [TestMethod]
    public async Task Given_RecentMessages_When_Clean_Then_MessagesNotDeleted()
    {
        await Given_CountMessages(10, DateTime.UtcNow);

        var olderThan = DateTime.UtcNow.AddMinutes(-5);
        var deletedCount = await When_Clean(10, olderThan);

        Assert.AreEqual(0, deletedCount);
        await Then_TableHasCount(10);
    }

    [TestMethod]
    public async Task Given_OldMessages_And_RecentMessages_When_Clean_Then_OldMessagesAreDeleted()
    {
        await Given_CountMessages(3,  DateTime.UtcNow.AddDays(-1));
        await Given_CountMessages(7, DateTime.UtcNow);
            
        var olderThan = DateTime.UtcNow.AddMinutes(-5);
        var deletedCount = await When_Clean(10, olderThan);
        Assert.AreEqual(3, deletedCount);

        await Then_TableHasCount(7);
    }
}