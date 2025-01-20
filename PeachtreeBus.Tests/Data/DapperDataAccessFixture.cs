using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using PeachtreeBus.Data;
using PeachtreeBus.DatabaseSharing;
using PeachtreeBus.Queues;
using PeachtreeBus.Sagas;
using System;
using System.Threading.Tasks;

namespace PeachtreeBus.Tests.Data;

[TestClass]
public class DapperDataAccessFixture
{
    private DapperDataAccess dataAccess = default!;
    private Mock<ISharedDatabase> sharedDatabase = default!;
    private Mock<IDbSchemaConfiguration> schemaConfig = default!;
    private Mock<ILogger<DapperDataAccess>> log = default!;

    private static readonly QueueName QueueName = new("TestQueue");
    private static readonly SagaName SagaName = new("TestSaga");

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
            dataAccess.Update((SagaData)null!, SagaName));
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
            dataAccess.FailMessage(null!, QueueName));
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
            dataAccess.CompleteMessage(null!, QueueName));
    }

    [TestMethod]
    [DataRow((string)null!)]
    [DataRow("")]
    public async Task Given_SagaKey_When_GetSagaData_Then_Throws(string sagaKey)
    {
        await Assert.ThrowsExceptionAsync<ArgumentException>(() =>
            dataAccess.GetSagaData(SagaName, sagaKey));
    }

    [TestMethod]
    [DataRow((string)null!)]
    [DataRow("")]
    public async Task Given_SagaKey_When_DeleteSagaData_Then_Throws(string sagaKey)
    {
        await Assert.ThrowsExceptionAsync<ArgumentException>(() =>
            dataAccess.DeleteSagaData(SagaName, sagaKey));
    }

    [TestMethod]
    [DataRow((string)null!)]
    [DataRow("")]
    public async Task Given_SagaKey_When_InsertSagaData_Then_Throws(string sagaKey)
    {
        var sagaData = new SagaData()
        {
            Key = sagaKey,
            SagaId = Guid.NewGuid(),
        };
        await Assert.ThrowsExceptionAsync<ArgumentException>(() =>
            dataAccess.Insert(sagaData, SagaName));
    }

    [TestMethod]
    public async Task Given_NullSagaData_When_InsertSagaData_Then_Throws()
    {
        await Assert.ThrowsExceptionAsync<ArgumentNullException>(() =>
            dataAccess.Insert(null!, SagaName));
    }

    [TestMethod]
    public async Task Given_EmptySagaId_When_InsertSagaData_Then_Throws()
    {
        var sagaData = new SagaData()
        {
            Key = "SagaKey",
            SagaId = Guid.Empty,
        };
        await Assert.ThrowsExceptionAsync<ArgumentException>(() =>
            dataAccess.Insert(sagaData, SagaName));
    }

    [TestMethod]
    public async Task Given_QueueMessageNull_When_Update_Then_Throws()
    {
        await Assert.ThrowsExceptionAsync<ArgumentNullException>(() =>
            dataAccess.Update(null!, QueueName));
    }

    [TestMethod]
    public async Task Given_SubscribedMessageNull_When_Update_Then_Throws()
    {
        await Assert.ThrowsExceptionAsync<ArgumentNullException>(() =>
            dataAccess.Update(null!));
    }

    [TestMethod]
    public async Task Given_QueueMessageNull_When_AddMessage_Then_Thow()
    {
        await Assert.ThrowsExceptionAsync<ArgumentNullException>(() =>
            dataAccess.AddMessage(null!, QueueName));
    }

    [TestMethod]
    public async Task Given_SubscribedMessageNull_When_AddMessage_Then_Thow()
    {
        await Assert.ThrowsExceptionAsync<ArgumentNullException>(() =>
            dataAccess.AddMessage(null!));
    }


    [TestMethod]
    public async Task Given_QueueMessageIdEmpty_When_AddMessage_Then_Thow()
    {
        await Assert.ThrowsExceptionAsync<ArgumentNullException>(() =>
            dataAccess.AddMessage(new() { MessageId = Guid.Empty }, QueueName));
    }

    [TestMethod]
    public async Task Given_SubscribedMessageMessageIdEmpty_When_AddMessage_Then_Thow()
    {
        await Assert.ThrowsExceptionAsync<ArgumentNullException>(() =>
            dataAccess.AddMessage(new() { MessageId = Guid.Empty }));
    }
}
