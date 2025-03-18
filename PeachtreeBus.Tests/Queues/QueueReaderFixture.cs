using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using PeachtreeBus.Data;
using PeachtreeBus.Errors;
using PeachtreeBus.Queues;
using PeachtreeBus.Sagas;
using PeachtreeBus.Tests.Sagas;
using System;
using System.Threading.Tasks;

namespace PeachtreeBus.Tests.Queues
{
    /// <summary>
    /// Proves the behavior of QueueReader
    /// </summary>
    [TestClass]
    public class QueueReaderFixture
    {
        private static readonly SerializedData SerializedTestSagaData = new("SerializedTestSagaData");
        private static readonly SerializedData SerializedHeaderData = new("SerializedHeaderData");
        private static readonly QueueName NextMessageQueue = new("NextMessageQueue");
        private static readonly SagaKey SagaKey = new("SagaKey");

        private QueueReader reader = default!;
        private Mock<IBusDataAccess> dataAccess = default!;
        private Mock<ILogger<QueueReader>> log = default!;
        private Mock<IPerfCounters> perfCounters = default!;
        private Mock<ISerializer> serializer = default!;
        private Mock<ISystemClock> clock = default!;
        private Mock<IQueueFailures> failures = default!;
        private Mock<IQueueRetryStrategy> retryStrategy = default!;

        private QueueContext Context = default!;

        private QueueMessage NextMessage = default!;
        private Headers NextMessageHeaders = default!;
        private TestSagaMessage1 NextUserMessage = default!;
        private RetryResult RetryResult = new(true, TimeSpan.Zero);

        [TestInitialize]
        public void TestInitialize()
        {
            dataAccess = new();
            log = new();
            perfCounters = new();
            serializer = new();
            clock = new();
            failures = new();
            retryStrategy = new();

            clock.SetupGet(x => x.UtcNow)
                .Returns(new DateTime(2022, 2, 22, 14, 22, 22, 222, DateTimeKind.Utc));

            serializer.Setup(s => s.SerializeSaga(It.IsAny<object>(), typeof(TestSagaData)))
                .Returns(() => SerializedTestSagaData);
            serializer.Setup(s => s.SerializeHeaders(It.IsAny<Headers>()))
                .Returns(() => SerializedHeaderData);

            reader = new QueueReader(
                dataAccess.Object,
                log.Object,
                perfCounters.Object,
                serializer.Object,
                clock.Object,
                failures.Object,
                retryStrategy.Object);

            Context = TestData.CreateQueueContext(
                messageData: TestData.CreateQueueMessage(id: new(12345), notBefore: clock.Object.UtcNow));

            NextMessage = TestData.CreateQueueMessage(id: new(678890), priority: 12);

            NextMessageHeaders = new()
            {
                MessageClass = "PeachtreeBus.Tests.Sagas.TestSagaMessage1, PeachtreeBus.Tests"
            };

            NextUserMessage = new();

            dataAccess.Setup(d => d.GetPendingQueued(NextMessageQueue))
                .ReturnsAsync(() => NextMessage);

            serializer.Setup(s => s.DeserializeHeaders(It.IsAny<SerializedData>()))
                .Returns(() => NextMessageHeaders);

            serializer.Setup(s => s.DeserializeMessage(It.IsAny<SerializedData>(), typeof(TestSagaMessage1)))
                .Returns(() => NextUserMessage);

            retryStrategy.Setup(r => r.DetermineRetry(It.IsAny<QueueContext>(), It.IsAny<Exception>(), It.IsAny<int>()))
                .Returns(() => RetryResult);
        }

        /// <summary>
        /// Proves that exceptions bubble up.
        /// </summary>
        /// <returns></returns>
        [TestMethod]
        public async Task Delay_Message_ThrowsWhenCountersThrows()
        {
            perfCounters.Setup(c => c.DelayMessage()).Throws(new ApplicationException());
            await Assert.ThrowsExceptionAsync<ApplicationException>(() =>
                reader.DelayMessage(Context, 1000));
        }

        /// <summary>
        /// Proves that exceptions bubble up.
        /// </summary>
        /// <returns></returns>
        [TestMethod]
        public async Task Delay_ThrowsWhenDataAccessThrows()
        {
            dataAccess.Setup(d => d.UpdateMessage(It.IsAny<QueueMessage>(), It.IsAny<QueueName>()))
                .Throws(new ApplicationException());
            await Assert.ThrowsExceptionAsync<ApplicationException>(() =>
                reader.DelayMessage(Context, 1000));
        }

