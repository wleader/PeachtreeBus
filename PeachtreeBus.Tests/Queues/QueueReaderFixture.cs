using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using PeachtreeBus.Data;
using PeachtreeBus.Errors;
using PeachtreeBus.Interfaces;
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

        private QueueReader reader = default!;
        private Mock<IBusDataAccess> dataAccess = default!;
        private Mock<ILogger<QueueReader>> log = default!;
        private Mock<IPerfCounters> perfCounters = default!;
        private Mock<ISerializer> serializer = default!;
        private Mock<ISystemClock> clock = default!;
        private Mock<IQueueFailures> failures = default!;

        private InternalQueueContext Context = default!;

        private QueueName NextMessageQueue = new("NextMessageQueue");
        private QueueMessage NextMessage = default!;
        private Headers NextMessageHeaders = default!;
        private TestSagaMessage1 NextUserMessage = new();

        [TestInitialize]
        public void TestInitialize()
        {
            dataAccess = new Mock<IBusDataAccess>();
            log = new Mock<ILogger<QueueReader>>();
            perfCounters = new Mock<IPerfCounters>();
            serializer = new Mock<ISerializer>();
            clock = new Mock<ISystemClock>();
            failures = new();

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
                failures.Object);

            Context = new()
            {
                SourceQueue = new("SourceQueue"),
                MessageData = new()
                {
                    Id = 12345,
                    NotBefore = clock.Object.UtcNow,
                }
            };

            NextMessage = new()
            {
                Id = 67890,
                Priority = 12,
            };

            NextMessageHeaders = new()
            {
                MessageClass = "PeachtreeBus.Tests.Sagas.TestSagaMessage1, PeachtreeBus.Tests"
            };

            dataAccess.Setup(d => d.GetPendingQueued(NextMessageQueue))
                .ReturnsAsync(() => NextMessage);

            serializer.Setup(s => s.DeserializeHeaders(It.IsAny<SerializedData>()))
                .Returns(() => NextMessageHeaders);

            serializer.Setup(s => s.DeserializeMessage(It.IsAny<SerializedData>(), typeof(TestSagaMessage1)))
                .Returns(() => NextUserMessage);

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
            dataAccess.Setup(d => d.Update(It.IsAny<QueueMessage>(), It.IsAny<QueueName>()))
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

            dataAccess.Setup(x => x.Update(Context.MessageData, Context.SourceQueue))
                .Callback((QueueMessage m, QueueName n) =>
                {
                    Assert.AreEqual(expectedTime, m.NotBefore);
                })
                .Returns(Task.CompletedTask);

            await reader.DelayMessage(Context, 1000);

            dataAccess.Verify(x => x.Update(Context.MessageData, Context.SourceQueue), Times.Once);
            Assert.AreEqual(1, dataAccess.Invocations.Count);
        }

        /// <summary>
        /// Proves that saga data is deleted when saga completes
        /// </summary>
        /// <returns></returns>
        [TestMethod]
        public async Task SaveSaga_DeletesCompleteSaga()
        {
            Context.SagaKey = "SagaKey";

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
            Context.SagaKey = "SagaKey";
            Context.SagaData = null;

            var testSaga = new TestSaga { SagaComplete = false };

            dataAccess.Setup(d => d.Insert(It.IsAny<SagaData>(), testSaga.SagaName))
                .Callback((SagaData d, SagaName n) =>
                {
                    Assert.AreEqual(SerializedTestSagaData, d.Data);
                    Assert.AreEqual("SagaKey", d.Key);
                    Assert.AreNotEqual(Guid.Empty, d.SagaId);
                })
                .Returns(Task.FromResult<long>(1));

            await reader.SaveSaga(testSaga, Context);

            Assert.IsNotNull(Context.SagaData);

            dataAccess.Verify(d => d.Insert(Context.SagaData, testSaga.SagaName), Times.Once);
        }

        /// <summary>
        /// Proves Saga Data is updated after a handler.
        /// </summary>
        /// <returns></returns>
        [TestMethod]
        public async Task SaveSaga_UpdatesExistingSagaData()
        {
            var sagaDataId = 100;

            Context.SagaKey = "SagaKey";
            Context.SagaData = new()
            {
                Key = Context.SagaKey,
                Data = new("DataToBeReplaced"),
                Id = sagaDataId,
            };

            var testSaga = new TestSaga { SagaComplete = false };

            await reader.SaveSaga(testSaga, Context);

            dataAccess.Verify(d => d.Update(
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
            Context.SagaKey = "SagaKey";

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

            Context.SagaKey = "SagaKey";

            var sagaData = new SagaData()
            {
                Blocked = false,
                Data = SerializedTestSagaData,
                Key = Context.SagaKey,
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
            Context.SagaKey = "SagaKey";

            var testSaga = new TestSaga { Data = default! };

            var sagaData = new SagaData
            {
                Blocked = true,
                Data = SerializedTestSagaData,
                Key = "SagaKey",
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
        public async Task Fail_UpdatesMessage()
        {
            var expectedRetries = (byte)(reader.MaxRetries - 1);

            Context.MessageData.Retries = (byte)(reader.MaxRetries - 2);

            var exception = new ApplicationException();

            serializer.Setup(s => s.SerializeHeaders(Context.Headers))
                .Callback((Headers h) =>
                {
                    Assert.AreEqual(exception.ToString(), h.ExceptionDetails);
                })
                .Returns(SerializedHeaderData);

            dataAccess.Setup(c => c.Update(Context.MessageData, Context.SourceQueue))
                .Callback((QueueMessage m, QueueName n) =>
                {
                    Assert.AreEqual(expectedRetries, m.Retries);
                    Assert.IsTrue(m.NotBefore >= clock.Object.UtcNow.AddSeconds(5));
                    Assert.AreEqual(SerializedHeaderData, m.Headers);
                })
                .Returns(Task.CompletedTask);

            await reader.Fail(Context, exception);

            dataAccess.Verify(d => d.Update(Context.MessageData, Context.SourceQueue), Times.Once);
            Assert.AreEqual(1, dataAccess.Invocations.Count);
        }

        /// <summary>
        /// Proves that message is failed after retries.
        /// </summary>
        /// <returns></returns>
        [TestMethod]
        public async Task Fail_FailsMaxReties()
        {
            var expectedRetries = reader.MaxRetries;
            Context.MessageData.Retries = (byte)(reader.MaxRetries - 1);

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
                    Assert.AreEqual(expectedRetries, m.Retries);
                    Assert.IsTrue(m.NotBefore >= clock.Object.UtcNow.AddSeconds(5));
                    Assert.AreEqual(SerializedHeaderData, m.Headers);
                    Assert.AreEqual(clock.Object.UtcNow, m.Failed);
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
        public async Task Fail_InvokesFailHandlerOnMaxRetries()
        {
            var context = new InternalQueueContext
            {
                MessageData = new QueueMessage
                {
                    Retries = (byte)(reader.MaxRetries - 1),
                },
                Headers = new Headers
                {

                },
                Message = new TestSagaMessage1()
            };


            var exception = new ApplicationException();

            await reader.Fail(context, exception);
            failures.Verify(f => f.Failed(context, context.Message, exception), Times.Once());
        }

        [TestMethod]
        public async Task Fail_DoesNotInvokeFailHandlerBeforeMaxRetries()
        {
            var context = new InternalQueueContext
            {
                MessageData = new QueueMessage
                {
                    Retries = (byte)(reader.MaxRetries - 2),
                },
                Headers = new Headers
                {

                },
                Message = new TestSagaMessage1()
            };


            var exception = new ApplicationException();

            await reader.Fail(context, exception);
            failures.Verify(f => f.Failed(context, context.Message, exception), Times.Never());
        }
    }
}
