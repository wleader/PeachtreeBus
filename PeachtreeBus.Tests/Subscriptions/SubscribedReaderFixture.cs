using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using PeachtreeBus.Data;
using PeachtreeBus.Errors;
using PeachtreeBus.Interfaces;
using PeachtreeBus.Subscriptions;
using PeachtreeBus.Tests.Sagas;
using System;
using System.Threading.Tasks;

namespace PeachtreeBus.Tests.Subscriptions
{
    /// <summary>
    /// Proves the behavior of SubscribedReader
    /// </summary>
    [TestClass]
    public class SubscribedReaderFixture
    {
        private SubscribedReader reader = default!;
        private Mock<IBusDataAccess> dataAccess = default!;
        private Mock<ILogger<SubscribedReader>> log = default!;
        private Mock<IPerfCounters> counters = default!;
        private Mock<ISerializer> serializer = default!;
        private Mock<ISystemClock> clock = default!;
        private Mock<ISubscribedFailures> failures = default!;

        private SubscribedMessage UpdatedMessage = default!;
        private SubscribedMessage FailedMessage = default!;
        private SubscribedMessage CompletedMessage = default!;

        private Guid SubscriberId = Guid.Parse("5d7ece7e-b9eb-4b97-91fa-af6bfe50394a");

        private SubscribedMessage NextMessage = default!;
        private Headers NextMessageHeaders = default!;
        private TestSagaMessage1 NextUserMessage = default!;

        [TestInitialize]
        public void TestInitialize()
        {
            dataAccess = new Mock<IBusDataAccess>();
            log = new Mock<ILogger<SubscribedReader>>();
            counters = new Mock<IPerfCounters>();
            serializer = new Mock<ISerializer>();
            clock = new Mock<ISystemClock>();
            failures = new();

            clock.SetupGet(x => x.UtcNow)
                .Returns(new DateTime(2022, 2, 22, 14, 22, 22, 222, DateTimeKind.Utc));

            dataAccess.Setup(d => d.Update(It.IsAny<SubscribedMessage>()))
                .Callback<SubscribedMessage>((m) =>
                {
                    UpdatedMessage = m;
                })
                .Returns(Task.CompletedTask);

            dataAccess.Setup(d => d.FailMessage(It.IsAny<SubscribedMessage>()))
                .Callback<SubscribedMessage>((d) =>
                {
                    FailedMessage = d;
                })
                .Returns(Task.CompletedTask);

            dataAccess.Setup(d => d.CompleteMessage(It.IsAny<SubscribedMessage>()))
                .Callback<SubscribedMessage>((d) =>
                {
                    CompletedMessage = d;
                })
                .Returns(Task.CompletedTask);

            reader = new SubscribedReader(
                dataAccess.Object,
                serializer.Object,
                log.Object,
                counters.Object,
                clock.Object,
                failures.Object);

            NextMessage = new()
            {
                Id = 12345,
                Priority = 24,
            };

            dataAccess.Setup(d => d.GetPendingSubscribed(SubscriberId))
                .ReturnsAsync(() => NextMessage);

            NextMessageHeaders = new()
            {
                MessageClass = "PeachtreeBus.Tests.Sagas.TestSagaMessage1, PeachtreeBus.Tests"
            };


            serializer.Setup(s => s.DeserializeHeaders(It.IsAny<SerializedData>()))
                .Returns(() => NextMessageHeaders);

            NextUserMessage = new();
            serializer.Setup(s => s.DeserializeMessage(It.IsAny<SerializedData>(), typeof(TestSagaMessage1)))
                .Returns(() => NextUserMessage);
        }

        /// <summary>
        /// Proves the message is retried when failed.
        /// </summary>
        /// <returns></returns>
        [TestMethod]
        public async Task Fail_UpdatesMessage()
        {
            var expectedRetries = (byte)(reader.MaxRetries - 1);
            var context = new InternalSubscribedContext
            {
                MessageData = new SubscribedMessage
                {
                    Retries = (byte)(reader.MaxRetries - 2),
                },
                Headers = new Headers
                {

                }
            };

            await reader.Fail(context, new ApplicationException());

            Assert.AreEqual(expectedRetries, context.MessageData.Retries);
            Assert.IsTrue(context.MessageData.NotBefore >= clock.Object.UtcNow.AddSeconds(5)); // 
            Assert.IsFalse(string.IsNullOrWhiteSpace(context.Headers.ExceptionDetails));
            counters.Verify(c => c.RetryMessage(), Times.Once);
            dataAccess.Verify(c => c.Update(It.IsAny<SubscribedMessage>()), Times.Once);
            Assert.IsTrue(ReferenceEquals(context.MessageData, UpdatedMessage));
        }

