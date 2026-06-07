using Microsoft.VisualStudio.TestTools.UnitTesting;
using PeachtreeBus.Core.Tests;
using PeachtreeBus.Data;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace PeachtreeBus.DataAccessTests;

public abstract class SubscriptionMessageFailedFixture : BusDataAccessFixtureBase
{
    [TestInitialize]
    public override void Initialize() => base.Initialize();

    [TestCleanup]
    public override void Cleanup() => base.Cleanup();

    [TestMethod]
    public async Task FailMessage_CantMutateFields()
    {
        var testMessage1 = TestData.CreateSubscribedData();
        TestDataAccess.InsertSubscribedPending(testMessage1);
        await Task.Delay(10); // wait for the rows to be ready

        // get and complete a message.
        var messageToComplete = await BusDataAccess.GetPendingSubscribed(testMessage1.SubscriberId);
        Assert.IsNotNull(messageToComplete);
        messageToComplete.Completed = DateTime.UtcNow;
        // screw with the fields that shouldn't change.
        messageToComplete.Body = new("NewBody");
        messageToComplete.Enqueued = messageToComplete.Enqueued.AddMinutes(1);
        messageToComplete.MessageId = UniqueIdentity.New();

        await BusDataAccess.FailMessage(messageToComplete);
        await Task.Delay(10); // wait for the rows to be ready

        // Check that it ended up in the completed table.
        var failed = TestDataAccess.GetSubscribedFailed();
        Assert.AreEqual(1, failed.Count);
        var actual = failed.Single(m => m.Id == testMessage1.Id);

        // check the immutable fields are the oringal valules.
        Assert.AreEqual(testMessage1.MessageId, actual.MessageId, "MessageId should not change.");
        DataAssert.AreEqual(testMessage1.Enqueued, actual.Enqueued);
        Assert.AreEqual(testMessage1.Body, actual.Body, "Body should not change.");
    }

    [TestMethod]
    public async Task FailMessage_DeletesFromPendingTable()
    {
        var expected1 = TestData.CreateSubscribedData(
            validUntil: DateTime.UtcNow.AddMinutes(-1));
        TestDataAccess.InsertSubscribedPending(expected1);

        TestDataAccess.Then_TableHasCount(TestConfig.SubscribedPending, 1);

        await BusDataAccess.FailMessage(expected1);

        TestDataAccess.Then_TableIsEmpty(TestConfig.SubscribedPending);
    }

    [TestMethod]
    public async Task FailMessage_InsertsIntoFailedTable()
    {
        TestDataAccess.Then_TableIsEmpty(TestConfig.SubscribedPending);
        TestDataAccess.Then_TableIsEmpty(TestConfig.SubscribedFailed);

        var expected1 = TestData.CreateSubscribedData(
            validUntil: DateTime.UtcNow.AddMinutes(-1));
        TestDataAccess.InsertSubscribedPending(expected1);

        await BusDataAccess.FailMessage(expected1);

        var failed = TestDataAccess.GetSubscribedFailed();

        Assert.AreEqual(1, failed.Count);

        var actual1 = failed.Single(s => s.Id == expected1.Id);
        Assert.IsTrue(actual1.Failed.HasValue);
        expected1.Failed = actual1.Failed;
        DataAssert.AreEqual(expected1, actual1);
    }
}