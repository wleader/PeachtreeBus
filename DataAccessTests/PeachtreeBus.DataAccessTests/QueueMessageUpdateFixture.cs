using Microsoft.VisualStudio.TestTools.UnitTesting;
using PeachtreeBus.ClassNames;
using PeachtreeBus.Core.Tests;
using PeachtreeBus.Data;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace PeachtreeBus.DataAccessTests;

public abstract class QueueMessageUpdateFixture : BusDataAccessFixtureBase
{
    [TestInitialize]
    public override Task Initialize() => base.Initialize();

    [TestCleanup]
    public override Task Cleanup() => base.Cleanup();

    [TestMethod]
    public async Task Update_UpdatesPendingTable()
    {
        // Add two messages;
        var testMessage1 = TestData.CreateQueueData();
        testMessage1.Id = await BusDataAccess.AddMessage(testMessage1, TestConfig.DefaultQueue);
        var testMessage2 = TestData.CreateQueueData();
        testMessage2.Id = await BusDataAccess.AddMessage(testMessage2, TestConfig.DefaultQueue);
        await Task.Delay(10); // wait for the rows to be ready

        // get and update a message.
        var toUpdate = await BusDataAccess.GetPendingQueued(TestConfig.DefaultQueue);
        Assert.IsNotNull(toUpdate);
        // set changed values
        toUpdate.MessageId = UniqueIdentity.New(); // this should never persist a change.
        toUpdate.Enqueued = toUpdate.Enqueued.AddMinutes(-1); // this should never change.
        toUpdate.Body = new("Changed Body"); // should never change.
        toUpdate.Headers = new() { MessageClass = ClassName.Default };
        toUpdate.NotBefore = toUpdate.NotBefore.AddMinutes(1);
        toUpdate.Completed = DateTime.UtcNow;
        toUpdate.Failed = DateTime.UtcNow;
        toUpdate.Retries = 10;

        await BusDataAccess.UpdateMessage(toUpdate, TestConfig.DefaultQueue);
        await Task.Delay(10); // wait for the rows to be ready

        // Check that it ended up in the error table.
        var pending = await TestDataAccess.GetQueuedPending();
        Assert.AreEqual(2, pending.Count);

        var expectUnchanged = toUpdate.Id == testMessage1.Id ? testMessage2 : testMessage1;
        var changedOriginal = toUpdate.Id != testMessage1.Id ? testMessage2 : testMessage1;

        var actualUnchanged = pending.Single(m => m.Id != toUpdate.Id);
        DataAssert.AreEqual(expectUnchanged, actualUnchanged);

        var actualChanged = pending.Single(m => m.Id == toUpdate.Id);
        // compare the un-changing fields.
        Assert.AreEqual(changedOriginal.Id, actualChanged.Id);
        Assert.AreEqual(changedOriginal.MessageId, actualChanged.MessageId);
        DataAssert.AreEqual(changedOriginal.Enqueued, actualChanged.Enqueued);
        Assert.AreEqual(changedOriginal.Body, actualChanged.Body);
        // compare the changeable fields.
        DataAssert.AreEqual(toUpdate.Headers, actualChanged.Headers);
        DataAssert.AreEqual(toUpdate.NotBefore, actualChanged.NotBefore);
        Assert.AreEqual(toUpdate.Retries, actualChanged.Retries);

        // completed and failed will be null for pending messages.            
        DataAssert.AreEqual(null, actualChanged.Completed);
        DataAssert.AreEqual(null, actualChanged.Failed);
    }
}