using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Threading.Tasks;

namespace PeachtreeBus.DataAccessTests;

[TestClass]
public class EstimateQueuePendingFixture : DapperDataAccessFixtureBase
{
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
    public async Task Given_NoMessagesInQueue_When_Estimate_ResultIsZero()
    {
        await Given_MessagesInQueue(0);

        Assert.AreEqual(0, await dataAccess.EstimateQueuePending(DefaultQueue));
    }

    [TestMethod]
    [DataRow(1)]
    [DataRow(2)]
    [DataRow(10)]
    public async Task Given_MessagesInQueue_When_Estimate_ResultIsValue(int value)
    {
        await Given_MessagesInQueue(value);
        await Task.Delay(20); // If SQL is slow to unlock this test is unreliable.
        Assert.AreEqual(value, await dataAccess.EstimateQueuePending(DefaultQueue));
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

        using var locked = new RowLock(QueuePending, lockCount);

        var actual = await dataAccess.EstimateQueuePending(DefaultQueue);

        // This needs a comment to explain, because why the assertions are what they
        // are is not obvious.
        // first getting an exact number is harder for SQL to do.
        // second if there are multiple processes reading from the queue,
        // there is no guarantee that those messages will still be there an instant later
        // so the reciever has to tolerate the count being higher than the actual number
        // of messages it gets.
        // There is no requirement that the estimate be exactly right,
        // and thus no assert here to enforce that.

        Assert.IsTrue(actual > 0, "Expected estimate result greater than zero");
        Assert.IsTrue(actual <= messageCount, $"Expected estimate result no greater than {messageCount}");
    }
}
