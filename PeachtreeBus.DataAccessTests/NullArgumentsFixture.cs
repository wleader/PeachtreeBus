using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using PeachtreeBus.Data;
using PeachtreeBus.DatabaseSharing;
using PeachtreeBus.Tests;
using System;
using System.Threading.Tasks;

namespace PeachtreeBus.DataAccessTests;

[TestClass]
public class NullArgumentsFixture
{
    private DapperDataAccess dataAccess = default!;
    private Mock<ISharedDatabase> sharedDatabase = default!;
    private Mock<IDbSchemaConfiguration> schemaConfig = default!;
    private Mock<ILogger<DapperDataAccess>> log = default!;

    [TestInitialize]
    public void Initialize()
    {
        sharedDatabase = new();
        schemaConfig = new();
        log = new();

        dataAccess = new(
            sharedDatabase.Object,
            schemaConfig.Object,
            log.Object);
    }

    [TestMethod]
    public async Task Given_NullSagaData_When_Update_Then_Throws()
    {
        await Assert.ThrowsExceptionAsync<ArgumentNullException>(() =>
            dataAccess.UpdateSagaData(null!, TestData.DefaultSagaName));
    }

    [TestMethod]
    public async Task Given_NullSubscribedMessage_When_FailMessage_Then_Throws()
    {
        await Assert.ThrowsExceptionAsync<ArgumentNullException>(() =>
            dataAccess.FailMessage(null!));
    }

    [TestMethod]
    public async Task Given_NullQueueMessage_When_FailMessage_Then_Throws()
    {
        await Assert.ThrowsExceptionAsync<ArgumentNullException>(() =>
            dataAccess.FailMessage(null!, TestData.DefaultQueueName));
    }

    [TestMethod]
    public async Task Given_NullSubscribedMessage_When_CompleteMessage_Then_Throws()
    {
        await Assert.ThrowsExceptionAsync<ArgumentNullException>(() =>
            dataAccess.CompleteMessage(null!));
    }

    [TestMethod]
    public async Task Given_NullQueueMessage_When_CompletedMessage_Then_Throws()
    {
        await Assert.ThrowsExceptionAsync<ArgumentNullException>(() =>
            dataAccess.CompleteMessage(null!, TestData.DefaultQueueName));
    }

    [TestMethod]
    public async Task Given_NullSagaData_When_InsertSagaData_Then_Throws()
    {
        await Assert.ThrowsExceptionAsync<ArgumentNullException>(() =>
            dataAccess.InsertSagaData(null!, TestData.DefaultSagaName));
    }

    [TestMethod]
    public async Task Given_NullQueueMessage_When_Update_Then_Throws()
    {
        await Assert.ThrowsExceptionAsync<ArgumentNullException>(() =>
            dataAccess.UpdateMessage(null!, TestData.DefaultQueueName));
    }

    [TestMethod]
    public async Task Given_NullSubscribedMessage_When_Update_Then_Throws()
    {
        await Assert.ThrowsExceptionAsync<ArgumentNullException>(() =>
            dataAccess.UpdateMessage(null!));
    }

    [TestMethod]
    public async Task Given_NullQueueMessage_When_AddMessage_Then_Thows()
    {
        await Assert.ThrowsExceptionAsync<ArgumentNullException>(() =>
            dataAccess.AddMessage(null!, TestData.DefaultQueueName));
    }

    [TestMethod]
    public async Task Given_NullSubscribedMessage_When_AddMessage_Then_Thows()
    {
        await Assert.ThrowsExceptionAsync<ArgumentNullException>(() =>
            dataAccess.AddMessage(null!));
    }

    [TestMethod]
    public async Task Given_NullMessage_When_Publish_Then_Throws()
    {
        await Assert.ThrowsExceptionAsync<ArgumentNullException>(() =>
            dataAccess.Publish(null!, TestData.DefaultCategory));
    }
}
