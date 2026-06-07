using Microsoft.VisualStudio.TestTools.UnitTesting;
using PeachtreeBus.Core.Tests;
using PeachtreeBus.Data;
using System;
using System.Linq;
using System.Threading.Tasks;
using PeachtreeBus.Queues;

namespace PeachtreeBus.DataAccessTests;

public abstract class QueueMessageGetPendingFixture : BusDataAccessFixtureBase
{
    [TestInitialize]
    public override void Initialize() => base.Initialize();

    [TestCleanup]
    public override void Cleanup() => base.Cleanup();

    [TestMethod]
    public async Task GetPendingQueued_GetsMessage()
    {
        // Add one message;
        var testMessage = TestData.CreateQueueData();
        testMessage.Id = await BusDataAccess.AddMessage(testMessage, TestConfig.DefaultQueue);

        await Task.Delay(10); // wait for the rows to be ready

        var actual = await BusDataAccess.GetPendingQueued(TestConfig.DefaultQueue);
        Assert.IsNotNull(actual);
        DataAssert.AreEqual(testMessage, actual);
    }

    [TestMethod]
    public async Task GetPendingQueued_LocksTheMessage()
    {
        // Add two messages;
        var testMessage1 = TestData.CreateQueueData();
        testMessage1.Id = await BusDataAccess.AddMessage(testMessage1, TestConfig.DefaultQueue);
        var testMessage2 = TestData.CreateQueueData();
        testMessage2.Id = await BusDataAccess.AddMessage(testMessage2, TestConfig.DefaultQueue);

        await Task.Delay(10); // wait for the rows to be ready

        // get a message and leave the transaction open.
        BusDataAccess.BeginTransaction();
        ILockedRows<QueueData>? rowsNotLockedByGetPending = null;
        try
        {
            var actual = await BusDataAccess.GetPendingQueued(TestConfig.DefaultQueue);
            Assert.IsNotNull(actual, "Did not read a message back.");

            rowsNotLockedByGetPending = TestDataAccess.LockRows<QueueData>(TestConfig.QueuePending);
            var unlockedMessages = rowsNotLockedByGetPending.Data;

            Assert.AreEqual(1, unlockedMessages.Count, "Wrong number of unlocked messages.");
            Assert.AreNotEqual(testMessage1.Id, testMessage2.Id, "Test Messages have the same ID.");
            Assert.IsFalse(unlockedMessages.Any(m => m.Id == actual.Id),
            $"Locked message {actual.Id} found in unlocked messages {unlockedMessages[0].Id}");
        }
        finally
        {
            rowsNotLockedByGetPending?.Dispose();
            BusDataAccess.RollbackTransaction();
        }
    }

    [TestMethod]
    public async Task GetPendingQueued_DoesNotReturnLocked()
    {
        // Add one message;
        var testMessage = TestData.CreateQueueData();
        testMessage.Id = await BusDataAccess.AddMessage(testMessage, TestConfig.DefaultQueue);
        await Task.Delay(10); // wait for the rows to be ready

        // lock the whole table.
        using var pending = TestDataAccess.LockRows<QueueData>(TestConfig.QueuePending);

        // check that the locked row can not be fetched.
        var actual = await BusDataAccess.GetPendingQueued(TestConfig.DefaultQueue);
        Assert.IsNull(actual);
    }

    [TestMethod]
    public async Task GetPendingQueued_DoesNotReturnDelayedMessage()
    {
        // Add one message;
        var testMessage = TestData.CreateQueueData();
        testMessage.NotBefore = testMessage.NotBefore.AddHours(1);
        testMessage.Id = await BusDataAccess.AddMessage(testMessage, TestConfig.DefaultQueue);
        await Task.Delay(10); // wait for the rows to be ready
        var actual = await BusDataAccess.GetPendingQueued(TestConfig.DefaultQueue);
        Assert.IsNull(actual);
    }

    [TestMethod]
    public async Task GetPendingQueued_DoesReturnDelayedAfterWait()
    {
        // Add one message;
        var testMessage = TestData.CreateQueueData();
        testMessage.NotBefore = testMessage.NotBefore.AddMilliseconds(200);
        testMessage.Id = await BusDataAccess.AddMessage(testMessage, TestConfig.DefaultQueue);
        await Task.Delay(10); // wait for the rows to be ready
        var actual = await BusDataAccess.GetPendingQueued(TestConfig.DefaultQueue);
        Assert.IsNull(actual);
        await Task.Delay(400);
        actual = await BusDataAccess.GetPendingQueued(TestConfig.DefaultQueue);
        Assert.IsNotNull(actual);
        DataAssert.AreEqual(testMessage, actual);
    }

    [TestMethod]
    public async Task GetPendingQueued_ReturnsHigherPriorityMessage()
    {
        var lowMessage = TestData.CreateQueueData();
        lowMessage.Priority = 1;
        lowMessage.NotBefore = DateTime.UtcNow.AddMinutes(-2);
        lowMessage.Id = await BusDataAccess.AddMessage(lowMessage, TestConfig.DefaultQueue);

        var highMessage = TestData.CreateQueueData();
        highMessage.Priority = 2;
        highMessage.NotBefore = DateTime.UtcNow.AddMinutes(-1);
        highMessage.Id = await BusDataAccess.AddMessage(highMessage, TestConfig.DefaultQueue);

        await Task.Delay(10); // wait for the rows to be ready

        var actual = await BusDataAccess.GetPendingQueued(TestConfig.DefaultQueue);
        Assert.IsNotNull(actual);
        DataAssert.AreEqual(highMessage, actual);
    }
}