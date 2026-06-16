using System;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using PeachtreeBus.Data;

namespace PeachtreeBus.Core.Tests.Data;

public abstract class BusDataAccessNullArgumentsFixtureBase<TBusDataAccess>
    where TBusDataAccess : IBusDataAccess
{
    private TBusDataAccess _dataAccess = default!;
    
    [TestInitialize]
    public void Initialize()
    {
        _dataAccess = CreateDataAccess();
    }

    protected abstract TBusDataAccess CreateDataAccess();
    
    [TestMethod]
    public async Task Given_NullSagaData_When_Update_Then_Throws()
    {
        await Assert.ThrowsExactlyAsync<ArgumentNullException>(() =>
            _dataAccess.UpdateSagaData(null!, TestData.DefaultSagaName));
    }

    [TestMethod]
    public async Task Given_NullSubscribedMessage_When_FailMessage_Then_Throws()
    {
        await Assert.ThrowsExactlyAsync<ArgumentNullException>(() =>
            _dataAccess.FailMessage(null!));
    }

    [TestMethod]
    public async Task Given_NullQueueMessage_When_FailMessage_Then_Throws()
    {
        await Assert.ThrowsExactlyAsync<ArgumentNullException>(() =>
            _dataAccess.FailMessage(null!, TestData.DefaultQueueName));
    }

    [TestMethod]
    public async Task Given_NullSubscribedMessage_When_CompleteMessage_Then_Throws()
    {
        await Assert.ThrowsExactlyAsync<ArgumentNullException>(() =>
            _dataAccess.CompleteMessage(null!));
    }

    [TestMethod]
    public async Task Given_NullQueueMessage_When_CompletedMessage_Then_Throws()
    {
        await Assert.ThrowsExactlyAsync<ArgumentNullException>(() =>
            _dataAccess.CompleteMessage(null!, TestData.DefaultQueueName));
    }

    [TestMethod]
    public async Task Given_NullSagaData_When_InsertSagaData_Then_Throws()
    {
        await Assert.ThrowsExactlyAsync<ArgumentNullException>(() =>
            _dataAccess.InsertSagaData(null!, TestData.DefaultSagaName));
    }

    [TestMethod]
    public async Task Given_NullQueueMessage_When_Update_Then_Throws()
    {
        await Assert.ThrowsExactlyAsync<ArgumentNullException>(() =>
            _dataAccess.UpdateMessage(null!, TestData.DefaultQueueName));
    }

    [TestMethod]
    public async Task Given_NullSubscribedMessage_When_Update_Then_Throws()
    {
        await Assert.ThrowsExactlyAsync<ArgumentNullException>(() =>
            _dataAccess.UpdateMessage(null!));
    }

    [TestMethod]
    public async Task Given_NullQueueMessage_When_AddMessage_Then_Throws()
    {
        await Assert.ThrowsExactlyAsync<ArgumentNullException>(() =>
            _dataAccess.AddMessage(null!, TestData.DefaultQueueName));
    }

    [TestMethod]
    public async Task Given_NullMessage_When_Publish_Then_Throws()
    {
        await Assert.ThrowsExactlyAsync<ArgumentNullException>(() =>
            _dataAccess.Publish(null!, TestData.DefaultTopic));
    }
}