        /// <summary>
        /// Proves that the messages not before is updated when the message needs to be delayed.
        /// </summary>
        /// <returns></returns>
        [TestMethod]
        public async Task Delay_DelaysMessage()
        {
            var expectedTime = Context.MessageData.NotBefore.AddMilliseconds(1000);

            dataAccess.Setup(x => x.UpdateMessage(Context.MessageData, Context.SourceQueue))
                .Callback((QueueMessage m, QueueName n) =>
                {
                    Assert.AreEqual(expectedTime, m.NotBefore);
                })
                .Returns(Task.CompletedTask);

            await reader.DelayMessage(Context, 1000);

            dataAccess.Verify(x => x.UpdateMessage(Context.MessageData, Context.SourceQueue), Times.Once);
            Assert.AreEqual(1, dataAccess.Invocations.Count);
        }

        /// <summary>
        /// Proves that saga data is deleted when saga completes
        /// </summary>
        /// <returns></returns>
        [TestMethod]
        public async Task SaveSaga_DeletesCompleteSaga()
        {
            Context.SagaKey = SagaKey;

            var testSaga = new TestSaga { SagaComplete = true };

            await reader.SaveSaga(testSaga, Context);

            dataAccess.Verify(d => d.DeleteSagaData(testSaga.SagaName, Context.SagaKey), Times.Once);
        }

        /// <summary>
        /// Proves Saga Data is inserted when needed.
        /// </summary>
        /// <returns></returns>
        [TestMethod]
        public async Task SaveSaga_InsertsNewSagaData()
        {
            Context.SagaKey = SagaKey;
            Context.SagaData = null;

            var testSaga = new TestSaga { SagaComplete = false };

            dataAccess.Setup(d => d.InsertSagaData(It.IsAny<SagaData>(), testSaga.SagaName))
                .Callback((SagaData d, SagaName n) =>
                {
                    Assert.AreEqual(SerializedTestSagaData, d.Data);
                    Assert.AreEqual(SagaKey, d.Key);
                })
                .Returns(Task.FromResult<Identity>(new(1)));

            await reader.SaveSaga(testSaga, Context);

            Assert.IsNotNull(Context.SagaData);

            dataAccess.Verify(d => d.InsertSagaData(Context.SagaData, testSaga.SagaName), Times.Once);
        }

        /// <summary>
        /// Proves Saga Data is updated after a handler.
        /// </summary>
        /// <returns></returns>
        [TestMethod]
        public async Task SaveSaga_UpdatesExistingSagaData()
        {
            var sagaDataId = new Identity(100);

            Context.SagaKey = SagaKey;
            Context.SagaData = new()
            {
                Key = Context.SagaKey,
                Data = new("DataToBeReplaced"),
                Id = sagaDataId,
                SagaId = UniqueIdentity.New(),
                Blocked = false,
            };

            var testSaga = new TestSaga { SagaComplete = false };

            await reader.SaveSaga(testSaga, Context);

            dataAccess.Verify(d => d.UpdateSagaData(
                It.Is((SagaData s) =>
                    s.Data == SerializedTestSagaData &&
                    s.Id == sagaDataId &&
                    s.Key == Context.SagaKey),
                testSaga.SagaName),
                Times.Once);
        }

