using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using PeachtreeBus.Data;
using PeachtreeBus.Errors;
using PeachtreeBus.Interfaces;
using PeachtreeBus.Model;
using PeachtreeBus.Queues;
using System;
using System.Text.Json;
using System.Threading.Tasks;

namespace PeachtreeBus.Tests.Queues
{
    /// <summary>
    /// Proves the behavior of QueueReader
    /// </summary>
    [TestClass]
    public class QueueReaderFixture
    {
        private QueueReader reader = default!;
        private Mock<IBusDataAccess> dataAccess = default!;
        private Mock<ILogger<QueueReader>> log = default!;
        private Mock<IPerfCounters> perfCounters = default!;
        private Mock<ISerializer> serializer = default!;
        private Mock<ISystemClock> clock = default!;
        private Mock<IQueueFailures> failures = default!;

        private QueueMessage UpdatedMessage = default!;
        private string UpdatedQueue = default!;
        private QueueMessage FailedMessage = default!;
        private string FailedQueue = default!;
        private QueueMessage CompletedMessage = default!;
        private string CompletedQueue = default!;
        private SagaData InsertedSagaData = default!;
        private string InsertedSagaName = default!;
        private SagaData UpdatedSagaData = default!;
        private string UpdatedSagaName = default!;

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

            dataAccess.Setup(d => d.Update(It.IsAny<QueueMessage>(), It.IsAny<string>()))
                .Callback<QueueMessage, string>((m, q) =>
                {
                    UpdatedMessage = m;
                    UpdatedQueue = q;
                })
                .Returns(Task.CompletedTask);

            dataAccess.Setup(d => d.Insert(It.IsAny<SagaData>(), It.IsAny<string>()))
                .Callback<SagaData, string>((d, n) =>
                {
                    InsertedSagaData = d;
                    InsertedSagaName = n;
                })
                .Returns(Task.FromResult<long>(12345));

            dataAccess.Setup(d => d.Update(It.IsAny<SagaData>(), It.IsAny<string>()))
                .Callback<SagaData, string>((d, n) =>
                {
                    UpdatedSagaData = d;
                    UpdatedSagaName = n;
                })
                .Returns(Task.CompletedTask);

            dataAccess.Setup(d => d.FailMessage(It.IsAny<QueueMessage>(), It.IsAny<string>()))
                .Callback<QueueMessage, string>((d, n) =>
                {
                    FailedMessage = d;
                    FailedQueue = n;
                })
                .Returns(Task.CompletedTask);

            dataAccess.Setup(d => d.CompleteMessage(It.IsAny<QueueMessage>(), It.IsAny<string>()))
                .Callback<QueueMessage, string>((d, n) =>
                {
                    CompletedMessage = d;
                    CompletedQueue = n;
                })
                .Returns(Task.CompletedTask);

            serializer.Setup(s => s.SerializeSaga(It.IsAny<object>(), typeof(TestSagaData)))
                .Returns("SerializedTestSagaData");

            reader = new QueueReader(dataAccess.Object, log.Object, perfCounters.Object, serializer.Object, clock.Object, failures.Object);
        }

        /// <summary>
        /// Proves that exceptions bubble up.
        /// </summary>
        /// <returns></returns>
        [TestMethod]
        [ExpectedException(typeof(ApplicationException))]
        public async Task Delay_Message_ThrowsWhenCountersThrows()
        {
            perfCounters.Setup(c => c.DelayMessage()).Throws(new ApplicationException());

            var now = clock.Object.UtcNow;
            var context = new InternalQueueContext
            {
                MessageData = new QueueMessage
                {
                    Id = 12345,
                    NotBefore = now,
                },
                SourceQueue = "SourceQueue"
            };
            await reader.DelayMessage(context, 1000);
        }

        /// <summary>
        /// Proves that exceptions bubble up.
        /// </summary>
        /// <returns></returns>
        [TestMethod]
        [ExpectedException(typeof(ApplicationException))]
        public async Task Delay_ThrowsWhenDataAccessThrows()
        {
            dataAccess.Setup(d => d.Update(It.IsAny<QueueMessage>(), It.IsAny<string>()))
                .Throws(new ApplicationException());

            var now = clock.Object.UtcNow;
            var context = new InternalQueueContext
            {
                MessageData = new QueueMessage
                {
                    Id = 12345,
                    NotBefore = now,
                },
                SourceQueue = "SourceQueue"
            };
            await reader.DelayMessage(context, 1000);
        }

