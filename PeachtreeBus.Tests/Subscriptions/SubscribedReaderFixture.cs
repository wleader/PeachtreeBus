using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using PeachtreeBus.Data;
using PeachtreeBus.Errors;
using PeachtreeBus.Serialization;
using PeachtreeBus.Subscriptions;
using PeachtreeBus.Tests.Sagas;
using System;
using System.Threading.Tasks;
using RetryResult = PeachtreeBus.Errors.RetryResult;

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
        private Mock<ISubscribedRetryStrategy> retryStrategy = default!;

        private static readonly SubscriberId SubscriberId = new(Guid.Parse("5d7ece7e-b9eb-4b97-91fa-af6bfe50394a"));

        private SubscribedData NextMessage = default!;
        private Headers NextMessageHeaders = default!;
        private TestSagaMessage1 NextUserMessage = default!;
        private RetryResult RetryResult = default!;
        private SubscribedContext Context = default!;
        private SerializedData SerializedHeaderData = default!;

        [TestInitialize]
        public void TestInitialize()
        {
            dataAccess = new();
            log = new();
            counters = new();
            serializer = new();
            clock = new();
            failures = new();
            retryStrategy = new();

            clock.SetupGet(x => x.UtcNow)
                .Returns(new DateTime(2022, 2, 22, 14, 22, 22, 222, DateTimeKind.Utc));

            reader = new SubscribedReader(
                dataAccess.Object,
                serializer.Object,
                log.Object,
                counters.Object,
                clock.Object,
                failures.Object,
                retryStrategy.Object);

            NextMessage = TestData.CreateSubscribedData(id: new(12345), priority: 24,
                subscriberId: SubscriberId);

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

            SerializedHeaderData = new("SerializedHeaderData");
            serializer.Setup(s => s.SerializeHeaders(It.IsAny<Headers>()))
                .Returns(() => SerializedHeaderData);

            retryStrategy.Setup(r => r.DetermineRetry(It.IsAny<SubscribedContext>(), It.IsAny<Exception>(), It.IsAny<FailureCount>()))
                .Returns(() => RetryResult);

            Context = TestData.CreateSubscribedContext(
                messageData: TestData.CreateSubscribedData(id: new(1234), notBefore: clock.Object.UtcNow));
        }

        /// <summary>
        /// Proves the message is retried when failed.
        /// </summary>
        /// <returns></returns>
        [TestMethod]
        public async Task Given_RetryStrategyReturnsRetry_When_Fail_Then_MessageIsUpdated()
        {
            var delay = TimeSpan.FromSeconds(5);
            RetryResult = new(true, delay);
            UtcDateTime expectedNotBefore = clock.Object.UtcNow.Add(delay);
            var expectedId = Context.Data.Id;

            var exception = new ApplicationException();

            serializer.Setup(s => s.SerializeHeaders(Context.InternalHeaders))
                .Callback((Headers h) =>
                {
                    Assert.AreEqual(exception.ToString(), h.ExceptionDetails);
                })
                .Returns(() => SerializedHeaderData);

            dataAccess.Setup(d => d.UpdateMessage(Context.Data))
                .Callback((SubscribedData m) =>
                {
                    Assert.AreEqual(expectedId, m.Id);
                    Assert.AreEqual(1, m.Retries);
                    Assert.AreEqual(expectedNotBefore, m.NotBefore);
                    Assert.AreEqual(SerializedHeaderData, m.Headers);
                });

            await reader.Fail(Context, exception);

            serializer.Verify(s => s.SerializeHeaders(Context.InternalHeaders), Times.Once);
            counters.Verify(c => c.RetryMessage(), Times.Once);
            dataAccess.Verify(c => c.UpdateMessage(Context.Data), Times.Once);
        }

        /// <summary>
        /// proves the message is failed after retries
        /// </summary>
        /// <returns></returns>
        [TestMethod]
        public async Task Given_RetryStrategyReturnsFail_When_Fail_Then_MessageIsFailed()
        {
            RetryResult = new(false, TimeSpan.Zero);
            var exception = new ApplicationException();
            var expectedId = Context.Data.Id;

            serializer.Setup(s => s.SerializeHeaders(Context.InternalHeaders))
                .Callback((Headers h) =>
                {
                    Assert.AreEqual(exception.ToString(), h.ExceptionDetails);
                })
                .Returns(() => SerializedHeaderData);

            dataAccess.Setup(d => d.FailMessage(Context.Data))
                .Callback((SubscribedData m) =>
                {
                    Assert.AreEqual(expectedId, m.Id);
                    Assert.AreEqual(SerializedHeaderData, m.Headers);
                });

            await reader.Fail(Context, new ApplicationException());

            counters.Verify(c => c.FailMessage(), Times.Once);
            serializer.Verify(s => s.SerializeHeaders(Context.InternalHeaders), Times.Once);
            dataAccess.Verify(c => c.FailMessage(It.IsAny<SubscribedData>()), Times.Once);
            Assert.AreEqual(1, dataAccess.Invocations.Count);
        }

        /// <summary>
        /// Proves completed messages are completed
        /// </summary>
        /// <returns></returns>
        [TestMethod]
        public async Task Complete_Completes()
        {
            var now = clock.Object.UtcNow;
            var expectedMessageId = Context.Data.Id;

            dataAccess.Setup(d => d.CompleteMessage(Context.Data))
                .Callback((SubscribedData m) =>
                {
                    Assert.AreEqual(expectedMessageId, m.Id);
                    Assert.AreEqual(clock.Object.UtcNow, m.Completed);
                });

            await reader.Complete(Context);

            dataAccess.Verify(d => d.CompleteMessage(Context.Data));

            Assert.AreEqual(now, Context.Data.Completed);
        }

        [TestMethod]
        public async Task GetNext_ReturnsNull()
        {
            dataAccess.Setup(d => d.GetPendingSubscribed(SubscriberId))
                .ReturnsAsync((SubscribedData?)null);
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
            Assert.AreSame(NextMessageHeaders, context.InternalHeaders);
            Assert.AreSame(NextMessage, context.Data);
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
            Assert.AreSame("System.Object", context.MessageClass);
            Assert.AreSame(NextMessage, context.Data);
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
            Assert.AreSame(NextMessageHeaders, context.InternalHeaders);
            Assert.AreSame(NextMessage, context.Data);
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
            Assert.AreSame(NextMessageHeaders, context.InternalHeaders);
            Assert.AreSame(NextMessage, context.Data);
            Assert.IsNull(context.Message);
            Assert.AreEqual(SubscriberId, context.SubscriberId);
            Assert.AreEqual(NextMessage.MessageId, context.MessageId);
            Assert.AreEqual(NextMessage.Priority, context.MessagePriority);
        }


        [TestMethod]
        public async Task Given_RetryStrategyRerturnsFail_When_Fail_Then_ErrorHanderIsInvoked()
        {
            RetryResult = new(false, TimeSpan.Zero);
            var exception = new ApplicationException();
            await reader.Fail(Context, exception);
            failures.Verify(f => f.Failed(Context, Context.Message, exception), Times.Once());
        }

        [TestMethod]
        public async Task Given_RetryStrategyRerturnsRetry_When_Fail_Then_ErrorHanderIsNotInvoked()
        {
            RetryResult = new(true, TimeSpan.FromSeconds(5));
            var exception = new ApplicationException();
            await reader.Fail(Context, exception);
            failures.Verify(f => f.Failed(Context, Context.Message, exception), Times.Never());
        }
    }
}