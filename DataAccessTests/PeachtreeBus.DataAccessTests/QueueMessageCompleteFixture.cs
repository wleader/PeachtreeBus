using Microsoft.VisualStudio.TestTools.UnitTesting;
using PeachtreeBus.Core.Tests;
using PeachtreeBus.Data;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace PeachtreeBus.DataAccessTests;

public abstract class QueueMessageCompleteFixture : BusDataAccessFixtureBase
{
    [TestInitialize]
    public override void Initialize() => base.Initialize();

    [TestCleanup]
    public override void Cleanup() => base.Cleanup();

    [TestMethod]
    public async Task CompleteMessage_InsertsIntoCompleteTable()
    {
        // Add two messages;
        var testMessage1 = TestData.CreateQueueData();
        testMessage1.Id = await BusDataAccess.AddMessage(testMessage1, TestConfig.DefaultQueue);
        var testMessage2 = TestData.CreateQueueData();
        testMessage2.Id = await BusDataAccess.AddMessage(testMessage2, TestConfig.DefaultQueue);
        await Task.Delay(10); // wait for the rows to be ready

        // get and complete a message.
        var messageToComplete = await BusDataAccess.GetPendingQueued(TestConfig.DefaultQueue);
        Assert.IsNotNull(messageToComplete);
        messageToComplete.Completed = DateTime.UtcNow;
        await BusDataAccess.CompleteMessage(messageToComplete, TestConfig.DefaultQueue);
        await Task.Delay(10); // wait for the rows to be ready

        // Check that it ended up in the completed table.
        var completed = TestDataAccess.GetQueuedCompleted();
        Assert.AreEqual(1, completed.Count);
        DataAssert.AreEqual(messageToComplete, completed[0]);
    }

    [TestMethod]
    public async Task CompleteMessage_DeletesFromPendingTable()
    {
        // Add two messages;
        var testMessage1 = TestData.CreateQueueData();
        testMessage1.Id = await BusDataAccess.AddMessage(testMessage1, TestConfig.DefaultQueue);
        var testMessage2 = TestData.CreateQueueData();
        testMessage2.Id = await BusDataAccess.AddMessage(testMessage2, TestConfig.DefaultQueue);
        await Task.Delay(10); // wait for the rows to be ready

        var messageToComplete = await BusDataAccess.GetPendingQueued(TestConfig.DefaultQueue);
        Assert.IsNotNull(messageToComplete);
        messageToComplete.Completed = DateTime.UtcNow;
        await BusDataAccess.CompleteMessage(messageToComplete, TestConfig.DefaultQueue);
        await Task.Delay(10); // wait for the rows to be ready

        var pending = TestDataAccess.GetQueuedPending();
        Assert.AreEqual(1, pending.Count);
        Assert.IsFalse(pending.Any(m => m.Id == messageToComplete.Id), "Completed message is still in the pending table.");
    }

    [TestMethod]
    public async Task CompleteMessage_CantMutateFields()
    {
        // Add two messages;
        var testMessage1 = TestData.CreateQueueData();
        testMessage1.Id = await BusDataAccess.AddMessage(testMessage1, TestConfig.DefaultQueue);
        await Task.Delay(10); // wait for the rows to be ready

        // get and complete a message.
        var messageToComplete = await BusDataAccess.GetPendingQueued(TestConfig.DefaultQueue);
        Assert.IsNotNull(messageToComplete);
        messageToComplete.Completed = DateTime.UtcNow;
        // screw with the fields that shouldn't change.
        messageToComplete.Body = new("NewBody");
        messageToComplete.Enqueued = messageToComplete.Enqueued.AddMinutes(1);
        messageToComplete.MessageId = UniqueIdentity.New();

        await BusDataAccess.CompleteMessage(messageToComplete, TestConfig.DefaultQueue);
        await Task.Delay(10); // wait for the rows to be ready

        // Check that it ended up in the completed table.
        var completed = TestDataAccess.GetQueuedCompleted();
        Assert.AreEqual(1, completed.Count);
        var actual = completed.Single(m => m.Id == testMessage1.Id);

        // check the immutable fields are the original values.
        Assert.AreEqual(testMessage1.MessageId, actual.MessageId, "MessageId should not change.");
        DataAssert.AreEqual(testMessage1.Enqueued, actual.Enqueued);
        Assert.AreEqual(testMessage1.Body, actual.Body, "Body should not change.");
    }
}