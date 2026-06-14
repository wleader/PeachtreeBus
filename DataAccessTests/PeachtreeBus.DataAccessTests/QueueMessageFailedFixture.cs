using Microsoft.VisualStudio.TestTools.UnitTesting;
using PeachtreeBus.Core.Tests;
using PeachtreeBus.Data;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace PeachtreeBus.DataAccessTests;

public abstract class QueueMessageFailedFixture : BusDataAccessFixtureBase
{
    [TestInitialize]
    public override Task Initialize() => base.Initialize();

    [TestCleanup]
    public override Task Cleanup() => base.Cleanup();


    /// <summary>
    /// Proves that the message is copied into the failed table.
    /// </summary>
    /// <returns></returns>
    [TestMethod]
    public async Task FailMessage_InsertsIntoFailedTable()
    {
        // Add two messages;
        var testMessage1 = TestData.CreateQueueData();
        testMessage1.Id = await BusDataAccess.AddMessage(testMessage1, TestConfig.DefaultQueue);
        var testMessage2 = TestData.CreateQueueData();
        testMessage2.Id = await BusDataAccess.AddMessage(testMessage2, TestConfig.DefaultQueue);
        await Task.Delay(10); // wait for the rows to be ready

        // get and Fail a message.
        var messageToFail = await BusDataAccess.GetPendingQueued(TestConfig.DefaultQueue);
        Assert.IsNotNull(messageToFail);
        messageToFail.Failed = DateTime.UtcNow;
        await BusDataAccess.FailMessage(messageToFail, TestConfig.DefaultQueue);
        await Task.Delay(10); // wait for the rows to be ready

        // Check that it ended up in the error table.
        var failed = await TestDataAccess.GetQueuedFailed();
        Assert.AreEqual(1, failed.Count);
        DataAssert.AreEqual(messageToFail, failed[0]);
    }

    /// <summary>
    /// Proves that the message is removed from the pending table.
    /// </summary>
    /// <returns></returns>
    [TestMethod]
    public async Task FailMessage_DeletesFromPendingTable()
    {
        // Add two messages;
        var testMessage1 = TestData.CreateQueueData();
        testMessage1.Id = await BusDataAccess.AddMessage(testMessage1, TestConfig.DefaultQueue);
        var testMessage2 = TestData.CreateQueueData();
        testMessage2.Id = await BusDataAccess.AddMessage(testMessage2, TestConfig.DefaultQueue);
        await Task.Delay(10); // wait for the rows to be ready

        var messageToFail = await BusDataAccess.GetPendingQueued(TestConfig.DefaultQueue);
        Assert.IsNotNull(messageToFail);
        messageToFail.Failed = DateTime.UtcNow;
        await BusDataAccess.FailMessage(messageToFail, TestConfig.DefaultQueue);
        await Task.Delay(10); // wait for the rows to be ready

        var pending = await TestDataAccess.GetQueuedPending();
        Assert.AreEqual(1, pending.Count);
        Assert.IsFalse(pending.Any(m => m.Id == messageToFail.Id), "Failed message is still in the pending table.");
    }

    /// <summary>
    /// proves that fields that should not change can't change.
    /// </summary>
    /// <returns></returns>
    [TestMethod]
    public async Task FailMessage_CantMutateFields()
    {
        // Add two messages;
        var testMessage1 = TestData.CreateQueueData();
        testMessage1.Id = await BusDataAccess.AddMessage(testMessage1, TestConfig.DefaultQueue);
        await Task.Delay(10); // wait for the rows to be ready

        // get and fail a message.
        var messageToFail = await BusDataAccess.GetPendingQueued(TestConfig.DefaultQueue);
        Assert.IsNotNull(messageToFail);
        messageToFail.Failed = DateTime.UtcNow;
        // screw with the fields that shouldn't change.
        messageToFail.Body = new("NewBody");
        messageToFail.Enqueued = messageToFail.Enqueued.AddMinutes(1);
        messageToFail.MessageId = UniqueIdentity.New();

        await BusDataAccess.FailMessage(messageToFail, TestConfig.DefaultQueue);
        await Task.Delay(10); // wait for the rows to be ready

        // Check that it ended up in the completed table.
        var failed = await TestDataAccess.GetQueuedFailed();
        Assert.AreEqual(1, failed.Count);
        var actual = failed.Single(m => m.Id == testMessage1.Id);

        // check the immutable fields are the original values.
        Assert.AreEqual(testMessage1.MessageId, actual.MessageId, "MessageId should not change.");
        DataAssert.AreEqual(testMessage1.Enqueued, actual.Enqueued);
        Assert.AreEqual(testMessage1.Body, actual.Body, "Body should not change.");
    }
}