using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using PeachtreeBus.Data;
using PeachtreeBus.Interfaces;
using PeachtreeBus.Subscriptions;
using PeachtreeBus.Tests.Fakes;
using PeachtreeBus.Tests.Sagas;
using System;
using System.Threading.Tasks;

namespace PeachtreeBus.Tests.Subscriptions
{
    /// <summary>
    /// Proves the behavior of SubscriptionPublisher
    /// </summary>
    [TestClass]
    public class SubscriptionPublisherFixture
    {
        // the class under test.
        private SubscribedPublisher publisher = default!;

        // Dependencies
        private Mock<IBusDataAccess> dataAccess = default!;
        private Mock<IPerfCounters> counters = default!;
        private FakeSerializer serializer = default!;
        private Mock<ISystemClock> clock = default!;
        private BusConfiguration configuration = default!;

        // a message to send.
        private TestData.TestSubscribedMessage userMessage = default!;

        // stores the parameters to the AddMessage calls.
        private SubscribedMessage? PublishedMessage;
        private Topic? PublishedTopic;
        private long PublishResult = 1;

        [TestInitialize]
        public void TestInitialize()
        {
            dataAccess = new();
            counters = new();
            serializer = new();
            clock = new();
            configuration = TestData.CreateBusConfiguration();

            PublishedMessage = null;
            PublishedTopic = null;

            clock.SetupGet(c => c.UtcNow).Returns(() => TestData.Now);

            dataAccess.Setup(d => d.Publish(It.IsAny<SubscribedMessage>(), It.IsAny<Topic>()))
                .Callback((SubscribedMessage m, Topic c) =>
                {
                    PublishedMessage = m;
                    PublishedTopic = c;
                })
                .ReturnsAsync(() => PublishResult);

            userMessage = TestData.CreateSubscribedUserMessage();

            publisher = new SubscribedPublisher(
                dataAccess.Object,
                configuration,
                serializer.Object,
                counters.Object,
                clock.Object);
        }

        /// <summary>
        /// Proves message cannot be null
        /// </summary>
        /// <returns></returns>
        [TestMethod]
        public async Task Publish_ThrowsWhenMessageIsNull()
        {
            await Assert.ThrowsExceptionAsync<ArgumentNullException>(() =>
                publisher.Publish(
                    TestData.DefaultTopic2,
                    userMessage.GetType(),
                    null!,
                    null));
        }

        /// <summary>
        /// Proves Type cannot be null
        /// </summary>
        /// <returns></returns>
        [TestMethod]
        public async Task Publish_ThrowsWhenTypeIsNull()
        {
            await Assert.ThrowsExceptionAsync<ArgumentNullException>(() =>
                publisher.Publish(
                    TestData.DefaultTopic2,
                    null!,
                    new TestSagaMessage1(),
                    null));
        }

        /// <summary>
        /// Proves Header is set.
        /// </summary>
        /// <returns></returns>
        [TestMethod]
        public async Task Publish_SetsMessageClassOfHeaders()
        {
            await publisher.Publish(
                TestData.DefaultTopic,
                userMessage.GetType(),
                userMessage,
                null);

            Assert.AreEqual(1, serializer.SerializedHeaders.Count);
            Assert.AreEqual("PeachtreeBus.Tests.TestData+TestSubscribedMessage, PeachtreeBus.Tests",
                serializer.SerializedHeaders[0].MessageClass);
        }

        /// <summary>
        /// Proves NotBefore is defaulted
        /// </summary>
        /// <returns></returns>
        [TestMethod]
        public async Task Publish_DefaultsNotBeforeToUtcNow()
        {
            await publisher.Publish(
                TestData.DefaultTopic,
                userMessage.GetType(),
                userMessage,
                null);

            Assert.AreEqual(clock.Object.UtcNow, PublishedMessage?.NotBefore);
        }

        /// <summary>
        /// Proves NotBefore is used
        /// </summary>
        /// <returns></returns>
        [TestMethod]
        public async Task Publish_UsesProvidedNotBefore()
        {
            UtcDateTime notBefore = DateTime.UtcNow;
            await publisher.Publish(
                TestData.DefaultTopic,
                userMessage.GetType(),
                userMessage,
                notBefore);

            Assert.AreEqual(notBefore, PublishedMessage?.NotBefore);
        }

        /// <summary>
        /// Proves NotBefore must have DateTimeKind
        /// </summary>
        /// <returns></returns>
        [TestMethod]
        public async Task Publish_ThrowsWhenNotBeforeKindUnspecified()
        {
            var notBefore = new DateTime(2022, 2, 23, 10, 54, 11, DateTimeKind.Unspecified);
            await Assert.ThrowsExceptionAsync<ArgumentException>(() =>
                publisher.Publish(
                    TestData.DefaultTopic,
                    typeof(TestSagaMessage1),
                    new TestSagaMessage1(),
                    notBefore));
        }

