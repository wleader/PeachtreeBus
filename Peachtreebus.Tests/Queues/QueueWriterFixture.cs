using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Peachtreebus.Tests.Sagas;
using PeachtreeBus;
using PeachtreeBus.Data;
using PeachtreeBus.Interfaces;
using PeachtreeBus.Model;
using PeachtreeBus.Queues;
using System;
using System.Threading.Tasks;

namespace Peachtreebus.Tests.Queues
{
    /// <summary>
    /// Proves the behavior of QueueWriter
    /// </summary>
    [TestClass]
    public class QueueWriterFixture
    {
        private QueueWriter writer;
        private Mock<IBusDataAccess> dataAccess;
        private Mock<IPerfCounters> counters;
        private Mock<ISerializer> serializer;
        private Mock<ISystemClock> clock;

        private QueueMessage AddedMessage = null;
        private string AddedToQueue = null;
        private Headers SerializedHeaders = null;

        [TestInitialize]
        public void TestInitialize()
        {
            dataAccess = new Mock<IBusDataAccess>();
            counters = new Mock<IPerfCounters>();
            serializer = new Mock<ISerializer>();
            clock = new Mock<ISystemClock>();

            clock.SetupGet(c => c.UtcNow)
                .Returns(new DateTime(2022, 2, 23, 10, 49, 32, 33, DateTimeKind.Utc));

            dataAccess.Setup(d => d.AddMessage(It.IsAny<QueueMessage>(), It.IsAny<string>()))
                .Callback<QueueMessage, string>((msg, qn) =>
                {
                    AddedMessage = msg;
                    AddedToQueue = qn;
                })
                .Returns(Task.FromResult<long>(12345));

            serializer.Setup(s => s.SerializeHeaders(It.IsAny<Headers>()))
                .Callback<Headers>(h => SerializedHeaders = h)
                .Returns("SerialziedHeaders");

            serializer.Setup(s => s.SerializeMessage(It.IsAny<object>(), It.IsAny<Type>()))
                .Returns("SerialziedMessage");

            writer = new QueueWriter(dataAccess.Object, counters.Object, serializer.Object, clock.Object);
        }

        /// <summary>
        /// Proves the message cannot be null.
        /// </summary>
        /// <returns></returns>
        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task WriteMessage_ThrowsWhenMessageIsNull()
        {
            await writer.WriteMessage(
                "QueueName",
                typeof(TestSagaMessage1),
                null,
                null);
        }

        /// <summary>
        /// Proves the type cannot be null
        /// </summary>
        /// <returns></returns>
        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task WriteMessage_ThrowsWhenTypeIsNull()
        {
            await writer.WriteMessage(
                "QueueName",
                null,
                new TestSagaMessage1(),
                null);
        }

        /// <summary>
        /// Proves the queue name cannot be null.
        /// </summary>
        /// <returns></returns>
        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task WriteMessage_ThrowsWhenQueueNameIsNull()
        {
            await writer.WriteMessage(
                null,
                typeof(TestSagaMessage1),
                new TestSagaMessage1(),
                null);
        }

        /// <summary>
        /// proves the queue name can not be empty
        /// </summary>
        /// <returns></returns>
        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task WriteMessage_ThrowsWhenQueueNameIsEmpty()
        {
            await writer.WriteMessage(
                "",
                typeof(TestSagaMessage1),
                new TestSagaMessage1(),
                null);
        }

        /// <summary>
        /// proves the queue name cannot be whitespace
        /// </summary>
        /// <returns></returns>
        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task WriteMessage_ThrowsWhenQueueNameIsWhitespace()
        {
            await writer.WriteMessage(
                " ",
                typeof(TestSagaMessage1),
                new TestSagaMessage1(),
                null);
        }

        /// <summary>
        /// proves the message class is set.
        /// </summary>
        /// <returns></returns>
        [TestMethod]
        public async Task WriteMessage_SetsMessageClassOfHeaders()
        {
            await writer.WriteMessage(
                "QueueName",
                typeof(TestSagaMessage1),
                new TestSagaMessage1(),
                null);

            Assert.IsNotNull(SerializedHeaders);
            Assert.AreEqual("Peachtreebus.Tests.Sagas.TestSagaMessage1, Peachtreebus.Tests", SerializedHeaders.MessageClass);
        }

