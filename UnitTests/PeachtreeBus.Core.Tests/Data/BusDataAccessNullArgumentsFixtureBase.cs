using System;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using PeachtreeBus.Data;

namespace PeachtreeBus.Core.Tests.Data;

public abstract class BusDataAccessNullArgumentsFixtureBase<TBusDataAccess>
    : BusDataAccessUnitTestBase<TBusDataAccess>
    where TBusDataAccess : IBusDataAccess
{
    [TestMethod]
    public async Task Given_NullSagaData_When_Update_Then_Throws()
    {
        await Assert.ThrowsExactlyAsync<ArgumentNullException>(() =>
            DataAccess.UpdateSagaData(null!, TestData.DefaultSagaName));
    }

    [TestMethod]
    public async Task Given_NullSubscribedMessage_When_FailMessage_Then_Throws()
    {
        await Assert.ThrowsExactlyAsync<ArgumentNullException>(() =>
            DataAccess.FailMessage(null!));
    }

    [TestMethod]
    public async Task Given_NullQueueMessage_When_FailMessage_Then_Throws()
    {
        await Assert.ThrowsExactlyAsync<ArgumentNullException>(() =>
            DataAccess.FailMessage(null!, TestData.DefaultQueueName));
    }

    [TestMethod]
    public async Task Given_NullSubscribedMessage_When_CompleteMessage_Then_Throws()
    {
        await Assert.ThrowsExactlyAsync<ArgumentNullException>(() =>
            DataAccess.CompleteMessage(null!));
    }

    [TestMethod]
    public async Task Given_NullQueueMessage_When_CompletedMessage_Then_Throws()
    {
        await Assert.ThrowsExactlyAsync<ArgumentNullException>(() =>
            DataAccess.CompleteMessage(null!, TestData.DefaultQueueName));
    }

    [TestMethod]
    public async Task Given_NullSagaData_When_InsertSagaData_Then_Throws()
    {
        await Assert.ThrowsExactlyAsync<ArgumentNullException>(() =>
            DataAccess.InsertSagaData(null!, TestData.DefaultSagaName));
    }

    [TestMethod]
    public async Task Given_NullQueueMessage_When_Update_Then_Throws()
    {
        await Assert.ThrowsExactlyAsync<ArgumentNullException>(() =>
            DataAccess.UpdateMessage(null!, TestData.DefaultQueueName));
    }

    [TestMethod]
    public async Task Given_NullSubscribedMessage_When_Update_Then_Throws()
    {
        await Assert.ThrowsExactlyAsync<ArgumentNullException>(() =>
            DataAccess.UpdateMessage(null!));
    }

    [TestMethod]
    public async Task Given_NullQueueMessage_When_AddMessage_Then_Throws()
    {
        await Assert.ThrowsExactlyAsync<ArgumentNullException>(() =>
            DataAccess.AddMessage(null!, TestData.DefaultQueueName));
    }

    [TestMethod]
    public async Task Given_NullMessage_When_Publish_Then_Throws()
    {
        await Assert.ThrowsExactlyAsync<ArgumentNullException>(() =>
            DataAccess.Publish(null!, TestData.DefaultTopic));
    }
}