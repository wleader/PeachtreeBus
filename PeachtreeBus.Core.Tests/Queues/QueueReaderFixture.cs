﻿using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using PeachtreeBus.ClassNames;
using PeachtreeBus.Core.Tests.Fakes;
using PeachtreeBus.Core.Tests.Sagas;
using PeachtreeBus.Data;
using PeachtreeBus.Errors;
using PeachtreeBus.Queues;
using PeachtreeBus.Sagas;
using PeachtreeBus.Serialization;
using PeachtreeBus.Telemetry;
using System;
using System.Threading.Tasks;
using RetryResult = PeachtreeBus.Errors.RetryResult;

namespace PeachtreeBus.Core.Tests.Queues;

[TestClass]
public class QueueReaderFixture
{
    private QueueReader reader = default!;
    private readonly Mock<IBusDataAccess> dataAccess = new();
    private readonly Mock<ILogger<QueueReader>> log = new();
    private readonly Mock<IMeters> meters = new();
    private readonly Mock<ISerializer> serializer = new();
    private readonly Mock<IQueueFailures> failures = new();
    private readonly Mock<IQueueRetryStrategy> retryStrategy = new();
    private readonly FakeClock clock = new();
    private readonly ClassNameService classNameService = new();

    private QueueContext Context = default!;

    private QueueData GetPendingResult = default!;
    private TestSagaMessage1 DeserializeMessageResult = default!;
    private RetryResult DetermineRetryResult;
    private SerializedData SerializeSagaDataResult;

    [TestInitialize]
    public void TestInitialize()
    {
        clock.Reset();

        // default mock return values.
        DeserializeMessageResult = new();
        DetermineRetryResult = new(true, TimeSpan.Zero);
        SerializeSagaDataResult = TestData.DefaultSagaData;
        GetPendingResult = TestData.CreateQueueData(
            id: new(678890),
            priority: 12,
            headers: new() { MessageClass = typeof(TestSagaMessage1).GetClassName() });
        Context = TestData.CreateQueueContext(
            messageData: TestData.CreateQueueData(
                id: new(12345),
                notBefore: clock.UtcNow));

        dataAccess.Reset();
        log.Reset();
        meters.Reset();
        serializer.Reset();
        failures.Reset();
        retryStrategy.Reset();

        serializer.Setup(s => s.Serialize(It.IsAny<object>(), typeof(TestSagaData)))
            .Returns(() => SerializeSagaDataResult);

        dataAccess.Setup(d => d.GetPendingQueued(TestData.DefaultQueueName))
            .ReturnsAsync(() => GetPendingResult);

        serializer.Setup(s => s.Deserialize(It.IsAny<SerializedData>(), typeof(TestSagaMessage1)))
            .Returns(() => DeserializeMessageResult);

        retryStrategy.Setup(r => r.DetermineRetry(It.IsAny<QueueContext>(), It.IsAny<Exception>(), It.IsAny<FailureCount>()))
            .Returns(() => DetermineRetryResult);

        reader = new QueueReader(
            dataAccess.Object,
            log.Object,
            meters.Object,
            serializer.Object,
            clock,
            failures.Object,
            retryStrategy.Object,
            classNameService);
    }

    [TestMethod]
    public async Task Given_DataAccessUpdateThrows_When_Delay_Throws()
    {
        dataAccess.Setup(d => d.UpdateMessage(It.IsAny<QueueData>(), It.IsAny<QueueName>()))
            .Throws(new ApplicationException());

        await Assert.ThrowsExactlyAsync<ApplicationException>(() =>
            reader.DelayMessage(Context, 1000));
    }

    [TestMethod]
    public async Task When_Delay_Then_DataAccessUpdate()
    {
        var expectedTime = Context.Data.NotBefore.AddMilliseconds(1000);

        dataAccess.Setup(x => x.UpdateMessage(Context.Data, Context.SourceQueue))
            .Callback((QueueData m, QueueName n) =>
            {
                Assert.AreEqual(expectedTime, m.NotBefore);
            })
            .Returns(Task.CompletedTask);

        await reader.DelayMessage(Context, 1000);

        dataAccess.Verify(x => x.UpdateMessage(Context.Data, Context.SourceQueue), Times.Once);
        Assert.AreEqual(1, dataAccess.Invocations.Count);
    }

