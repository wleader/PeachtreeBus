using Microsoft.VisualStudio.TestTools.UnitTesting;
using PeachtreeBus.Core.Tests;
using PeachtreeBus.Subscriptions;
using System.Threading.Tasks;

namespace PeachtreeBus.DataAccessTests;

[TestClass]
public class EstimateSubscribedPendingFixture : DapperDataAccessFixtureBase
{
    private readonly SubscriberId SubscriberId = SubscriberId.New();

    [TestInitialize]
    public override void TestInitialize()
    {
        base.TestInitialize();
    }

    [TestCleanup]
    public override void TestCleanup()
    {
        base.TestCleanup();
    }

    [TestMethod]
    public async Task Given_SubscribedMessagesPending_When_Estimate_ResultIsZero()
    {
        await Given_SubscribedMessagesPending(SubscriberId, 0);

        Assert.AreEqual(0, await dataAccess.EstimateSubscribedPending(SubscriberId));
    }

    [TestMethod]
    [DataRow(1)]
    [DataRow(2)]
    [DataRow(10)]
    public async Task Given_SubscribedMessagesPending_When_Estimate_ResultIsValue(int value)
    {
        await Given_SubscribedMessagesPending(SubscriberId, value);
        await Task.Delay(20); // If SQL is slow to unlock this test is unreliable.
        Assert.AreEqual(value, await dataAccess.EstimateSubscribedPending(SubscriberId));
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
        await Given_SubscribedMessagesPending(SubscriberId, messageCount);

        using var locked = new RowLock(QueuePending, lockCount);
        var actual = await dataAccess.EstimateSubscribedPending(SubscriberId);

        // its actually kind of hard to not count locked rows,
        // and we only have to produce an estimate, not an exact number,
        // So the asserts below do not force a specific value, Just a range.

        Assert.IsTrue(actual > 0, "Expected estimate result greater than zero");
        Assert.IsTrue(actual <= messageCount, $"Expected estimate result no greater than {messageCount}");
    }

    [TestMethod]
    [DataRow(1)]
    [DataRow(5)]
    [DataRow(10)]
    public async Task Given_SubsrcribedMessagesPendingForMultipleIds_When_Estimate_Then_Result(
        int count)
    {
        await Given_SubscribedMessagesPending(SubscriberId, count);
        var secondSubscriber = SubscriberId.New();
        await Given_SubscribedMessagesPending(secondSubscriber, count);

        Assert.AreEqual(count, await dataAccess.EstimateSubscribedPending(SubscriberId));
    }

    private async Task Given_SubscribedMessagesPending(SubscriberId subscriberId, int count)
    {
        for (var i = 0; i < count; i++)
        {
            var testMessage = TestData.CreateSubscribedData(
                subscriberId: subscriberId);
            await InsertSubscribedMessage(testMessage);
        }
    }
}
