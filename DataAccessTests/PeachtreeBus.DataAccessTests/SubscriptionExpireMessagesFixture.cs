using Microsoft.VisualStudio.TestTools.UnitTesting;
using PeachtreeBus.Core.Tests;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace PeachtreeBus.DataAccessTests;

public abstract class SubscriptionExpireMessagesFixture : BusDataAccessFixtureBase
{
    [TestInitialize]
    public override void Initialize() => base.Initialize();

    [TestCleanup]
    public override void Cleanup() => base.Cleanup();

    [TestMethod]
    public async Task ExpireMessages_InsertsIntoFailedTable()
    {
        TestDataAccess.Then_TableIsEmpty(TestConfig.SubscribedPending);
        TestDataAccess.Then_TableIsEmpty(TestConfig.SubscribedFailed);

        var expected1 = TestData.CreateSubscribedData(
            validUntil: DateTime.UtcNow.AddMinutes(-1));
        TestDataAccess.InsertSubscribedPending(expected1);

        var expected2 = TestData.CreateSubscribedData(
            validUntil: DateTime.UtcNow.AddMinutes(-1));
        TestDataAccess.InsertSubscribedPending(expected2);

        await BusDataAccess.ExpireSubscriptionMessages(1000);

        var failed = TestDataAccess.GetSubscribedFailed();
        Assert.AreEqual(2, failed.Count);

        var actual1 = failed.Single(s => s.Id == expected1.Id);
        Assert.IsTrue(actual1.Failed.HasValue);
        expected1.Failed = actual1.Failed;
        DataAssert.AreEqual(expected1, actual1);

        var actual2 = failed.Single(s => s.Id == expected2.Id);
        Assert.IsTrue(actual2.Failed.HasValue);
        expected2.Failed = actual2.Failed;
        DataAssert.AreEqual(expected2, actual2);
    }

    [TestMethod]
    public async Task ExpireMessages_DeletesFromPending()
    {

        var expected1 = TestData.CreateSubscribedData(
            validUntil: DateTime.UtcNow.AddMinutes(-1));
        TestDataAccess.InsertSubscribedPending(expected1);

        var expected2 = TestData.CreateSubscribedData(
            validUntil: DateTime.UtcNow.AddMinutes(-1));
        TestDataAccess.InsertSubscribedPending(expected2);

        TestDataAccess.Then_TableHasCount(TestConfig.SubscribedPending, 2);

        await BusDataAccess.ExpireSubscriptionMessages(1000);

        TestDataAccess.Then_TableIsEmpty(TestConfig.SubscribedPending);
    }

    [TestMethod]
    public async Task ExpireMessage_LimitsToMaxCount()
    {
        var expected1 = TestData.CreateSubscribedData(
            validUntil: DateTime.UtcNow.AddMinutes(-1));
        TestDataAccess.InsertSubscribedPending(expected1);

        var expected2 = TestData.CreateSubscribedData(
            validUntil: DateTime.UtcNow.AddMinutes(-1));
        TestDataAccess.InsertSubscribedPending(expected2);

        TestDataAccess.Then_TableHasCount(TestConfig.SubscribedPending, 2);

        await BusDataAccess.ExpireSubscriptionMessages(1);

        TestDataAccess.Then_TableHasCount(TestConfig.SubscribedPending, 1);

        await BusDataAccess.ExpireSubscriptionMessages(1);

        TestDataAccess.Then_TableIsEmpty(TestConfig.SubscribedPending);
    }
}