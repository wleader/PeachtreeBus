using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using PeachtreeBus.Data;
using PeachtreeBus.Errors;
using PeachtreeBus.Queues;
using PeachtreeBus.Sagas;
using PeachtreeBus.Serialization;
using PeachtreeBus.Telemetry;
using PeachtreeBus.Tests.Fakes;
using PeachtreeBus.Tests.Sagas;
using System;
using System.Threading.Tasks;
using RetryResult = PeachtreeBus.Errors.RetryResult;

namespace PeachtreeBus.Tests.Queues;

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

    private QueueContext Context = default!;

    private QueueData GetPendingResult = default!;
    private TestSagaMessage1 DeserializeMessageResult = default!;
    private RetryResult DetermineRetryResult;
    private SerializedData SerializeSagaDataResult;

    [TestInitialize]
    public void TestInitialize()
    {
        // default mock return values.
        DeserializeMessageResult = new();
        DetermineRetryResult = new(true, TimeSpan.Zero);
        SerializeSagaDataResult = TestData.DefaultSagaData;
        GetPendingResult = TestData.CreateQueueData(
            id: new(678890),
            priority: 12,
            headers: new(typeof(TestSagaMessage1)));
        Context = TestData.CreateQueueContext(
            messageData: TestData.CreateQueueData(
                id: new(12345),
                notBefore: FakeClock.Instance.UtcNow));

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
            FakeClock.Instance,
            failures.Object,
            retryStrategy.Object);
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
    public async Task Given_SagaComplete_When_SaveSaga_DeletesCompleteSaga()
    {
        var expectedKey = Context.SagaKey;
        var testSaga = new TestSaga { SagaComplete = true };

        await reader.SaveSaga(testSaga, Context);

        dataAccess.Verify(d => d.DeleteSagaData(testSaga.SagaName, expectedKey), Times.Once);
    }

    /// <summary>
    /// Proves Saga Data is inserted when needed.
    /// </summary>
    /// <returns></returns>
    [TestMethod]
    public async Task SaveSaga_InsertsNewSagaData()
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
                Assert.AreEqual(FakeClock.Instance.UtcNow, d.MetaData.Started.Value);
                Assert.AreEqual(FakeClock.Instance.UtcNow, d.MetaData.LastMessageTime.Value);
            })
            .Returns(Task.FromResult<Identity>(new(1)));

        await reader.SaveSaga(testSaga, Context);

        Assert.IsNotNull(Context.SagaData);
        dataAccess.Verify(d => d.InsertSagaData(Context.SagaData, testSaga.SagaName), Times.Once);
        Assert.AreEqual(1, dataAccess.Invocations.Count);
    }

    /// <summary>
    /// Proves Saga Data is updated after a handler.
    /// </summary>
    /// <returns></returns>
    [TestMethod]
    public async Task SaveSaga_UpdatesExistingSagaData()
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
                lastMessageTime: FakeClock.Instance.UtcNow.AddDays(-1)),
            Blocked = false,
        };

        var testSaga = new TestSaga { SagaComplete = false };

        dataAccess.Setup(d => d.UpdateSagaData(Context.SagaData, testSaga.SagaName))
            .Callback((SagaData data, SagaName name) =>
            {
                Assert.AreEqual(FakeClock.Instance.UtcNow, data.MetaData.LastMessageTime.Value);
                Assert.AreEqual(TestData.DefaultSagaData, data.Data);
                Assert.AreEqual(TestData.DefaultSagaKey, data.Key);
                Assert.AreEqual(sagaDataId, data.Id);
            });

        await reader.SaveSaga(testSaga, Context);

        dataAccess.Verify(d => d.UpdateSagaData(Context.SagaData, testSaga.SagaName), Times.Once);
        Assert.AreEqual(1, dataAccess.Invocations.Count);
    }

    /// <summary>
    /// Proves that new saga data is created as needed
    /// </summary>
    /// <returns></returns>
    [TestMethod]
    public async Task LoadSaga_InitializesWhenNew()
    {
        var testSaga = new TestSaga() { Data = null! };
        Context.SagaKey = TestData.DefaultSagaKey;

        dataAccess.Setup(d => d.GetSagaData(testSaga.SagaName, Context.SagaKey))
            .ReturnsAsync((SagaData)null!);

        await reader.LoadSaga(testSaga, Context);

        Assert.IsNotNull(testSaga.Data);
    }

    /// <summary>
    /// Proves that saga data can be deserialized.
    /// </summary>
    /// <returns></returns>
    [TestMethod]
    public async Task LoadSaga_DeserializesExisting()
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

    /// <summary>
    /// Proves that blocked saga data can be handled.
    /// </summary>
    /// <returns></returns>
    [TestMethod]
    public async Task LoadSaga_ReturnsWhenBlocked()
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
        Assert.IsNull(testSaga.Data);
        Assert.IsTrue(Context.SagaData!.Blocked);
    }

    /// <summary>
    /// Proves that Fail updates retry counts.
    /// </summary>
    /// <returns></returns>
    [TestMethod]
    public async Task Given_RetryStrategyReturnsRetry_When_MessageFails_Then_MessageIsUpdated()
    {
        var delay = TimeSpan.FromSeconds(5);
        DetermineRetryResult = new(true, delay);
        Context.Data.Retries = 0;
        UtcDateTime expectedNotBefore = FakeClock.Instance.UtcNow.Add(delay);

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

    /// <summary>
    /// Proves that message is failed after retries.
    /// </summary>
    /// <returns></returns>
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
                Assert.IsNotNull(m.Headers);
                Assert.AreEqual(exception.ToString(), m.Headers.ExceptionDetails);
            })
            .Returns(Task.CompletedTask);

        await reader.Fail(Context, new ApplicationException());

        dataAccess.Verify(d => d.FailMessage(Context.Data, Context.SourceQueue), Times.Once);
        Assert.AreEqual(1, dataAccess.Invocations.Count);
        meters.Verify(c => c.FailMessage(), Times.Once);
    }

    /// <summary>
    /// Proves that messages are compelted.
    /// </summary>
    /// <returns></returns>
    [TestMethod]
    public async Task Complete_Completes()
    {
        dataAccess.Setup(d => d.CompleteMessage(Context.Data, Context.SourceQueue))
            .Callback((QueueData m, QueueName n) =>
            {
                Assert.AreEqual(FakeClock.Instance.UtcNow, m.Completed);
            })
            .Returns(Task.CompletedTask);

        await reader.Complete(Context);

        meters.Verify(c => c.CompleteMessage(), Times.Once);
        dataAccess.Verify(d => d.CompleteMessage(Context.Data, Context.SourceQueue), Times.Once);
        Assert.AreEqual(1, dataAccess.Invocations.Count);
    }

    [TestMethod]
    public async Task GetNext_ReturnsNull()
    {
        dataAccess.Setup(d => d.GetPendingQueued(TestData.DefaultQueueName))
            .ReturnsAsync((QueueData)null!);

        Assert.IsNull(await reader.GetNext(TestData.DefaultQueueName));
    }

    /// <summary>
    /// Proves a good message is handled.
    /// </summary>
    /// <returns></returns>
    [TestMethod]
    public async Task GetNext_GetsGoodMessage()
    {
        var context = await reader.GetNext(TestData.DefaultQueueName);

        Assert.IsNotNull(context?.Data);
        Assert.AreSame(GetPendingResult, context.Data);
        Assert.IsNotNull(context?.Headers);
        Assert.AreEqual(TestData.DefaultQueueName, context.SourceQueue);
        Assert.IsFalse(context.SagaBlocked);
        Assert.AreEqual(GetPendingResult.MessageId, context.MessageId);
        Assert.AreEqual(GetPendingResult.Priority, context.MessagePriority);
        Assert.AreSame(DeserializeMessageResult, context.Message);
    }

    /// <summary>
    /// Proves the behavior when headers cannot deserialize
    /// </summary>
    /// <returns></returns>
    [TestMethod]
    public async Task GetNext_HandlesUndeserializableHeaders()
    {
        GetPendingResult.Headers = null;

        serializer.Setup(s => s.Deserialize(It.IsAny<SerializedData>(), typeof(object)))
            .Returns(null!);

        var context = await reader.GetNext(TestData.DefaultQueueName);

        Assert.IsNotNull(context?.Data);
        Assert.AreSame(GetPendingResult, context.Data);
        Assert.IsNotNull(context?.UserHeaders);
        Assert.AreSame("System.Object", context.MessageClass);
        Assert.AreEqual(TestData.DefaultQueueName, context.SourceQueue);
        Assert.IsFalse(context.SagaBlocked);
        Assert.AreEqual(GetPendingResult.MessageId, context.MessageId);
        Assert.AreEqual(GetPendingResult.Priority, context.MessagePriority);
        Assert.IsNull(context.Message);
    }

    /// <summary>
    /// Proves the behavior when the message type is unrecognized.
    /// </summary>
    /// <returns></returns>
    [TestMethod]
    public async Task GetNext_HandlesUnrecognizedMessageClass()
    {
        GetPendingResult.Headers = new()
        { MessageClass = "PeachtreeBus.Tests.Sagas.TestSagaNotARealMessage, PeachtreeBus.Tests" };

        var context = await reader.GetNext(TestData.DefaultQueueName);

        Assert.IsNotNull(context?.Data);
        Assert.AreSame(GetPendingResult, context.Data);
        Assert.AreEqual(TestData.DefaultQueueName, context.SourceQueue);
        Assert.IsFalse(context.SagaBlocked);
        Assert.AreEqual(GetPendingResult.MessageId, context.MessageId);
        Assert.AreEqual(GetPendingResult.Priority, context.MessagePriority);
        Assert.IsNull(context.Message);
    }

    /// <summary>
    /// Proves a message fails when it can deserialize
    /// </summary>
    /// <returns></returns>
    [TestMethod]
    public async Task GetNext_HandlesUndeserializableMessageBody()
    {
        serializer.Setup(s => s.Deserialize(It.IsAny<SerializedData>(), typeof(TestSagaMessage1)))
            .Throws(new Exception("Test Exception"));

        var context = await reader.GetNext(TestData.DefaultQueueName);

        Assert.IsNotNull(context);
        Assert.AreSame(GetPendingResult, context.Data);
        Assert.AreEqual(TestData.DefaultQueueName, context.SourceQueue);
        Assert.IsFalse(context.SagaBlocked);
        Assert.AreEqual(GetPendingResult.MessageId, context.MessageId);
        Assert.AreEqual(GetPendingResult.Priority, context.MessagePriority);
        Assert.IsNull(context.Message);
    }

    [TestMethod]
    public async Task Given_RetryStrategyReturnsFail_When_Fail_ThenErrorHandlerIsInvoked()
    {
        var context = TestData.CreateQueueContext();

        DetermineRetryResult = new(false, TimeSpan.Zero);

        var exception = new ApplicationException();

        await reader.Fail(context, exception);
        failures.Verify(f => f.Failed(context, context.Message, exception), Times.Once());
    }

    [TestMethod]
    public async Task Given_RetryStrategyReturnsRetry_When_Fail_ThenErrorHandlerIsNotInvoked()
    {
        var context = TestData.CreateQueueContext();

        var exception = new ApplicationException();

        DetermineRetryResult = new(true, TimeSpan.FromSeconds(5));

        await reader.Fail(context, exception);
        failures.Verify(f => f.Failed(context, context.Message, exception), Times.Never());
    }
}