    [TestMethod]
    public async Task Given_SagaComplete_When_SaveSaga_Then_DeletesCompleteSaga()
    {
        var expectedKey = Context.SagaKey;
        var testSaga = new TestSaga { SagaComplete = true };

        await reader.SaveSaga(testSaga, Context);

        dataAccess.Verify(d => d.DeleteSagaData(testSaga.SagaName, expectedKey), Times.Once);
    }

    [TestMethod]
    public async Task Given_NoExistingSagaData_When_SaveSaga_Then_InsertsNewSagaData()
    {
        Context.SagaKey = TestData.DefaultSagaKey;
        Context.SagaData = null;

        var testSaga = new TestSaga { SagaComplete = false };

        dataAccess.Setup(d => d.InsertSagaData(It.IsAny<SagaData>(), testSaga.SagaName))
            .Callback((SagaData d, SagaName n) =>
            {
                Assert.AreSame(Context.SagaData, d);
                Assert.AreEqual(TestData.DefaultSagaData, d.Data);
                Assert.AreEqual(TestData.DefaultSagaKey, d.Key);
                Assert.AreEqual(clock.UtcNow, d.MetaData.Started.Value);
                Assert.AreEqual(clock.UtcNow, d.MetaData.LastMessageTime.Value);
            })
            .Returns(Task.FromResult<Identity>(new(1)));

        await reader.SaveSaga(testSaga, Context);

        Assert.IsNotNull(Context.SagaData);
        dataAccess.Verify(d => d.InsertSagaData(Context.SagaData, testSaga.SagaName), Times.Once);
        Assert.AreEqual(1, dataAccess.Invocations.Count);
    }

    [TestMethod]
    public async Task Given_ExistingSagaData_When_SaveSaga_Then_UpdatesExistingSagaData()
    {
        var sagaDataId = new Identity(100);

        Context.SagaKey = TestData.DefaultSagaKey;
        Context.SagaData = new()
        {
            Key = Context.SagaKey,
            Data = new("DataToBeReplaced"),
            Id = sagaDataId,
            SagaId = UniqueIdentity.New(),
            MetaData = TestData.CreateSagaMetaData(
                lastMessageTime: clock.UtcNow.AddDays(-1)),
            Blocked = false,
        };

        var testSaga = new TestSaga { SagaComplete = false };

        dataAccess.Setup(d => d.UpdateSagaData(Context.SagaData, testSaga.SagaName))
            .Callback((SagaData data, SagaName name) =>
            {
                Assert.AreEqual(clock.UtcNow, data.MetaData.LastMessageTime.Value);
                Assert.AreEqual(TestData.DefaultSagaData, data.Data);
                Assert.AreEqual(TestData.DefaultSagaKey, data.Key);
                Assert.AreEqual(sagaDataId, data.Id);
            });

        await reader.SaveSaga(testSaga, Context);

        dataAccess.Verify(d => d.UpdateSagaData(Context.SagaData, testSaga.SagaName), Times.Once);
        Assert.AreEqual(1, dataAccess.Invocations.Count);
    }

    [TestMethod]
    public async Task Given_GetSagaDataReturnsNull_When_LoadSaga_Then_NewSagaDataIsCreated()
    {
        var testSaga = new TestSaga() { Data = null! };
        Context.SagaKey = TestData.DefaultSagaKey;

        dataAccess.Setup(d => d.GetSagaData(testSaga.SagaName, Context.SagaKey))
            .ReturnsAsync((SagaData)null!);

        await reader.LoadSaga(testSaga, Context);

        serializer.Verify(d => d.Deserialize(It.IsAny<SerializedData>(), typeof(TestSagaData)), Times.Never);
        Assert.IsNotNull(testSaga?.Data);
    }

