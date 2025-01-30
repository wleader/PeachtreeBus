using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using PeachtreeBus.Data;
using PeachtreeBus.Interfaces;
using PeachtreeBus.Queues;
using PeachtreeBus.Tests.Fakes;
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

        private QueueWriter writer = default!;
        private Mock<IBusDataAccess> dataAccess = default!;
        private Mock<IPerfCounters> counters = default!;
        private FakeSerializer serializer = default!;
        private Mock<ISystemClock> clock = default!;

        private QueueMessage? AddedMessage = null;
        private QueueName? AddedToQueue = default;
        private object userMessage = default!;

        [TestInitialize]
        public void TestInitialize()
        {
            dataAccess = new();
            counters = new();
            serializer = new();
            clock = new();

            clock.SetupGet(c => c.UtcNow).Returns(() => TestData.Now);

            dataAccess.Setup(d => d.AddMessage(It.IsAny<QueueMessage>(), It.IsAny<QueueName>()))
                .Callback<QueueMessage, QueueName>((msg, qn) =>
                {
                    AddedMessage = msg;
                    AddedToQueue = qn;
                })
                .Returns(Task.FromResult<Identity>(new(12345)));

            userMessage = TestData.CreateQueueUserMessage();

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
                    TestData.DefaultQueueName,
                    userMessage.GetType(),
                    null!));
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
                    TestData.DefaultQueueName,
                    null!,
                    userMessage));
        }

        /// <summary>
        /// proves the message class is set.
        /// </summary>
        /// <returns></returns>
        [TestMethod]
        public async Task WriteMessage_SetsMessageClassOfHeaders()
        {
            await writer.WriteMessage(
                TestData.DefaultQueueName,
                userMessage.GetType(),
                userMessage);
            Assert.AreEqual(1, serializer.SerializedHeaders.Count);
            Assert.AreEqual("PeachtreeBus.Tests.TestData+TestQueuedMessage, PeachtreeBus.Tests",
                serializer.SerializedHeaders[0].MessageClass);
        }

        /// <summary>
        /// Proves that NotBefore defaults to Now
        /// </summary>
        /// <returns></returns>
        [TestMethod]
        public async Task WriteMessage_DefaultsNotBeforeToUtcNow()
        {
            await writer.WriteMessage(
                TestData.DefaultQueueName,
                userMessage.GetType(),
                userMessage);

            Assert.IsNotNull(AddedMessage);
            Assert.AreEqual(TestData.Now, AddedMessage.NotBefore);
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
                TestData.DefaultQueueName,
                userMessage.GetType(),
                userMessage,
                notBefore: notBefore);

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
                    TestData.DefaultQueueName,
                    userMessage.GetType(),
                    userMessage,
                    notBefore: notBefore));
        }

        /// <summary>
        /// Proves Enqueued is set to now
        /// </summary>
        /// <returns></returns>
        [TestMethod]
        public async Task WriteMessage_SetsEnqueuedToUtcNow()
        {
            await writer.WriteMessage(
                TestData.DefaultQueueName,
                userMessage.GetType(),
                userMessage,
                notBefore: null);

            Assert.IsNotNull(AddedMessage);
            Assert.AreEqual(TestData.Now, AddedMessage.Enqueued);
        }

        /// <summary>
        /// Proves completed defaults to null
        /// </summary>
        /// <returns></returns>
        [TestMethod]
        public async Task WriteMessage_SetsCompletedToNull()
        {
            await writer.WriteMessage(
                TestData.DefaultQueueName,
                userMessage.GetType(),
                userMessage);

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
                TestData.DefaultQueueName,
                userMessage.GetType(),
                userMessage);

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
                TestData.DefaultQueueName,
                userMessage.GetType(),
                userMessage);

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
                TestData.DefaultQueueName,
                userMessage.GetType(),
                userMessage);

            Assert.IsNotNull(AddedMessage);
            Assert.AreEqual(serializer.SerializeHeadersResult, AddedMessage.Headers);
        }

        /// <summary>
        /// proves body is serialized.
        /// </summary>
        /// <returns></returns>
        [TestMethod]
        public async Task WriteMessage_UsesBodyFromSerializer()
        {
            await writer.WriteMessage(
                TestData.DefaultQueueName,
                userMessage.GetType(),
                userMessage);

            Assert.IsNotNull(AddedMessage);
            Assert.AreEqual(serializer.SerializeMessageResult, AddedMessage.Body);
        }

        /// <summary>
        /// Proves counters are invoked
        /// </summary>
        /// <returns></returns>
        [TestMethod]
        public async Task WriteMessage_CountSentMessages()
        {
            await writer.WriteMessage(
                TestData.DefaultQueueName,
                userMessage.GetType(),
                userMessage);

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
                TestData.DefaultQueueName,
                userMessage.GetType(),
                userMessage);

            dataAccess.Verify(d => d.AddMessage(It.IsAny<QueueMessage>(), TestData.DefaultQueueName), Times.Once);
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
                userMessage.GetType(),
                userMessage);
            Assert.AreEqual(expected, AddedToQueue);
        }

        [TestMethod]
        public async Task Given_MessageIsNotIQueuedMessage_When_WriteMessage_Then_ThrowsUsefulException()
        {
            await Assert.ThrowsExceptionAsync<TypeIsNotIQueueMessageException>(() =>
                writer.WriteMessage(new("FooBazQueue"),
                typeof(object),
                new object()));
        }

        [TestMethod]
        public async Task Given_Priority_When_Publish_Then_PriorityIsSet()
        {
            await writer.WriteMessage(
                TestData.DefaultQueueName,
                userMessage.GetType(),
                userMessage,
                priority: 100);

            Assert.IsNotNull(AddedMessage);
            Assert.AreEqual(100, AddedMessage.Priority);
        }

        [TestMethod]
        public async Task Given_UserHeaders_When_Publish_Then_UserHeadersAreUsed()
        {
            await writer.WriteMessage(
                TestData.DefaultQueueName,
                userMessage.GetType(),
                userMessage,
                userHeaders: TestData.DefaultUserHeaders);

            Assert.AreEqual(1, serializer.SerializedHeaders.Count);
            Assert.AreSame(TestData.DefaultUserHeaders, serializer.SerializedHeaders[0].UserHeaders);
        }
    }
}
