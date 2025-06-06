﻿using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using PeachtreeBus.ClassNames;
using PeachtreeBus.Core.Tests.Sagas;
using PeachtreeBus.Data;
using PeachtreeBus.Errors;
using PeachtreeBus.Serialization;
using PeachtreeBus.Subscriptions;
using PeachtreeBus.Telemetry;
using System;
using System.Threading.Tasks;
using RetryResult = PeachtreeBus.Errors.RetryResult;

namespace PeachtreeBus.Core.Tests.Subscriptions
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
        private Mock<IMeters> meters = default!;
        private Mock<ISerializer> serializer = default!;
        private Mock<ISystemClock> clock = default!;
        private Mock<ISubscribedFailures> failures = default!;
        private Mock<ISubscribedRetryStrategy> retryStrategy = default!;
        private readonly ClassNameService classNameService = new();

        private static readonly SubscriberId SubscriberId = new(Guid.Parse("5d7ece7e-b9eb-4b97-91fa-af6bfe50394a"));

        private SubscribedData NextMessage = default!;
        private Headers NextMessageHeaders = default!;
        private TestSagaMessage1 NextUserMessage = default!;
        private RetryResult RetryResult = default!;
        private SubscribedContext Context = default!;

        [TestInitialize]
        public void TestInitialize()
        {
            dataAccess = new();
            log = new();
            meters = new();
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
                meters.Object,
                clock.Object,
                failures.Object,
                retryStrategy.Object,
                classNameService);

            NextMessageHeaders = new()
            {
                MessageClass = new("PeachtreeBus.Core.Tests.Sagas.TestSagaMessage1, PeachtreeBus.Core.Tests")
            };

            NextMessage = TestData.CreateSubscribedData(id: new(12345), priority: 24,
                subscriberId: SubscriberId,
                headers: NextMessageHeaders);

            dataAccess.Setup(d => d.GetPendingSubscribed(SubscriberId))
                .ReturnsAsync(() => NextMessage);

            NextUserMessage = new();
            serializer.Setup(s => s.Deserialize(It.IsAny<SerializedData>(), typeof(TestSagaMessage1)))
                .Returns(() => NextUserMessage);

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

            dataAccess.Setup(d => d.UpdateMessage(Context.Data))
                .Callback((SubscribedData m) =>
                {
                    Assert.AreEqual(expectedId, m.Id);
                    Assert.AreEqual(1, m.Retries);
                    Assert.AreEqual(expectedNotBefore, m.NotBefore);
                    Assert.AreSame(Context.Data, m);
                    Assert.AreSame(Context.Headers, m.Headers);
                    Assert.AreEqual(exception.ToString(), m.Headers?.ExceptionDetails);
                });

            await reader.Fail(Context, exception);

            meters.Verify(c => c.RetryMessage(), Times.Once);
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

            dataAccess.Setup(d => d.FailMessage(Context.Data))
                .Callback((SubscribedData m) =>
                {
                    Assert.AreSame(Context.Data, m);
                    Assert.AreEqual(expectedId, m.Id);
                    Assert.AreSame(Context.Headers, m.Headers);
                    Assert.AreEqual(exception.ToString(), m.Headers?.ExceptionDetails);
                });

            await reader.Fail(Context, new ApplicationException());

            meters.Verify(c => c.FailMessage(), Times.Once);
            dataAccess.Verify(c => c.FailMessage(It.IsAny<SubscribedData>()), Times.Once);
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
            Assert.AreSame(NextMessageHeaders, context.Headers);
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
            NextMessage.Headers = null!;

            var context = await reader.GetNext(SubscriberId);

            Assert.IsNotNull(context);
            Assert.AreEqual(ClassName.Default, context.MessageClass);
            Assert.AreSame(NextMessage, context.Data);
            Assert.IsNull(context?.Message);
            Assert.AreEqual(SubscriberId, context!.SubscriberId);
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
                new("PeachtreeBus.Tests.Sagas.TestSagaNotARealMessage, PeachtreeBus.Tests");

            var context = await reader.GetNext(SubscriberId);

            Assert.IsNotNull(context);
            Assert.AreSame(NextMessageHeaders, context.Headers);
            Assert.AreSame(NextMessage, context.Data);
            Assert.IsNull(context?.Message);
            Assert.AreEqual(SubscriberId, context!.SubscriberId);
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
            serializer.Setup(s => s.Deserialize(It.IsAny<SerializedData>(), typeof(TestSagaMessage1)))
                .Throws(new Exception("Test Exception"));

            var context = await reader.GetNext(SubscriberId);

            Assert.IsNotNull(context);
            Assert.AreSame(NextMessageHeaders, context.Headers);
            Assert.AreSame(NextMessage, context.Data);
            Assert.IsNull(context?.Message);
            Assert.AreEqual(SubscriberId, context!.SubscriberId);
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