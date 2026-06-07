using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using PeachtreeBus.Core.Tests;
using PeachtreeBus.Core.Tests.Fakes;
using PeachtreeBus.Data;
using PeachtreeBus.DatabaseSharing;
using System;
using System.Threading.Tasks;

namespace PeachtreeBus.DataAccessTests;

[TestClass]
public class NullArgumentsFixture
{
    private MsSqlBusDataAccess _dataAccess = null!;
    private readonly Mock<ISqlSharedDatabase> _sharedDatabase = new();
    private readonly Mock<IBusConfiguration> _schemaConfig = new();
    private readonly Mock<ILogger<MsSqlBusDataAccess>> _log = new();
    private readonly Mock<IDapperMethods> _sqlExecutor = new();
    private readonly FakeBreakerProvider _breakerProvider = new();

    [TestInitialize]
    public void Initialize()
    {
        _sharedDatabase.Reset();
        _schemaConfig.Reset();
        _log.Reset();
        _sqlExecutor.Reset();

        _dataAccess = new(
            _sharedDatabase.Object,
            _schemaConfig.Object,
            _log.Object,
            _sqlExecutor.Object,
            _breakerProvider);
    }

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