        /// <summary>
        /// Proves tha a message ID is generated
        /// </summary>
        /// <returns></returns>
        [TestMethod]
        public async Task WriteMessage_AssignsMessageId()
        {
            await writer.WriteMessage(
                "QueueName",
                typeof(TestSagaMessage1),
                new TestSagaMessage1(),
                null);

            Assert.IsNotNull(AddedMessage);
            Assert.AreNotEqual(Guid.Empty, AddedMessage.MessageId);
        }

        /// <summary>
        /// Proves that NotBefore defaults to Now
        /// </summary>
        /// <returns></returns>
        [TestMethod]
        public async Task WriteMessage_DefaultsNotBeforeToUtcNow()
        {
            await writer.WriteMessage(
                "QueueName",
                typeof(TestSagaMessage1),
                new TestSagaMessage1(),
                null);

            Assert.IsNotNull(AddedMessage);
            Assert.AreEqual(clock.Object.UtcNow, AddedMessage.NotBefore);
        }

        /// <summary>
        /// Proves the supplied NotBefore is used
        /// </summary>
        /// <returns></returns>
        [TestMethod]
        public async Task WriteMessage_UsesProvidedNotBefore()
        {
            var notBefore = DateTime.UtcNow;
            await writer.WriteMessage(
                            "QueueName",
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
        [ExpectedException(typeof(ArgumentException))]
        public async Task WriteMessage_ThrowsWhenNotBeforeKindUnspecified()
        {
            var notBefore = new DateTime(2022, 2, 23, 10, 54, 11, DateTimeKind.Unspecified);
            await writer.WriteMessage(
                            "QueueName",
                            typeof(TestSagaMessage1),
                            new TestSagaMessage1(),
                            notBefore);
        }

        /// <summary>
        /// Proves Enqueued is set to now
        /// </summary>
        /// <returns></returns>
        [TestMethod]
        public async Task WriteMessage_SetsEnqueuedToUtcNow()
        {
            await writer.WriteMessage(
                "QueueName",
                typeof(TestSagaMessage1),
                new TestSagaMessage1(),
                null);

            Assert.IsNotNull(AddedMessage);
            Assert.AreEqual(clock.Object.UtcNow, AddedMessage.Enqueued);
        }

        /// <summary>
        /// Proves completed defaults to null
        /// </summary>
        /// <returns></returns>
        [TestMethod]
        public async Task WriteMessage_SetsCompletedToNull()
        {
            await writer.WriteMessage(
                "QueueName",
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
                "QueueName",
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
                "QueueName",
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
                "QueueName",
                typeof(TestSagaMessage1),
                new TestSagaMessage1(),
                null);

            Assert.IsNotNull(AddedMessage);
            Assert.AreEqual("SerialziedHeaders", AddedMessage.Headers);
        }

        /// <summary>
        /// proves body is serialized.
        /// </summary>
        /// <returns></returns>
        [TestMethod]
        public async Task WriteMessage_UsesBodyFromSerializer()
        {
            await writer.WriteMessage(
                "QueueName",
                typeof(TestSagaMessage1),
                new TestSagaMessage1(),
                null);

            Assert.IsNotNull(AddedMessage);
            Assert.AreEqual("SerialziedMessage", AddedMessage.Body);
        }

        /// <summary>
        /// Proves counters are invoked
        /// </summary>
        /// <returns></returns>
        [TestMethod]
        public async Task WriteMessage_CountSentMessages()
        {
            await writer.WriteMessage(
                "QueueName",
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
                "QueueName",
                typeof(TestSagaMessage1),
                new TestSagaMessage1(),
                null);

            dataAccess.Verify(d => d.AddMessage(It.IsAny<QueueMessage>(), "QueueName"), Times.Once);
        }

        /// <summary>
        /// Proves the correct queue is used.
        /// </summary>
        /// <returns></returns>
        [TestMethod]
        public async Task WriteMessage_SendsToCorrectQueue()
        {
            await writer.WriteMessage(
                "FooBazQueue",
                typeof(TestSagaMessage1),
                new TestSagaMessage1(),
                null);

            Assert.AreEqual("FooBazQueue", AddedToQueue);
        }
    }
}
