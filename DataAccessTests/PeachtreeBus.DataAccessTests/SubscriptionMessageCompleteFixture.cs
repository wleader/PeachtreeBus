using Microsoft.VisualStudio.TestTools.UnitTesting;
using PeachtreeBus.Core.Tests;
using PeachtreeBus.Data;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace PeachtreeBus.DataAccessTests;

public abstract class SubscriptionMessageCompleteFixture : BusDataAccessFixtureBase
{
    [TestInitialize]
    public override Task Initialize() => base.Initialize();

    [TestCleanup]
    public override Task Cleanup() => base.Cleanup();

    [TestMethod]
    public async Task CompleteMessage_CantMutateFields()
    {
        var testMessage1 = TestData.CreateSubscribedData();
        await TestDataAccess.InsertSubscribedPending(testMessage1);
        await Task.Delay(10); // wait for the rows to be ready

        // get and complete a message.
        var messageToComplete = await BusDataAccess.GetPendingSubscribed(testMessage1.SubscriberId);
        Assert.IsNotNull(messageToComplete);
        messageToComplete.Completed = DateTime.UtcNow;
        // screw with the fields that shouldn't change.
        messageToComplete.Body = new("NewBody");
        messageToComplete.Enqueued = messageToComplete.Enqueued.AddMinutes(1);
        messageToComplete.MessageId = UniqueIdentity.New();

        await BusDataAccess.CompleteMessage(messageToComplete);
        await Task.Delay(10); // wait for the rows to be ready

        // Check that it ended up in the completed table.
        var completed = await TestDataAccess.GetSubscribedCompleted();
        Assert.AreEqual(1, completed.Count);
        var actual = completed.Single(m => m.Id == testMessage1.Id);

        // check the immutable fields are the original values.
        Assert.AreEqual(testMessage1.MessageId, actual.MessageId, "MessageId should not change.");
        DataAssert.AreEqual(testMessage1.Enqueued, actual.Enqueued);
        Assert.AreEqual(testMessage1.Body, actual.Body, "Body should not change.");
    }

    [TestMethod]
    public async Task CompleteMessage_DeletesFromPendingTable()
    {
        var expected1 = TestData.CreateSubscribedData(
            validUntil: DateTime.UtcNow.AddMinutes(-1));
        await TestDataAccess.InsertSubscribedPending(expected1);

        await TestDataAccess.Then_TableHasCount(TestConfig.SubscribedPending, 1);

        await BusDataAccess.CompleteMessage(expected1);

        await TestDataAccess.Then_TableIsEmpty(TestConfig.SubscribedPending);
    }

    [TestMethod]
    public async Task CompleteMessage_InsertsIntoCompleteTable()
    {
        await TestDataAccess.Then_TableIsEmpty(TestConfig.SubscribedPending);
        await TestDataAccess.Then_TableIsEmpty(TestConfig.SubscribedCompleted);

        var expected1 = TestData.CreateSubscribedData(
            validUntil: DateTime.UtcNow.AddMinutes(-1));
        await TestDataAccess.InsertSubscribedPending(expected1);

        await BusDataAccess.CompleteMessage(expected1);

        var completed = await TestDataAccess.GetSubscribedCompleted();

        Assert.AreEqual(1, completed.Count);

        var actual1 = completed.Single(s => s.Id == expected1.Id);
        Assert.IsTrue(actual1.Completed.HasValue);
        expected1.Completed = actual1.Completed;
        DataAssert.AreEqual(expected1, actual1);
    }
}