        /// <summary>
        /// Proves that the messages not before is updated when the message needs to be delayed.
        /// </summary>
        /// <returns></returns>
        [TestMethod]
        public async Task Delay_DelaysMessage()
        {
            var now = clock.Object.UtcNow;
            var context = new InternalQueueContext
            {
                MessageData = new QueueMessage
                {
                    Id = 12345,
                    NotBefore = now,
                },
                SourceQueue = "SourceQueue"
            };
            await reader.DelayMessage(context, 1000);

            var expectedTime = now.AddMilliseconds(1000);

            Assert.IsTrue(ReferenceEquals(context.MessageData, UpdatedMessage));
            Assert.AreEqual(expectedTime, context.MessageData.NotBefore);
            Assert.AreEqual(UpdatedQueue, "SourceQueue");
        }

        /// <summary>
        /// Proves that saga data is deleted when saga completes
        /// </summary>
        /// <returns></returns>
        [TestMethod]
        public async Task SaveSaga_DeletesCompleteSaga()
        {
            var context = new InternalQueueContext
            {
                MessageData = new QueueMessage
                {
                    Id = 12345,
                    NotBefore = clock.Object.UtcNow,
                },
                SourceQueue = "SourceQueue",
                SagaKey = "SagaKey"
            };

            var testSaga = new TestSaga
            {
                SagaComplete = true
            };

            await reader.SaveSaga(testSaga, context);

            dataAccess.Verify(d => d.DeleteSagaData(testSaga.SagaName, context.SagaKey), Times.Once);
        }

        /// <summary>
        /// Proves Saga Data is inserted when needed.
        /// </summary>
        /// <returns></returns>
        [TestMethod]
        public async Task SaveSaga_InsertsNewSagaData()
        {
            var context = new InternalQueueContext
            {
                MessageData = new QueueMessage
                {
                    Id = 12345,
                    NotBefore = clock.Object.UtcNow,
                },
                SourceQueue = "SourceQueue",
                SagaKey = "SagaKey",
                SagaData = null
            };

            var testSaga = new TestSaga
            {
                SagaComplete = false
            };

            await reader.SaveSaga(testSaga, context);

            dataAccess.Verify(d => d.Insert(It.IsAny<SagaData>(), It.IsAny<string>()), Times.Once);

            Assert.AreEqual(testSaga.SagaName, InsertedSagaName);
            Assert.AreEqual("SerializedTestSagaData", InsertedSagaData.Data);
            Assert.AreEqual(context.SagaKey, InsertedSagaData.Key);
        }

        /// <summary>
        /// Proves Saga Data is updated after a handler.
        /// </summary>
        /// <returns></returns>
        [TestMethod]
        public async Task SaveSaga_UpdatesExistingSagaData()
        {
            var context = new InternalQueueContext
            {
                MessageData = new QueueMessage
                {
                    Id = 12345,
                    NotBefore = clock.Object.UtcNow,
                },
                SourceQueue = "SourceQueue",
                SagaKey = "SagaKey",
                SagaData = new SagaData
                {
                    Key = "SagaKey",
                    Data = "InitialData",
                }
            };

            var testSaga = new TestSaga
            {
                SagaComplete = false
            };

            await reader.SaveSaga(testSaga, context);

            dataAccess.Verify(d => d.Update(It.IsAny<SagaData>(), It.IsAny<string>()), Times.Once);

            Assert.AreEqual(testSaga.SagaName, UpdatedSagaName);
            Assert.AreEqual("SerializedTestSagaData", UpdatedSagaData.Data);
            Assert.AreEqual(context.SagaKey, UpdatedSagaData.Key);
        }

