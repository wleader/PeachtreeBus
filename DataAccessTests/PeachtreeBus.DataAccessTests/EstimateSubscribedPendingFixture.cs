using Microsoft.VisualStudio.TestTools.UnitTesting;
using PeachtreeBus.Core.Tests;
using PeachtreeBus.Subscriptions;
using System.Threading.Tasks;
using PeachtreeBus.Queues;

namespace PeachtreeBus.DataAccessTests;

public abstract class EstimateSubscribedPendingFixture : BusDataAccessFixtureBase
{
    private readonly SubscriberId _subscriberId = SubscriberId.New();

    [TestInitialize]
    public override Task Initialize() => base.Initialize();

    [TestCleanup]
    public override Task Cleanup() => base.Cleanup();

    [TestMethod]
    public async Task Given_SubscribedMessagesPending_When_Estimate_ResultIsZero()
    {
        await TestDataAccess.Then_TableIsEmpty(TestConfig.SubscribedPending);
        Assert.AreEqual(0, await BusDataAccess.EstimateSubscribedPending(_subscriberId));
    }

    [TestMethod]
    [DataRow(1)]
    [DataRow(2)]
    [DataRow(10)]
    public async Task Given_SubscribedMessagesPending_When_Estimate_ResultIsValue(int value)
    {
        await Given_SubscribedMessagesPending(_subscriberId, value);
        await Task.Delay(20); // If SQL is slow to unlock this test is unreliable.
        Assert.AreEqual(value, await BusDataAccess.EstimateSubscribedPending(_subscriberId));
    }

    [TestMethod]
    [DataRow(1, 1)]
    [DataRow(1, 0)]
    [DataRow(10, 1)]
    [DataRow(10, 5)]
    [DataRow(10, 10)]
    public async Task Given_SubscribedMessagesPending_And_RowsAreLock_When_Estimate_Then_Result(
        int messageCount, int lockCount)
    {
        await Given_SubscribedMessagesPending(_subscriberId, messageCount);

        using var locked = TestDataAccess.LockRows<QueueData>(TestConfig.QueuePending, lockCount);
        var actual = await BusDataAccess.EstimateSubscribedPending(_subscriberId);

        // its actually kind of hard to not count locked rows,
        // and we only have to produce an estimate, not an exact number,
        // So the asserts below do not force a specific value, Just a range.

        Assert.IsGreaterThan(0, actual, "Expected estimate result greater than zero");
        Assert.IsLessThanOrEqualTo(messageCount, actual, $"Expected estimate result no greater than {messageCount}");
    }

    [TestMethod]
    [DataRow(1)]
    [DataRow(5)]
    [DataRow(10)]
    public async Task Given_SubscribedMessagesPendingForMultipleIds_When_Estimate_Then_Result(
        int count)
    {
        await Given_SubscribedMessagesPending(_subscriberId, count);
        var secondSubscriber = SubscriberId.New();
        await Given_SubscribedMessagesPending(secondSubscriber, count);

        Assert.AreEqual(count, await BusDataAccess.EstimateSubscribedPending(_subscriberId));
    }

    private Task Given_SubscribedMessagesPending(SubscriberId subscriberId, int count)
        => Repeat(() => Given_SubscribedMessagesPending(subscriberId), count);

    private Task Given_SubscribedMessagesPending(SubscriberId subscriberId)
        => TestDataAccess.InsertSubscribedPending(
            TestData.CreateSubscribedData(subscriberId: subscriberId));
}
