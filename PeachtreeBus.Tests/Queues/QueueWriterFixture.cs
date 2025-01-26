﻿using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using PeachtreeBus.Data;
using PeachtreeBus.Interfaces;
using PeachtreeBus.Queues;
using PeachtreeBus.Tests.Sagas;
using System;
using System.Threading.Tasks;

namespace PeachtreeBus.Tests.Queues
{
    /// <summary>
    /// Proves the behavior of QueueWriter
    /// </summary>
    [TestClass]
    public class QueueWriterFixture
    {
        public class MessageWithoutInterface { }

        private static readonly SerializedData MessageData = new("SerializedMessage");
        private static readonly SerializedData HeaderData = new("SerializedHeaders");
        private QueueWriter writer = default!;
        private Mock<IBusDataAccess> dataAccess = default!;
        private Mock<IPerfCounters> counters = default!;
        private Mock<ISerializer> serializer = default!;
        private Mock<ISystemClock> clock = default!;

        private QueueMessage AddedMessage = null!;
        private QueueName AddedToQueue = default!;
        private Headers SerializedHeaders = null!;
        private readonly UtcDateTime _now = new DateTime(2022, 2, 23, 10, 49, 32, 33, DateTimeKind.Utc);
        private readonly QueueName queueName = new("QueueName");

        [TestInitialize]
        public void TestInitialize()
        {
            dataAccess = new Mock<IBusDataAccess>();
            counters = new Mock<IPerfCounters>();
            serializer = new Mock<ISerializer>();
            clock = new Mock<ISystemClock>();

            clock.SetupGet(c => c.UtcNow).Returns(() => _now.Value);

            dataAccess.Setup(d => d.AddMessage(It.IsAny<QueueMessage>(), It.IsAny<QueueName>()))
                .Callback<QueueMessage, QueueName>((msg, qn) =>
                {
                    AddedMessage = msg;
                    AddedToQueue = qn;
                })
                .Returns(Task.FromResult<Identity>(new(12345)));

            serializer.Setup(s => s.SerializeHeaders(It.IsAny<Headers>()))
                .Callback<Headers>(h => SerializedHeaders = h)
                .Returns(HeaderData);

            serializer.Setup(s => s.SerializeMessage(It.IsAny<object>(), It.IsAny<Type>()))
                .Returns(MessageData);

            writer = new QueueWriter(dataAccess.Object, counters.Object, serializer.Object, clock.Object);
        }

        /// <summary>
        /// Proves the message cannot be null.
        /// </summary>
        /// <returns></returns>
        [TestMethod]
        public async Task WriteMessage_ThrowsWhenMessageIsNull()
        {
            await Assert.ThrowsExceptionAsync<ArgumentNullException>(() =>
                writer.WriteMessage(
                    queueName,
                    typeof(TestSagaMessage1),
                    null!,
                    null));
        }

        /// <summary>
        /// Proves the type cannot be null
        /// </summary>
        /// <returns></returns>
        [TestMethod]
        public async Task WriteMessage_ThrowsWhenTypeIsNull()
        {
            await Assert.ThrowsExceptionAsync<ArgumentNullException>(() =>
                writer.WriteMessage(
                    queueName,
                    null!,
                    new TestSagaMessage1(),
                    null));
        }

        /// <summary>
        /// proves the message class is set.
        /// </summary>
        /// <returns></returns>
        [TestMethod]
        public async Task WriteMessage_SetsMessageClassOfHeaders()
        {
            await writer.WriteMessage(
                queueName,
                typeof(TestSagaMessage1),
                new TestSagaMessage1(),
                null);

            Assert.IsNotNull(SerializedHeaders);
            Assert.AreEqual("PeachtreeBus.Tests.Sagas.TestSagaMessage1, PeachtreeBus.Tests", SerializedHeaders.MessageClass);
        }

        /// <summary>
        /// Proves that NotBefore defaults to Now
        /// </summary>
        /// <returns></returns>
        [TestMethod]
        public async Task WriteMessage_DefaultsNotBeforeToUtcNow()
        {
            await writer.WriteMessage(
                queueName,
                typeof(TestSagaMessage1),
                new TestSagaMessage1(),
                null);

            Assert.IsNotNull(AddedMessage);
            Assert.AreEqual(_now, AddedMessage.NotBefore);
        }

        /// <summary>
        /// Proves the supplied NotBefore is used
        /// </summary>
        /// <returns></returns>
        [TestMethod]
        public async Task WriteMessage_UsesProvidedNotBefore()
        {
            UtcDateTime notBefore = DateTime.UtcNow;
            await writer.WriteMessage(
                            queueName,
                            typeof(TestSagaMessage1),
                            new TestSagaMessage1(),
                            notBefore);

            Assert.IsNotNull(AddedMessage);
            Assert.AreEqual(notBefore, AddedMessage.NotBefore);
        }