        /// <summary>
        /// Proves that new saga data is created as needed
        /// </summary>
        /// <returns></returns>
        [TestMethod]
        public async Task LoadSaga_InitializesWhenNew()
        {
            var context = new InternalQueueContext
            {

            };
            var testSaga = new TestSaga
            {

            };

            dataAccess.Setup(d => d.GetSagaData(It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync((SagaData)null!);

            await reader.LoadSaga(testSaga, context);

            Assert.IsNotNull(testSaga.Data);
        }

        /// <summary>
        /// Proves that saga data can be deserialized.
        /// </summary>
        /// <returns></returns>
        [TestMethod]
        public async Task LoadSaga_DeserializesExisting()
        {
            var context = new InternalQueueContext
            {

            };
            var testSaga = new TestSaga
            {

            };

            dataAccess.Setup(d => d.GetSagaData(It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync(new SagaData
                {
                    Blocked = false,
                    Data = "{}",
                    Key = "SagaKey",
                });

            serializer.Setup(s => s.DeserializeSaga("{}", typeof(TestSagaData)))
                .Returns(new TestSagaData());

            await reader.LoadSaga(testSaga, context);

            Assert.IsNotNull(testSaga.Data);

            serializer.Verify(s => s.DeserializeSaga("{}", typeof(TestSagaData)), Times.Once);
        }

        /// <summary>
        /// Proves that blocked saga data can be handled.
        /// </summary>
        /// <returns></returns>
        [TestMethod]
        public async Task LoadSaga_ReturnsWhenBlocked()
        {
            var context = new InternalQueueContext
            {

            };
            var testSaga = new TestSaga
            {

            };

            dataAccess.Setup(d => d.GetSagaData(It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync(new SagaData
                {
                    Blocked = true,
                    Data = "{}",
                    Key = "SagaKey",
                });

            await reader.LoadSaga(testSaga, context);

            Assert.IsNotNull(testSaga.Data);
            Assert.IsTrue(context.SagaData!.Blocked);
        }

        /// <summary>
        /// Proves that Fail updates retry counts.
        /// </summary>
        /// <returns></returns>
        [TestMethod]
        public async Task Fail_UpdatesMessage()
        {
            var expectedRetries = (byte)(reader.MaxRetries - 1);
            var context = new InternalQueueContext
            {
                SourceQueue = "TestSourceQueue",
                MessageData = new QueueMessage
                {
                    Retries = (byte)(reader.MaxRetries - 2),
                },
                Headers = new Headers
                {

                },
            };

            await reader.Fail(context, new ApplicationException());

            Assert.AreEqual(expectedRetries, context.MessageData.Retries);
            Assert.IsTrue(context.MessageData.NotBefore >= clock.Object.UtcNow.AddSeconds(5)); // 
            Assert.IsFalse(string.IsNullOrWhiteSpace(context.Headers.ExceptionDetails));
            perfCounters.Verify(c => c.RetryMessage(), Times.Once);
            dataAccess.Verify(c => c.Update(It.IsAny<QueueMessage>(), It.IsAny<string>()), Times.Once);
            Assert.IsTrue(ReferenceEquals(context.MessageData, UpdatedMessage));
            Assert.AreEqual("TestSourceQueue", UpdatedQueue);
        }

        /// <summary>
        /// Proves that message is failed after retries.
        /// </summary>
        /// <returns></returns>
        [TestMethod]
        public async Task Fail_FailsMaxReties()
        {
            var expectedRetries = (byte)(reader.MaxRetries);
            var context = new InternalQueueContext
            {
                SourceQueue = "TestSourceQueue",
                MessageData = new QueueMessage
                {
                    Retries = (byte)(reader.MaxRetries - 1),
                },
                Headers = new Headers
                {

                },
            };

            await reader.Fail(context, new ApplicationException());

            Assert.AreEqual(expectedRetries, context.MessageData.Retries);
            Assert.IsTrue(context.MessageData.NotBefore >= clock.Object.UtcNow.AddSeconds(5)); // 
            Assert.IsFalse(string.IsNullOrWhiteSpace(context.Headers.ExceptionDetails));
            perfCounters.Verify(c => c.FailMessage(), Times.Once);
            dataAccess.Verify(c => c.FailMessage(It.IsAny<QueueMessage>(), It.IsAny<string>()), Times.Once);
            Assert.IsTrue(ReferenceEquals(context.MessageData, FailedMessage));
            Assert.AreEqual("TestSourceQueue", FailedQueue);
        }

        /// <summary>
        /// Proves that messages are compelted.
        /// </summary>
        /// <returns></returns>
        [TestMethod]
        public async Task Completee_Completes()
        {
            var now = clock.Object.UtcNow;
            var context = new InternalQueueContext
            {
                MessageData = new QueueMessage
                {
                    Id = 12345,
                    NotBefore = now,
                },
                SourceQueue = "SourceQueue"
            };
            await reader.Complete(context);

            Assert.IsTrue(ReferenceEquals(context.MessageData, CompletedMessage));
            Assert.AreEqual(now, context.MessageData.Completed);
            Assert.AreEqual(CompletedQueue, "SourceQueue");
        }

        /// <summary>
        /// Proves a good message is handled.
        /// </summary>
        /// <returns></returns>
        [TestMethod]
        public async Task GetNext_GetsGoodMessage()
        {
            var messageClass = typeof(TestSagaMessage1).FullName + ", " + typeof(TestSagaMessage1).Assembly.GetName().Name;

            var expectedQueueMessage = new QueueMessage
            {
                Headers = "{ \"MessageClass\":\"" + messageClass + "\"}",
                MessageId = Guid.NewGuid()
            };

            var expectedHeaders = new Headers
            {
                MessageClass = messageClass,
                ExceptionDetails = null
            };

            var expectedUserMessage = new TestSagaMessage1();

            dataAccess.Setup(d => d.GetPendingQueued("SourceQueue"))
                .ReturnsAsync(expectedQueueMessage);

            serializer.Setup(s => s.DeserializeHeaders(It.IsAny<string>()))
               .Returns(expectedHeaders);

            serializer.Setup(s => s.DeserializeMessage(It.IsAny<string>(), typeof(TestSagaMessage1)))
                .Returns(expectedUserMessage);

            var context = await reader.GetNext("SourceQueue");

            Assert.IsTrue(ReferenceEquals(expectedQueueMessage, context!.MessageData));
            Assert.IsTrue(ReferenceEquals(expectedHeaders, context.Headers));
            Assert.IsTrue(ReferenceEquals(expectedUserMessage, context.Message));
            Assert.AreEqual("SourceQueue", context.SourceQueue);
            Assert.IsFalse(context.SagaBlocked);
            Assert.AreEqual(expectedQueueMessage.MessageId, context.MessageId);
        }

        /// <summary>
        /// Proves the behavior when headers cannot deserialize
        /// </summary>
        /// <returns></returns>
        [TestMethod]
        public async Task GetNext_HandlesUndeserializableHeaders()
        {
            var messageClass = typeof(TestSagaMessage1).FullName + ", " + typeof(TestSagaMessage1).Assembly.GetName().Name;

            var expectedQueueMessage = new QueueMessage
            {
                Headers = "{ \"MessageClass\":\"" + messageClass + "\"}"
            };

            var expectedHeaders = new Headers
            {
                MessageClass = messageClass,
                ExceptionDetails = null
            };

            var expectedUserMessage = new TestSagaMessage1();

            dataAccess.Setup(d => d.GetPendingQueued("SourceQueue"))
                .ReturnsAsync(expectedQueueMessage);

            serializer.Setup(s => s.DeserializeHeaders(It.IsAny<string>()))
               .Throws(new JsonException());

            serializer.Setup(s => s.DeserializeMessage(It.IsAny<string>(), typeof(TestSagaMessage1)))
                .Returns(expectedUserMessage);

            var message = await reader.GetNext("SourceQueue");

            Assert.IsTrue(ReferenceEquals(expectedQueueMessage, message!.MessageData));
            Assert.IsNotNull(message.Headers);
            Assert.IsNull(message.Message);
            Assert.AreEqual("SourceQueue", message.SourceQueue);
        }

        /// <summary>
        /// Proves the behavior when the message type is unrecognized.
        /// </summary>
        /// <returns></returns>
        [TestMethod]
        public async Task GetNext_HandlesUnrecognizedMessageClass()
        {
            var messageClass = "Peachtreebus.Tests.Sagas.TestSagaNotARealMessage, " + typeof(TestSagaMessage1).Assembly.GetName().Name;

            var expectedQueueMessage = new QueueMessage
            {
                Headers = "{ \"MessageClass\":\"" + messageClass + "\"}"
            };

            var expectedHeaders = new Headers
            {
                MessageClass = messageClass,
                ExceptionDetails = null
            };

            var expectedUserMessage = new TestSagaMessage1();

            dataAccess.Setup(d => d.GetPendingQueued("SourceQueue"))
                .ReturnsAsync(expectedQueueMessage);

            serializer.Setup(s => s.DeserializeHeaders(It.IsAny<string>()))
               .Returns(expectedHeaders);

            var message = await reader.GetNext("SourceQueue");

            serializer.Verify(s => s.DeserializeMessage(It.IsAny<string>(), It.IsAny<Type>()), Times.Never);
            Assert.IsTrue(ReferenceEquals(expectedQueueMessage, message!.MessageData));
            Assert.IsNotNull(message.Headers);
            Assert.IsNull(message.Message);
            Assert.AreEqual("SourceQueue", message.SourceQueue);
        }

        /// <summary>
        /// Proves a message fails when it can deserialize
        /// </summary>
        /// <returns></returns>
        [TestMethod]
        public async Task GetNext_HandlesUndeserializableMessageBody()
        {
            var messageClass = typeof(TestSagaMessage1).FullName + ", " + typeof(TestSagaMessage1).Assembly.GetName().Name;

            var expectedQueueMessage = new QueueMessage
            {
                Headers = "{ \"MessageClass\":\"" + messageClass + "\"}"
            };

            var expectedHeaders = new Headers
            {
                MessageClass = messageClass,
                ExceptionDetails = null
            };

            var expectedUserMessage = new TestSagaMessage1();

            dataAccess.Setup(d => d.GetPendingQueued("SourceQueue"))
                .ReturnsAsync(expectedQueueMessage);

            serializer.Setup(s => s.DeserializeHeaders(It.IsAny<string>()))
               .Returns(expectedHeaders);

            serializer.Setup(s => s.DeserializeMessage(It.IsAny<string>(), typeof(TestSagaMessage1)))
                .Throws(new JsonException());

            var message = await reader.GetNext("SourceQueue");

            Assert.IsTrue(ReferenceEquals(expectedQueueMessage, message!.MessageData));
            Assert.IsTrue(ReferenceEquals(expectedHeaders, message.Headers));
            Assert.IsNull(message.Message);
            Assert.AreEqual("SourceQueue", message.SourceQueue);
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