        /// <summary>
        /// proves the message is failed after retries
        /// </summary>
        /// <returns></returns>
        [TestMethod]
        public async Task Fail_FailsMaxReties()
        {
            var expectedRetries = (byte)(reader.MaxRetries);
            var context = new InternalSubscribedContext
            {
                MessageData = new SubscribedMessage
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
            counters.Verify(c => c.FailMessage(), Times.Once);
            dataAccess.Verify(c => c.FailMessage(It.IsAny<SubscribedMessage>()), Times.Once);
            Assert.IsTrue(ReferenceEquals(context.MessageData, FailedMessage));
        }

        /// <summary>
        /// Proves completed messages are completed
        /// </summary>
        /// <returns></returns>
        [TestMethod]
        public async Task Complete_Completes()
        {
            var now = clock.Object.UtcNow;
            var context = new InternalSubscribedContext
            {
                MessageData = new SubscribedMessage
                {
                    Id = 12345,
                    NotBefore = now,
                },
            };
            await reader.Complete(context);

            Assert.IsTrue(ReferenceEquals(context.MessageData, CompletedMessage));
            Assert.AreEqual(now, context.MessageData.Completed);
        }

        [TestMethod]
        public async Task GetNext_ReturnsNull()
        {
            dataAccess.Setup(d => d.GetPendingSubscribed(SubscriberId))
                .ReturnsAsync((SubscribedMessage?)null);
            Assert.IsNull(await reader.GetNext(SubscriberId));
        }

        /// <summary>
        /// Proves reads good messages.
        /// </summary>
        /// <returns></returns>
        [TestMethod]
        public async Task GetNext_GetsGoodMessage()
        {
            var context = await reader.GetNext(SubscriberId);

            Assert.IsNotNull(context);
            Assert.AreSame(NextMessageHeaders, context.Headers);
            Assert.AreSame(NextMessage, context.MessageData);
            Assert.AreSame(NextUserMessage, context.Message);
            Assert.AreEqual(SubscriberId, context.SubscriberId);
            Assert.AreEqual(NextMessage.MessageId, context.MessageId);
            Assert.AreEqual(NextMessage.Priority, context.MessagePriority);
        }

        /// <summary>
        /// Proves when headers are unserializable
        /// </summary>
        /// <returns></returns>
        [TestMethod]
        public async Task GetNext_HandlesUndeserializableHeaders()
        {
            serializer.Setup(s => s.DeserializeHeaders(It.IsAny<SerializedData>()))
               .Throws(new Exception("Test Exception"));

            var context = await reader.GetNext(SubscriberId);

            Assert.IsNotNull(context);
            Assert.AreSame("System.Object", context.Headers.MessageClass);
            Assert.AreSame(NextMessage, context.MessageData);
            Assert.IsNull(context.Message);
            Assert.AreEqual(SubscriberId, context.SubscriberId);
            Assert.AreEqual(NextMessage.MessageId, context.MessageId);
            Assert.AreEqual(NextMessage.Priority, context.MessagePriority);
        }

        /// <summary>
        /// Proves when message class is unrecognized.
        /// </summary>
        /// <returns></returns>
        [TestMethod]
        public async Task GetNext_HandlesUnrecognizedMessageClass()
        {
            NextMessageHeaders.MessageClass =
                "PeachtreeBus.Tests.Sagas.TestSagaNotARealMessage, PeachtreeBus.Tests";

            var context = await reader.GetNext(SubscriberId);

            Assert.IsNotNull(context);
            Assert.AreSame(NextMessageHeaders, context.Headers);
            Assert.AreSame(NextMessage, context.MessageData);
            Assert.IsNull(context.Message);
            Assert.AreEqual(SubscriberId, context.SubscriberId);
            Assert.AreEqual(NextMessage.MessageId, context.MessageId);
            Assert.AreEqual(NextMessage.Priority, context.MessagePriority);
        }

        /// <summary>
        /// Proves when body is unserializable
        /// </summary>
        /// <returns></returns>
        [TestMethod]
        public async Task GetNext_HandlesUnserializableMessageBody()
        {
            serializer.Setup(s => s.DeserializeMessage(It.IsAny<SerializedData>(), typeof(TestSagaMessage1)))
                .Throws(new Exception("Test Exception"));

            var context = await reader.GetNext(SubscriberId);

            Assert.IsNotNull(context);
            Assert.AreSame(NextMessageHeaders, context.Headers);
            Assert.AreSame(NextMessage, context.MessageData);
            Assert.IsNull(context.Message);
            Assert.AreEqual(SubscriberId, context.SubscriberId);
            Assert.AreEqual(NextMessage.MessageId, context.MessageId);
            Assert.AreEqual(NextMessage.Priority, context.MessagePriority);
        }


        [TestMethod]
        public async Task Fail_InvokesFailHandlerOnMaxRetries()
        {
            var context = new InternalSubscribedContext
            {
                MessageData = new SubscribedMessage
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
            var context = new InternalSubscribedContext
            {
                MessageData = new SubscribedMessage
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