        /// <summary>
        /// Proves that new saga data is created as needed
        /// </summary>
        /// <returns></returns>
        [TestMethod]
        public async Task LoadSaga_InitializesWhenNew()
        {
            var testSaga = new TestSaga() { Data = null! };
            Context.SagaKey = SagaKey;

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

            Context.SagaKey = SagaKey;

            var sagaData = new SagaData()
            {
                Blocked = false,
                Data = SerializedTestSagaData,
                Key = Context.SagaKey,
                SagaId = UniqueIdentity.New(),
            };

            dataAccess.Setup(d => d.GetSagaData(testSaga.SagaName, Context.SagaKey))
                .ReturnsAsync(sagaData);

            var data = new TestSagaData();
            serializer.Setup(s => s.DeserializeSaga(sagaData.Data, typeof(TestSagaData))).Returns(data);

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
            Context.SagaKey = SagaKey;

            var testSaga = new TestSaga { Data = default! };

            var sagaData = new SagaData
            {
                Blocked = true,
                Data = SerializedTestSagaData,
                Key = SagaKey,
                SagaId = UniqueIdentity.New(),
            };

            dataAccess.Setup(d => d.GetSagaData(testSaga.SagaName, Context.SagaKey))
                .ReturnsAsync(sagaData);

            await reader.LoadSaga(testSaga, Context);

            serializer.Verify(s => s.DeserializeSaga(sagaData.Data, typeof(TestSagaData)), Times.Never);
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
            RetryResult = new(true, delay);
            Context.MessageData.Retries = 0;
            UtcDateTime expectedNotBefore = clock.Object.UtcNow.Add(delay);

            var exception = new ApplicationException();

            serializer.Setup(s => s.SerializeHeaders(Context.Headers))
                .Callback((Headers h) =>
                {
                    Assert.AreEqual(exception.ToString(), h.ExceptionDetails);
                })
                .Returns(SerializedHeaderData);

            dataAccess.Setup(c => c.UpdateMessage(Context.MessageData, Context.SourceQueue))
                .Callback((QueueMessage m, QueueName n) =>
                {
                    Assert.AreEqual(1, m.Retries);
                    Assert.AreEqual(expectedNotBefore, m.NotBefore);
                    Assert.AreEqual(SerializedHeaderData, m.Headers);
                })
                .Returns(Task.CompletedTask);

            await reader.Fail(Context, exception);

            dataAccess.Verify(d => d.UpdateMessage(Context.MessageData, Context.SourceQueue), Times.Once);
            Assert.AreEqual(1, dataAccess.Invocations.Count);
        }

        /// <summary>
        /// Proves that message is failed after retries.
        /// </summary>
        /// <returns></returns>
        [TestMethod]
        public async Task Given_RetryStrategyReturnsFail_When_Fail_Then_MessagesIsFailed()
        {
            RetryResult = new(false, TimeSpan.FromHours(1));
            var expectedMessageId = Context.MessageData.Id;
            var exception = new ApplicationException();

            serializer.Setup(s => s.SerializeHeaders(Context.Headers))
                .Callback((Headers h) =>
                {
                    Assert.AreEqual(exception.ToString(), h.ExceptionDetails);
                })
                .Returns(SerializedHeaderData);

            dataAccess.Setup(c => c.FailMessage(Context.MessageData, Context.SourceQueue))
                .Callback((QueueMessage m, QueueName n) =>
                {
                    Assert.AreEqual(expectedMessageId, m.Id);
                    Assert.AreEqual(SerializedHeaderData, m.Headers);
                })
                .Returns(Task.CompletedTask);

            await reader.Fail(Context, new ApplicationException());

            dataAccess.Verify(d => d.FailMessage(Context.MessageData, Context.SourceQueue), Times.Once);
            Assert.AreEqual(1, dataAccess.Invocations.Count);
            perfCounters.Verify(c => c.FailMessage(), Times.Once);
        }

        /// <summary>
        /// Proves that messages are compelted.
        /// </summary>
        /// <returns></returns>
        [TestMethod]
        public async Task Complete_Completes()
        {
            dataAccess.Setup(d => d.CompleteMessage(Context.MessageData, Context.SourceQueue))
                .Callback((QueueMessage m, QueueName n) =>
                {
                    Assert.AreEqual(clock.Object.UtcNow, m.Completed);
                })
                .Returns(Task.CompletedTask);

            await reader.Complete(Context);

            perfCounters.Verify(c => c.CompleteMessage(), Times.Once);
            dataAccess.Verify(d => d.CompleteMessage(Context.MessageData, Context.SourceQueue), Times.Once);
            Assert.AreEqual(1, dataAccess.Invocations.Count);
        }

        [TestMethod]
        public async Task GetNext_ReturnsNull()
        {
            dataAccess.Setup(d => d.GetPendingQueued(NextMessageQueue))
                .ReturnsAsync((QueueMessage)null!);

            Assert.IsNull(await reader.GetNext(NextMessageQueue));
        }

        /// <summary>
        /// Proves a good message is handled.
        /// </summary>
        /// <returns></returns>
        [TestMethod]
        public async Task GetNext_GetsGoodMessage()
        {
            var context = await reader.GetNext(NextMessageQueue);

            Assert.IsNotNull(context?.MessageData);
            Assert.AreSame(NextMessage, context.MessageData);
            Assert.IsNotNull(context?.Headers);
            Assert.AreSame(NextMessageHeaders, context.Headers);
            Assert.AreEqual(NextMessageQueue, context.SourceQueue);
            Assert.IsFalse(context.SagaBlocked);
            Assert.AreEqual(NextMessage.MessageId, context.MessageId);
            Assert.AreEqual(NextMessage.Priority, context.MessagePriority);
            Assert.AreSame(NextUserMessage, context.Message);
        }

