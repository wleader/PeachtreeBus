﻿using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Peachtreebus.Tests.Sagas;
using PeachtreeBus;
using PeachtreeBus.Data;
using PeachtreeBus.Errors;
using PeachtreeBus.Interfaces;
using PeachtreeBus.Model;
using PeachtreeBus.Subscriptions;
using System;
using System.Text.Json;
using System.Threading.Tasks;

namespace Peachtreebus.Tests.Subscriptions
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

        /// <summary>
        /// Proves reads good messages.
        /// </summary>
        /// <returns></returns>
        [TestMethod]
        public async Task GetNext_GetsGoodMessage()
        {
            var messageClass = typeof(TestSagaMessage1).FullName + ", " + typeof(TestSagaMessage1).Assembly.GetName().Name;

            var expectedMessage = new SubscribedMessage
            {
                Headers = "{ \"MessageClass\":\"" + messageClass + "\"}"
            };

            var expectedHeaders = new Headers
            {
                MessageClass = messageClass,
                ExceptionDetails = null
            };

            var expectedUserMessage = new TestSagaMessage1();

            var subscriberId = Guid.NewGuid();

            dataAccess.Setup(d => d.GetPendingSubscribed(subscriberId))
                .ReturnsAsync(expectedMessage);

            serializer.Setup(s => s.DeserializeHeaders(It.IsAny<string>()))
               .Returns(expectedHeaders);

            serializer.Setup(s => s.DeserializeMessage(It.IsAny<string>(), typeof(TestSagaMessage1)))
                .Returns(expectedUserMessage);

            var context = await reader.GetNext(subscriberId);
            Assert.IsNotNull(context);
            Assert.IsTrue(ReferenceEquals(expectedMessage, context.MessageData));
            Assert.IsTrue(ReferenceEquals(expectedHeaders, context.Headers));
            Assert.IsTrue(ReferenceEquals(expectedUserMessage, context.Message));
            Assert.AreEqual(expectedMessage.MessageId, context.MessageId);
        }

        /// <summary>
        /// Proves when headers are unserializable
        /// </summary>
        /// <returns></returns>
        [TestMethod]
        public async Task GetNext_HandlesUndeserializableHeaders()
        {
            var messageClass = typeof(TestSagaMessage1).FullName + ", " + typeof(TestSagaMessage1).Assembly.GetName().Name;

            var expectedMessage = new SubscribedMessage
            {
                Headers = "{ \"MessageClass\":\"" + messageClass + "\"}"
            };

            var expectedHeaders = new Headers
            {
                MessageClass = messageClass,
                ExceptionDetails = null
            };

            var expectedUserMessage = new TestSagaMessage1();

            var subscriberId = Guid.NewGuid();

            dataAccess.Setup(d => d.GetPendingSubscribed(subscriberId))
                .ReturnsAsync(expectedMessage);

            serializer.Setup(s => s.DeserializeHeaders(It.IsAny<string>()))
               .Throws(new JsonException());

            serializer.Setup(s => s.DeserializeMessage(It.IsAny<string>(), typeof(TestSagaMessage1)))
                .Returns(expectedUserMessage);

            var message = await reader.GetNext(subscriberId);
            Assert.IsNotNull(message);
            Assert.IsTrue(ReferenceEquals(expectedMessage, message.MessageData));
            Assert.IsNotNull(message.Headers);
            Assert.IsNull(message.Message);
        }

        /// <summary>
        /// Proves when message class is unrecognized.
        /// </summary>
        /// <returns></returns>
        [TestMethod]
        public async Task GetNext_HandlesUnrecognizedMessageClass()
        {
            var messageClass = "Peachtreebus.Tests.Sagas.TestSagaNotARealMessage, " + typeof(TestSagaMessage1).Assembly.GetName().Name;

            var expectedMessage = new SubscribedMessage
            {
                Headers = "{ \"MessageClass\":\"" + messageClass + "\"}"
            };

            var expectedHeaders = new Headers
            {
                MessageClass = messageClass,
                ExceptionDetails = null
            };

            var expectedUserMessage = new TestSagaMessage1();

            Guid subscriberId = Guid.NewGuid();

            dataAccess.Setup(d => d.GetPendingSubscribed(subscriberId))
                .ReturnsAsync(expectedMessage);

            serializer.Setup(s => s.DeserializeHeaders(It.IsAny<string>()))
               .Returns(expectedHeaders);

            var message = await reader.GetNext(subscriberId);
            Assert.IsNotNull(message);
            serializer.Verify(s => s.DeserializeMessage(It.IsAny<string>(), It.IsAny<Type>()), Times.Never);
            Assert.IsTrue(ReferenceEquals(expectedMessage, message.MessageData));
            Assert.IsNotNull(message.Headers);
            Assert.IsNull(message.Message);
        }

        /// <summary>
        /// Proves when body is unserializable
        /// </summary>
        /// <returns></returns>
        [TestMethod]
        public async Task GetNext_HandlesUnserializableMessageBody()
        {
            var messageClass = typeof(TestSagaMessage1).FullName + ", " + typeof(TestSagaMessage1).Assembly.GetName().Name;

            var expectedMessage = new SubscribedMessage
            {
                Headers = "{ \"MessageClass\":\"" + messageClass + "\"}"
            };

            var expectedHeaders = new Headers
            {
                MessageClass = messageClass,
                ExceptionDetails = null
            };

            var expectedUserMessage = new TestSagaMessage1();

            var subscriberId = Guid.NewGuid();

            dataAccess.Setup(d => d.GetPendingSubscribed(subscriberId))
                .ReturnsAsync(expectedMessage);

            serializer.Setup(s => s.DeserializeHeaders(It.IsAny<string>()))
               .Returns(expectedHeaders);

            serializer.Setup(s => s.DeserializeMessage(It.IsAny<string>(), typeof(TestSagaMessage1)))
                .Throws(new JsonException());

            var message = await reader.GetNext(subscriberId);
            Assert.IsNotNull(message);
            Assert.IsTrue(ReferenceEquals(expectedMessage, message.MessageData));
            Assert.IsTrue(ReferenceEquals(expectedHeaders, message.Headers));
            Assert.IsNull(message.Message);
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