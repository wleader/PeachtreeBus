using Microsoft.VisualStudio.TestTools.UnitTesting;
using PeachtreeBus.Core.Tests;
using PeachtreeBus.Subscriptions;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace PeachtreeBus.DataAccessTests;

public abstract class SubscriptionMessageGetPendingFixture : BusDataAccessFixtureBase
{
    [TestInitialize]
    public override Task Initialize() => base.Initialize();

    [TestCleanup]
    public override Task Cleanup() => base.Cleanup();

    [TestMethod]
    public async Task GetPendingSubscriptionMessage_DoesNotReturnDelayedMessage()
    {
        // Add one message;
        var testMessage = TestData.CreateSubscribedData(
            notBefore: DateTime.UtcNow.AddHours(1));
        await TestDataAccess.InsertSubscribedPending(testMessage);
        await Task.Delay(10); // wait for the rows to be ready
        var actual = await BusDataAccess.GetPendingSubscribed(testMessage.SubscriberId);
        Assert.IsNull(actual);
    }

    [TestMethod]
    public async Task GetPendingSubscriptionMessage_DoesNotReturnLocked()
    {
        // Add one message;
        var testMessage = TestData.CreateSubscribedData();
        await TestDataAccess.InsertSubscribedPending(testMessage);
        await Task.Delay(10); // wait for the rows to be ready

        // lock the subscribed message.
        using var pending = TestDataAccess.LockRows<SubscribedData>(TestConfig.SubscribedPending);

        // check that the locked row can not be fetched.
        var actual = await BusDataAccess.GetPendingSubscribed(testMessage.SubscriberId);
        Assert.IsNull(actual);
    }

    [TestMethod]
    public async Task GetPendingSubscriptionMessage_DoesReturnDelayedAfterWait()
    {
        // Add one message;
        var testMessage = TestData.CreateSubscribedData(
            notBefore: DateTime.UtcNow.AddMilliseconds(200));
        await TestDataAccess.InsertSubscribedPending(testMessage);
        await Task.Delay(10); // wait for the rows to be ready
        var actual = await BusDataAccess.GetPendingSubscribed(testMessage.SubscriberId);
        Assert.IsNull(actual);
        await Task.Delay(400);
        actual = await BusDataAccess.GetPendingSubscribed(testMessage.SubscriberId);
        Assert.IsNotNull(actual);
        DataAssert.AreEqual(testMessage, actual);
    }

    [TestMethod]
    public async Task GetPendingSubscriptionMessage_GetsMessage()
    {
        // Add one message;
        var testMessage = TestData.CreateSubscribedData();
        await TestDataAccess.InsertSubscribedPending(testMessage);

        await Task.Delay(10); // wait for the rows to be ready

        var actual = await BusDataAccess.GetPendingSubscribed(testMessage.SubscriberId);
        Assert.IsNotNull(actual);
        DataAssert.AreEqual(testMessage, actual);
    }

    [TestMethod]
    public async Task GetPendingSubscriptionMessage_LocksTheMessage()
    {
        // Add two messages;
        var testMessage1 = TestData.CreateSubscribedData();
        await TestDataAccess.InsertSubscribedPending(testMessage1);
        var testMessage2 = TestData.CreateSubscribedData();
        await TestDataAccess.InsertSubscribedPending(testMessage2);

        await Task.Delay(10); // wait for the rows to be ready

        // get a message and leave the transaction open.
        BusDataAccess.BeginTransaction();
        try
        {
            var actual = await BusDataAccess.GetPendingSubscribed(testMessage1.SubscriberId);
            Assert.IsNotNull(actual, "Did not read a message back.");

            using var data = TestDataAccess.LockRows<SubscribedData>(TestConfig.SubscribedPending);
            var unlockedMessages = data.Data;

            Assert.AreEqual(1, unlockedMessages.Count, "Wrong number of unlocked messages.");
            Assert.AreNotEqual(testMessage1.Id, testMessage2.Id, "Test Messages have the same ID.");
            Assert.IsFalse(unlockedMessages.Any(m => m.Id == actual.Id), $"Locked message {actual.Id} found in unlocked messages {unlockedMessages[0].Id}");
        }
        finally
        {
            BusDataAccess.RollbackTransaction();
        }
    }

    [TestMethod]
    public async Task GetPendingSubscriptionMessage_ReturnsHigherPriorityMessage()
    {
        var subscriber = SubscriberId.New();

        var lowMessage = TestData.CreateSubscribedData(
            priority: 1,
            notBefore: DateTime.UtcNow.AddMinutes(-2),
            subscriberId: subscriber);
        await TestDataAccess.InsertSubscribedPending(lowMessage);

        var highMessage = TestData.CreateSubscribedData(
            priority: 2,
            notBefore: DateTime.UtcNow.AddMinutes(-1),
            subscriberId: subscriber);
        await TestDataAccess.InsertSubscribedPending(highMessage);

        await Task.Delay(10); // wait for the rows to be ready

        var actual = await BusDataAccess.GetPendingSubscribed(subscriber);
        Assert.IsNotNull(actual);
        DataAssert.AreEqual(highMessage, actual);
    }

    [TestMethod]
    public async Task GetPendingSubscriptionMessage_ThrowsIfSubscriberIsGuidEmpty()
    {
        await Assert.ThrowsExactlyAsync<SubscriberIdException>(() =>
            BusDataAccess.GetPendingSubscribed(TestData.UnintializedSubscriberId));
    }
}