        /// <summary>
        /// proves NotBefore DateTimeKind is requried.
        /// </summary>
        /// <returns></returns>
        [TestMethod]
        public async Task WriteMessage_ThrowsWhenNotBeforeKindUnspecified()
        {
            var notBefore = new DateTime(2022, 2, 23, 10, 54, 11, DateTimeKind.Unspecified);
            await Assert.ThrowsExceptionAsync<ArgumentException>(() =>
                writer.WriteMessage(
                    queueName,
                    typeof(TestSagaMessage1),
                    new TestSagaMessage1(),
                    notBefore));
        }

        /// <summary>
        /// Proves Enqueued is set to now
        /// </summary>
        /// <returns></returns>
        [TestMethod]
        public async Task WriteMessage_SetsEnqueuedToUtcNow()
        {
            await writer.WriteMessage(
                queueName,
                typeof(TestSagaMessage1),
                new TestSagaMessage1(),
                null);

            Assert.IsNotNull(AddedMessage);
            Assert.AreEqual(_now, AddedMessage.Enqueued);
        }

        /// <summary>
        /// Proves completed defaults to null
        /// </summary>
        /// <returns></returns>
        [TestMethod]
        public async Task WriteMessage_SetsCompletedToNull()
        {
            await writer.WriteMessage(
                queueName,
                typeof(TestSagaMessage1),
                new TestSagaMessage1(),
                null);

            Assert.IsNotNull(AddedMessage);
            Assert.IsFalse(AddedMessage.Completed.HasValue);
        }

        /// <summary>
        /// Proves failed defaults to null
        /// </summary>
        /// <returns></returns>
        [TestMethod]
        public async Task WriteMessage_SetsFailedToNull()
        {
            await writer.WriteMessage(
                queueName,
                typeof(TestSagaMessage1),
                new TestSagaMessage1(),
                null);

            Assert.IsNotNull(AddedMessage);
            Assert.IsFalse(AddedMessage.Failed.HasValue);
        }

        /// <summary>
        /// Proves retries defaults to zero
        /// </summary>
        /// <returns></returns>
        [TestMethod]
        public async Task WriteMessage_SetsRetriesToZero()
        {
            await writer.WriteMessage(
                queueName,
                typeof(TestSagaMessage1),
                new TestSagaMessage1(),
                null);

            Assert.IsNotNull(AddedMessage);
            Assert.AreEqual(0, AddedMessage.Retries);
        }

        /// <summary>
        /// Proves headers are serialized.
        /// </summary>
        /// <returns></returns>
        [TestMethod]
        public async Task WriteMessage_UsesHeadersFromSerializer()
        {
            await writer.WriteMessage(
                queueName,
                typeof(TestSagaMessage1),
                new TestSagaMessage1(),
                null);

            Assert.IsNotNull(AddedMessage);
            Assert.AreEqual(HeaderData, AddedMessage.Headers);
        }

        /// <summary>
        /// proves body is serialized.
        /// </summary>
        /// <returns></returns>
        [TestMethod]
        public async Task WriteMessage_UsesBodyFromSerializer()
        {
            await writer.WriteMessage(
                queueName,
                typeof(TestSagaMessage1),
                new TestSagaMessage1(),
                null);

            Assert.IsNotNull(AddedMessage);
            Assert.AreEqual(MessageData, AddedMessage.Body);
        }

        /// <summary>
        /// Proves counters are invoked
        /// </summary>
        /// <returns></returns>
        [TestMethod]
        public async Task WriteMessage_CountSentMessages()
        {
            await writer.WriteMessage(
                queueName,
                typeof(TestSagaMessage1),
                new TestSagaMessage1(),
                null);

            counters.Verify(c => c.SentMessage(), Times.Once);
        }

        /// <summary>
        /// proves Data Access add message is used.
        /// </summary>
        /// <returns></returns>
        [TestMethod]
        public async Task WriteMessage_InvokesDataAccess()
        {
            await writer.WriteMessage(
                queueName,
                typeof(TestSagaMessage1),
                new TestSagaMessage1(),
                null);

            dataAccess.Verify(d => d.AddMessage(It.IsAny<QueueMessage>(), queueName), Times.Once);
        }

        /// <summary>
        /// Proves the correct queue is used.
        /// </summary>
        /// <returns></returns>
        [TestMethod]
        public async Task WriteMessage_SendsToCorrectQueue()
        {
            var expected = new QueueName("FooBazQueue");
            await writer.WriteMessage(
                expected,
                typeof(TestSagaMessage1),
                new TestSagaMessage1(),
                null);
            Assert.AreEqual(expected, AddedToQueue);
        }

        [TestMethod]
        public async Task Given_MessageIsNotIQueuedMessage_When_WriteMessage_Then_ThrowsUsefulException()
        {
            await Assert.ThrowsExceptionAsync<TypeIsNotIQueueMessageException>(() =>
                writer.WriteMessage(new("FooBazQueue"),
                typeof(MessageWithoutInterface),
                new MessageWithoutInterface(),
                null));
        }
    }
}
