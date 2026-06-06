using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Threading.Tasks;
using PeachtreeBus.Core.Tests;

namespace PeachtreeBus.DataAccessTests;

public abstract class EstimateQueuePendingFixture : BusDataAccessFixtureBase
{
    [TestInitialize]
    public override void Initialize() => base.Initialize();

    [TestCleanup]
    public override void Cleanup() => base.Cleanup();

    private Task Given_MessageInQueue() =>
        BusDataAccess.AddMessage(
            TestData.CreateQueueData(notBefore: DateTime.Now.AddMinutes(-1)),
            TestConfig.DefaultQueue);
    
    private Task Given_MessagesInQueue(int count) => Repeat(Given_MessageInQueue, count);

    [TestMethod]
    public async Task Given_NoMessagesInQueue_When_Estimate_ResultIsZero()
    {
        TestDataAccess.Then_TableIsEmpty(TestConfig.QueuePending);
        Assert.AreEqual(0, await BusDataAccess.EstimateQueuePending(TestConfig.DefaultQueue));
    }

    [TestMethod]
    [DataRow(1)]
    [DataRow(2)]
    [DataRow(10)]
    public async Task Given_MessagesInQueue_When_Estimate_ResultIsValue(int value)
    {
        await Given_MessagesInQueue(value);
        await Task.Delay(20); // If SQL is slow to unlock this test is unreliable.
        Assert.AreEqual(value, await BusDataAccess.EstimateQueuePending(TestConfig.DefaultQueue));
    }

    [TestMethod]
    [DataRow(1, 1)]
    [DataRow(1, 0)]
    [DataRow(10, 1)]
    [DataRow(10, 5)]
    [DataRow(10, 10)]
    public async Task Given_MessagesInQueue_And_RowsAreLock_When_Estimate_Then_Result(
        int messageCount, int lockCount)
    {
        await Given_MessagesInQueue(messageCount);

        using var locked = TestDataAccess.LockRows(TestConfig.QueuePending, lockCount);

        var actual = await BusDataAccess.EstimateQueuePending(TestConfig.DefaultQueue);

        // This needs a comment to explain, because why the assertions are what they
        // are is not obvious.
        // First, getting an exact number is harder for SQL to do.
        // Second, if there are multiple processes reading from the queue,
        // there is no guarantee that those messages will still be there an instant later.
        // The receiver has to tolerate the count being higher than the actual number
        // of messages available.
        // There is no requirement that the estimate be exactly right,
        // and thus no assert here to enforce that.

        Assert.IsGreaterThan(0, actual, "Expected estimate result greater than zero");
        Assert.IsLessThanOrEqualTo(messageCount, actual, $"Expected estimate result no greater than {messageCount}");
    }
}