        /// <summary>
        /// Proves the behavior when headers cannot deserialize
        /// </summary>
        /// <returns></returns>
        [TestMethod]
        public async Task GetNext_HandlesUndeserializableHeaders()
        {
            var deserializeException = new Exception("Test Exception");

            serializer.Setup(s => s.DeserializeHeaders(It.IsAny<SerializedData>()))
               .Throws(deserializeException);

            serializer.Setup(s => s.DeserializeMessage(It.IsAny<SerializedData>(), typeof(object)))
                .Returns(null!);

            var context = await reader.GetNext(NextMessageQueue);

            Assert.IsNotNull(context?.MessageData);
            Assert.AreSame(NextMessage, context.MessageData);
            Assert.IsNotNull(context?.Headers);
            Assert.AreSame("System.Object", context.Headers.MessageClass);
            Assert.AreEqual(NextMessageQueue, context.SourceQueue);
            Assert.IsFalse(context.SagaBlocked);
            Assert.AreEqual(NextMessage.MessageId, context.MessageId);
            Assert.AreEqual(NextMessage.Priority, context.MessagePriority);
            Assert.IsNull(context.Message);
        }

        /// <summary>
        /// Proves the behavior when the message type is unrecognized.
        /// </summary>
        /// <returns></returns>
        [TestMethod]
        public async Task GetNext_HandlesUnrecognizedMessageClass()
        {
            NextMessageHeaders.MessageClass =
                "PeachtreeBus.Tests.Sagas.TestSagaNotARealMessage, PeachtreeBus.Tests";

            var context = await reader.GetNext(NextMessageQueue);

            Assert.IsNotNull(context?.MessageData);
            Assert.AreSame(NextMessage, context.MessageData);
            Assert.IsNotNull(context?.Headers);
            Assert.AreSame(NextMessageHeaders, context.Headers);
            Assert.AreEqual(NextMessageQueue, context.SourceQueue);
            Assert.IsFalse(context.SagaBlocked);
            Assert.AreEqual(NextMessage.MessageId, context.MessageId);
            Assert.AreEqual(NextMessage.Priority, context.MessagePriority);
            Assert.IsNull(context.Message);
        }

        /// <summary>
        /// Proves a message fails when it can deserialize
        /// </summary>
        /// <returns></returns>
        [TestMethod]
        public async Task GetNext_HandlesUndeserializableMessageBody()
        {
            serializer.Setup(s => s.DeserializeMessage(It.IsAny<SerializedData>(), typeof(TestSagaMessage1)))
                .Throws(new Exception("Test Exception"));

            var context = await reader.GetNext(NextMessageQueue);

            Assert.IsNotNull(context);
            Assert.AreSame(NextMessage, context.MessageData);
            Assert.AreSame(NextMessageHeaders, context.Headers);
            Assert.AreEqual(NextMessageQueue, context.SourceQueue);
            Assert.IsFalse(context.SagaBlocked);
            Assert.AreEqual(NextMessage.MessageId, context.MessageId);
            Assert.AreEqual(NextMessage.Priority, context.MessagePriority);
            Assert.IsNull(context.Message);
        }

        [TestMethod]
        public async Task Given_RetryStrategyReturnsFail_When_Fail_ThenErrorHandlerIsInvoked()
        {
            var context = TestData.CreateQueueContext();

            RetryResult = new(false, TimeSpan.Zero);

            var exception = new ApplicationException();

            await reader.Fail(context, exception);
            failures.Verify(f => f.Failed(context, context.Message, exception), Times.Once());
        }

        [TestMethod]
        public async Task Given_RetryStrategyReturnsRetry_When_Fail_ThenErrorHandlerIsNotInvoked()
        {
            var context = TestData.CreateQueueContext();

            var exception = new ApplicationException();

            RetryResult = new(true, TimeSpan.FromSeconds(5));

            await reader.Fail(context, exception);
            failures.Verify(f => f.Failed(context, context.Message, exception), Times.Never());
        }
    }
}