    [TestMethod]
    public async Task Given_GetSagaDataReturnsValue_When_LoadSaga_Then_DataIsDeserialized()
    {
        var testSaga = new TestSaga { };

        Context.SagaKey = TestData.DefaultSagaKey;
        var sagaData = new SagaData()
        {
            Blocked = false,
            Data = TestData.DefaultSagaData,
            Key = Context.SagaKey,
            SagaId = UniqueIdentity.New(),
            MetaData = TestData.CreateSagaMetaData(),
        };
        dataAccess.Setup(d => d.GetSagaData(testSaga.SagaName, Context.SagaKey))
            .ReturnsAsync(sagaData);

        var data = new TestSagaData();
        serializer.Setup(s => s.Deserialize(sagaData.Data, typeof(TestSagaData))).Returns(data);

        await reader.LoadSaga(testSaga, Context);

        Assert.AreSame(data, testSaga.Data);
    }

    [TestMethod]
    public async Task Given_SagaBlocked_When_LoadSaga_Then_DataPropertyIsNull()
    {
        Context.SagaKey = TestData.DefaultSagaKey;

        var testSaga = new TestSaga { Data = default! };

        var sagaData = new SagaData
        {
            Blocked = true,
            Data = TestData.DefaultSagaData,
            Key = TestData.DefaultSagaKey,
            MetaData = TestData.CreateSagaMetaData(),
            SagaId = UniqueIdentity.New(),
        };

        dataAccess.Setup(d => d.GetSagaData(testSaga.SagaName, Context.SagaKey))
            .ReturnsAsync(sagaData);

        await reader.LoadSaga(testSaga, Context);

        serializer.Verify(s => s.Deserialize(sagaData.Data, typeof(TestSagaData)), Times.Never);
        Assert.IsNull(testSaga?.Data);
        Assert.IsTrue(Context.SagaData!.Blocked);
    }

    [TestMethod]
    public async Task Given_RetryStrategyReturnsRetry_When_MessageFails_Then_MessageIsUpdated()
    {
        var delay = TimeSpan.FromSeconds(5);
        DetermineRetryResult = new(true, delay);
        Context.Data.Retries = 0;
        UtcDateTime expectedNotBefore = clock.UtcNow.Add(delay);

        var exception = new ApplicationException();

        dataAccess.Setup(c => c.UpdateMessage(Context.Data, Context.SourceQueue))
            .Callback((QueueData m, QueueName n) =>
            {
                Assert.AreEqual(1, m.Retries);
                Assert.AreEqual(expectedNotBefore, m.NotBefore);
                Assert.AreSame(Context.Headers, m.Headers);
                Assert.AreEqual(exception.ToString(), m.Headers?.ExceptionDetails);
            })
            .Returns(Task.CompletedTask);

        await reader.Fail(Context, exception);

        dataAccess.Verify(d => d.UpdateMessage(Context.Data, Context.SourceQueue), Times.Once);
        Assert.AreEqual(1, dataAccess.Invocations.Count);
    }

    [TestMethod]
    public async Task Given_RetryStrategyReturnsFail_When_Fail_Then_MessagesIsFailed()
    {
        DetermineRetryResult = new(false, TimeSpan.FromHours(1));
        var expectedMessageId = Context.Data.Id;
        var exception = new ApplicationException();

        dataAccess.Setup(c => c.FailMessage(Context.Data, Context.SourceQueue))
            .Callback((QueueData m, QueueName n) =>
            {
                Assert.AreEqual(expectedMessageId, m.Id);
                Assert.AreEqual(exception.ToString(), m.Headers.ExceptionDetails);
            })
            .Returns(Task.CompletedTask);

        await reader.Fail(Context, new ApplicationException());

        dataAccess.Verify(d => d.FailMessage(Context.Data, Context.SourceQueue), Times.Once);
        Assert.AreEqual(1, dataAccess.Invocations.Count);
        meters.Verify(c => c.FailMessage(), Times.Once);
    }

    [TestMethod]
    public async Task When_Complete_Then_Completes()
    {
        dataAccess.Setup(d => d.CompleteMessage(Context.Data, Context.SourceQueue))
            .Callback((QueueData m, QueueName n) =>
            {
                Assert.AreEqual(clock.UtcNow, m.Completed);
            })
            .Returns(Task.CompletedTask);

        await reader.Complete(Context);

        meters.Verify(c => c.CompleteMessage(), Times.Once);
        dataAccess.Verify(d => d.CompleteMessage(Context.Data, Context.SourceQueue), Times.Once);
        Assert.AreEqual(1, dataAccess.Invocations.Count);
    }

