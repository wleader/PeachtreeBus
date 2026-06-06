using Microsoft.VisualStudio.TestTools.UnitTesting;
using PeachtreeBus.ClassNames;
using PeachtreeBus.Core.Tests;
using PeachtreeBus.Data;
using PeachtreeBus.Subscriptions;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace PeachtreeBus.DataAccessTests;

public abstract class SubscriptionMessageUpdateFixture : BusDataAccessFixtureBase
{
    [TestInitialize]
    public override void Initialize() => base.Initialize();

    [TestCleanup]
    public override void Cleanup() => base.Cleanup();

    /// <summary>
    /// proves that the pending table is updated.
    /// And that the fields that must not change do not change.
    /// </summary>
    /// <returns></returns>
    [TestMethod]
    public async Task Update_UpdatesPendingTable()
    {
        // Add two messages;
        var testMessage1 = TestData.CreateSubscribedData();
        testMessage1.SubscriberId = SubscriberId.New();
        TestDataAccess.InsertSubscribedPending(testMessage1);
        var testMessage2 = TestData.CreateSubscribedData();
        testMessage2.SubscriberId = SubscriberId.New();
        TestDataAccess.InsertSubscribedPending(testMessage2);
        await Task.Delay(10); // wait for the rows to be ready

        // get and update a message.
        var toUpdate = await BusDataAccess.GetPendingSubscribed(testMessage1.SubscriberId);
        Assert.IsNotNull(toUpdate);

        // set changed values
        toUpdate.SubscriberId = SubscriberId.New(); // should never change.
        toUpdate.ValidUntil = toUpdate.ValidUntil.AddDays(1); // should never change
        toUpdate.MessageId = UniqueIdentity.New(); // this should never persist a change.
        toUpdate.Enqueued = toUpdate.Enqueued.AddMinutes(-1); // this should never change.
        toUpdate.Body = new("Changed Body"); // should never change.
        toUpdate.Headers = new() { MessageClass = ClassName.Default };
        toUpdate.NotBefore = toUpdate.NotBefore.AddMinutes(1);
        toUpdate.Completed = DateTime.UtcNow;
        toUpdate.Failed = DateTime.UtcNow;
        toUpdate.Retries = 10;

        await BusDataAccess.UpdateMessage(toUpdate);
        await Task.Delay(10); // wait for the rows to be ready

        // Check that it is still pending
        var pending = TestDataAccess.GetSubscribedPending();
        Assert.AreEqual(2, pending.Count);

        var expectUnchanged = toUpdate.Id == testMessage1.Id ? testMessage2 : testMessage1;
        var changedOriginal = toUpdate.Id != testMessage1.Id ? testMessage2 : testMessage1;

        var actualUnchanged = pending.Single(m => m.Id != toUpdate.Id);
        DataAssert.AreEqual(expectUnchanged, actualUnchanged);

        var actualChanged = pending.Single(m => m.Id == toUpdate.Id);
        // compare the unchanging fields.
        Assert.AreEqual(changedOriginal.Id, actualChanged.Id);
        Assert.AreEqual(changedOriginal.MessageId, actualChanged.MessageId);
        DataAssert.AreEqual(changedOriginal.Enqueued, actualChanged.Enqueued);
        Assert.AreEqual(changedOriginal.Body, actualChanged.Body);
        Assert.AreEqual(changedOriginal.SubscriberId, actualChanged.SubscriberId);
        DataAssert.AreEqual(changedOriginal.ValidUntil, actualChanged.ValidUntil);

        // compare the changeable fields.
        DataAssert.AreEqual(toUpdate.Headers, actualChanged.Headers);
        DataAssert.AreEqual(toUpdate.NotBefore, actualChanged.NotBefore);
        Assert.AreEqual(toUpdate.Retries, actualChanged.Retries);

        // completed and failed will be null for pending messages.            
        DataAssert.AreEqual(null, actualChanged.Completed);
        DataAssert.AreEqual(null, actualChanged.Failed);
    }
}