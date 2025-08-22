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
    private DapperDataAccess dataAccess = default!;
    private Mock<ISharedDatabase> sharedDatabase = default!;
    private Mock<IBusConfiguration> schemaConfig = default!;
    private Mock<ILogger<DapperDataAccess>> log = default!;
    private readonly Mock<IDapperMethods> sqlExecutor = new();
    private readonly FakeBreakerProvider breakerProvider = new();

    [TestInitialize]
    public void Initialize()
    {
        sharedDatabase = new();
        schemaConfig = new();
        log = new();
        sqlExecutor.Reset();

        dataAccess = new(
            sharedDatabase.Object,
            schemaConfig.Object,
            log.Object,
            sqlExecutor.Object,
            breakerProvider);
    }

    [TestMethod]
    public async Task Given_NullSagaData_When_Update_Then_Throws()
    {
        await Assert.ThrowsExactlyAsync<ArgumentNullException>(() =>
            dataAccess.UpdateSagaData(null!, TestData.DefaultSagaName));
    }

    [TestMethod]
    public async Task Given_NullSubscribedMessage_When_FailMessage_Then_Throws()
    {
        await Assert.ThrowsExactlyAsync<ArgumentNullException>(() =>
            dataAccess.FailMessage(null!));
    }

    [TestMethod]
    public async Task Given_NullQueueMessage_When_FailMessage_Then_Throws()
    {
        await Assert.ThrowsExactlyAsync<ArgumentNullException>(() =>
            dataAccess.FailMessage(null!, TestData.DefaultQueueName));
    }

    [TestMethod]
    public async Task Given_NullSubscribedMessage_When_CompleteMessage_Then_Throws()
    {
        await Assert.ThrowsExactlyAsync<ArgumentNullException>(() =>
            dataAccess.CompleteMessage(null!));
    }

    [TestMethod]
    public async Task Given_NullQueueMessage_When_CompletedMessage_Then_Throws()
    {
        await Assert.ThrowsExactlyAsync<ArgumentNullException>(() =>
            dataAccess.CompleteMessage(null!, TestData.DefaultQueueName));
    }

    [TestMethod]
    public async Task Given_NullSagaData_When_InsertSagaData_Then_Throws()
    {
        await Assert.ThrowsExactlyAsync<ArgumentNullException>(() =>
            dataAccess.InsertSagaData(null!, TestData.DefaultSagaName));
    }

    [TestMethod]
    public async Task Given_NullQueueMessage_When_Update_Then_Throws()
    {
        await Assert.ThrowsExactlyAsync<ArgumentNullException>(() =>
            dataAccess.UpdateMessage(null!, TestData.DefaultQueueName));
    }

    [TestMethod]
    public async Task Given_NullSubscribedMessage_When_Update_Then_Throws()
    {
        await Assert.ThrowsExactlyAsync<ArgumentNullException>(() =>
            dataAccess.UpdateMessage(null!));
    }

    [TestMethod]
    public async Task Given_NullQueueMessage_When_AddMessage_Then_Thows()
    {
        await Assert.ThrowsExactlyAsync<ArgumentNullException>(() =>
            dataAccess.AddMessage(null!, TestData.DefaultQueueName));
    }

    [TestMethod]
    public async Task Given_NullMessage_When_Publish_Then_Throws()
    {
        await Assert.ThrowsExactlyAsync<ArgumentNullException>(() =>
            dataAccess.Publish(null!, TestData.DefaultTopic));
    }
}
