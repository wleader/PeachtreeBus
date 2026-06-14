using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Threading.Tasks;

namespace PeachtreeBus.DataAccessTests;

public abstract class ManagementCancelSubscribedMessageFixture : ManagementDataAccessFixtureBase
{
    [TestInitialize]
    public override Task Initialize() => base.Initialize();

    [TestCleanup]
    public override Task Cleanup() => base.Cleanup();

    [TestMethod]
    public async Task InsertsIntoTargetTable()
    {
        var s1 = await CreatePendingSubscribed();
        await CreatePendingSubscribed();

        await BusDataAccess.CancelPendingSubscribedMessage(s1.Id);

        var failed = await BusDataAccess.GetFailedSubscribedMessages(0, int.MaxValue);
        Assert.AreEqual(1, failed.Count);
        Assert.AreEqual(1, failed.Count);
        Assert.AreEqual(s1.MessageId, failed[0].MessageId);
        DataAssert.AreEqual(s1.Headers, failed[0].Headers);
        DataAssert.AreEqual(s1.ValidUntil, failed[0].ValidUntil);
        Assert.AreEqual(s1.Body, failed[0].Body);
        DataAssert.AreEqual(s1.Enqueued, failed[0].Enqueued);
        Assert.AreEqual(0, failed[0].Retries);
        Assert.AreEqual(null, failed[0].Completed);
        DataAssert.AreEqual(DateTime.UtcNow, failed[0].Failed, 5000);
        Assert.AreEqual(s1.SubscriberId, failed[0].SubscriberId);
        DataAssert.AreEqual(s1.NotBefore, failed[0].NotBefore);
    }

    [TestMethod]
    public async Task DeletesFromSourceTable()
    {
        var s1 = await CreatePendingSubscribed();
        var s2 = await CreatePendingSubscribed();

        await BusDataAccess.CancelPendingSubscribedMessage(s1.Id);

        var pending = await BusDataAccess.GetPendingSubscribedMessages(0, int.MaxValue);
        Assert.AreEqual(1, pending.Count);
        Assert.AreEqual(s2.Id, pending[0].Id);
    }
}