    [TestMethod]
    public async Task Given_GetPendingReturnsNull_When_GetNext_Then_ReturnsNull()
    {
        dataAccess.Setup(d => d.GetPendingQueued(TestData.DefaultQueueName))
            .ReturnsAsync((QueueData)null!);

        Assert.IsNull(await reader.GetNext(TestData.DefaultQueueName));
    }

    [TestMethod]
    public async Task Given_ValidData_When_GetNext_Then_ContextSetup()
    {
        var context = await reader.GetNext(TestData.DefaultQueueName);

        Assert.IsNotNull(context);
        Assert.AreSame(GetPendingResult, context.Data);
        Assert.AreEqual(TestData.DefaultQueueName, context.SourceQueue);
        Assert.IsFalse(context.SagaBlocked);
        Assert.AreSame(DeserializeMessageResult, context.Message);
    }

    [TestMethod]
    public async Task Given_UndeserializableHeaders_When_GetNext_Then_ContextSetup_And_ContextMessageIsNull()
    {
        GetPendingResult.Headers = null!;

        serializer.Setup(s => s.Deserialize(GetPendingResult.Body, typeof(object)))
            .Returns(null!);

        var context = await reader.GetNext(TestData.DefaultQueueName);

        Assert.IsNotNull(context);
        Assert.AreSame(GetPendingResult, context.Data);
        Assert.AreEqual(TestData.DefaultQueueName, context.SourceQueue);
        Assert.IsFalse(context.SagaBlocked);
        Assert.IsNull(context?.Message);
    }

    [TestMethod]
    public async Task Given_UnrecognizedMessageClass_When_GetNext_Then_ContextSetup_And_ContextMessageIsNull()
    {
        GetPendingResult.Headers = new()
        {
            MessageClass = new("PeachtreeBus.Tests.Sagas.TestSagaNotARealMessage, PeachtreeBus.Tests")
        };

        var context = await reader.GetNext(TestData.DefaultQueueName);

        Assert.IsNotNull(context);
        Assert.AreSame(GetPendingResult, context.Data);
        Assert.AreEqual(TestData.DefaultQueueName, context.SourceQueue);
        Assert.IsFalse(context.SagaBlocked);
        Assert.IsNull(context?.Message);
    }

    [TestMethod]
    public async Task Given_UndeserializableBody_When_GetNext_Then_ContextSetup_And_ContextMessageIsNull()
    {
        serializer.Setup(s => s.Deserialize(It.IsAny<SerializedData>(), typeof(TestSagaMessage1)))
            .Throws(new Exception("Test Exception"));

        var context = await reader.GetNext(TestData.DefaultQueueName);

        Assert.IsNotNull(context);
        Assert.AreSame(GetPendingResult, context.Data);
        Assert.AreEqual(TestData.DefaultQueueName, context.SourceQueue);
        Assert.IsFalse(context.SagaBlocked);
        Assert.IsNull(context?.Message);
    }

    [TestMethod]
    public async Task Given_RetryStrategyReturnsFail_When_Fail_Then_ErrorHandlerIsInvoked()
    {
        var context = TestData.CreateQueueContext();

        DetermineRetryResult = new(false, TimeSpan.Zero);

        var exception = new ApplicationException();

        await reader.Fail(context, exception);
        failures.Verify(f => f.Failed(context, context.Message, exception), Times.Once());
    }

    [TestMethod]
    public async Task Given_RetryStrategyReturnsRetry_When_Fail_Then_ErrorHandlerIsNotInvoked()
    {
        var context = TestData.CreateQueueContext();

        var exception = new ApplicationException();

        DetermineRetryResult = new(true, TimeSpan.FromSeconds(5));

        await reader.Fail(context, exception);
        failures.Verify(f => f.Failed(context, context.Message, exception), Times.Never());
    }
}