        /// <summary>
        /// Proves Enqueued is set.
        /// </summary>
        /// <returns></returns>
        [TestMethod]
        public async Task Publish_SetsEnqueuedToUtcNow()
        {
            await publisher.Publish(
                TestData.DefaultTopic,
                userMessage.GetType(),
               userMessage,
                null);
            Assert.AreEqual(clock.Object.UtcNow, PublishedMessage?.Enqueued);
        }

        /// <summary>
        /// Proves that completed defaults to null
        /// </summary>
        /// <returns></returns>
        [TestMethod]
        public async Task Publish_SetsCompletedToNull()
        {
            await publisher.Publish(
                TestData.DefaultTopic,
                userMessage.GetType(),
                userMessage,
                null);

            Assert.IsNotNull(PublishedMessage);
            Assert.IsNull(PublishedMessage.Completed);
        }

        /// <summary>
        /// Proves that failed defaults to null.
        /// </summary>
        /// <returns></returns>
        [TestMethod]
        public async Task Publish_SetsFailedToNull()
        {
            await publisher.Publish(
                TestData.DefaultTopic,
                userMessage.GetType(),
               userMessage,
                null);

            Assert.IsNotNull(PublishedMessage);
            Assert.IsNull(PublishedMessage.Failed);
        }

        /// <summary>
        /// proves retries defaults to zero.
        /// </summary>
        /// <returns></returns>
        [TestMethod]
        public async Task Publish_SetsRetriesToZero()
        {
            await publisher.Publish(
                TestData.DefaultTopic,
                userMessage.GetType(),
                userMessage,
                null);

            Assert.IsNotNull(PublishedMessage);
            Assert.AreEqual(0, PublishedMessage.Retries);
        }

        /// <summary>
        /// Proves headers are serialized.
        /// </summary>
        /// <returns></returns>
        [TestMethod]
        public async Task Publish_UsesHeadersFromSerializer()
        {
            await publisher.Publish(
                TestData.DefaultTopic,
                userMessage.GetType(),
                userMessage,
                null);

            Assert.AreEqual(serializer.SerializeHeadersResult, PublishedMessage?.Headers);
        }

        /// <summary>
        /// Proves Body is serialized
        /// </summary>
        /// <returns></returns>
        [TestMethod]
        public async Task Publish_UsesBodyFromSerializer()
        {
            await publisher.Publish(
                TestData.DefaultTopic,
                userMessage.GetType(),
                userMessage,
                null);

            Assert.AreEqual(serializer.SerializeMessageResult, PublishedMessage?.Body);
        }

        /// <summary>
        /// Proves Perf counters are used.
        /// </summary>
        /// <returns></returns>
        [TestMethod]
        [DataRow(0)]
        [DataRow(1)]
        [DataRow(10)]
        [DataRow(long.MaxValue)]
        public async Task Publish_CountPublishedMessages(long count)
        {
            PublishResult = count;

            await publisher.Publish(
                TestData.DefaultTopic,
                userMessage.GetType(),
                userMessage,
                null);

            counters.Verify(c => c.PublishMessage(count), Times.Once);
        }

        /// <summary>
        /// Proves DataAccess is used
        /// </summary>
        /// <returns></returns>
        [TestMethod]
        public async Task Given_Topic_When_Publish_Then_TopicIsUsed()
        {
            await publisher.Publish(
                TestData.DefaultTopic,
                userMessage.GetType(),
                userMessage,
                null);

            Assert.IsTrue(PublishedTopic.HasValue);
            Assert.AreEqual(TestData.DefaultTopic, PublishedTopic.Value);
        }

        [TestMethod]
        public async Task Given_MessageIsNotISubscribedMessage_When_WriteMessage_Then_ThrowsUsefulException()
        {
            await Assert.ThrowsExceptionAsync<TypeIsNotISubscribedMessageException>(() =>
                publisher.Publish(TestData.DefaultTopic2, typeof(object), new object(), null));
        }

        [TestMethod]
        public async Task Given_Priority_When_Publish_Then_PriorityIsSet()
        {
            await publisher.Publish(
                TestData.DefaultTopic,
                userMessage.GetType(),
                userMessage,
                priority: 100);

            Assert.AreEqual(100, PublishedMessage?.Priority);
        }

        [TestMethod]
        public async Task Given_UserHeaders_When_Publish_Then_UserHeadersAreUsed()
        {
            await publisher.Publish(
                TestData.DefaultTopic,
                userMessage.GetType(),
                userMessage,
                userHeaders: TestData.DefaultUserHeaders);

            Assert.AreEqual(1, serializer.SerializedHeaders.Count);
            Assert.AreSame(TestData.DefaultUserHeaders, serializer.SerializedHeaders[0].UserHeaders);
        }

        [TestMethod]
        public async Task When_Publish_Then_ValidUntilUsesConfiguration()
        {
            var expectedValidUntil = clock.Object.UtcNow.Add(configuration.PublishConfiguration.Lifespan);

            await publisher.Publish(
                TestData.DefaultTopic,
                userMessage.GetType(),
                userMessage);

            Assert.AreEqual(expectedValidUntil, PublishedMessage?.ValidUntil);
        }
    }
}