using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using PeachtreeBus.Data;
using PeachtreeBus.Interfaces;
using PeachtreeBus.Subscriptions;
using PeachtreeBus.Tests.Fakes;
using PeachtreeBus.Tests.Sagas;
using System;
using System.Collections.Generic;
using System.Linq;
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

        // a message to send.
        private TestData.TestSubscribedMessage userMessage = default!;

        // stores the parameters to the AddMessage calls.
        private readonly List<SubscribedMessage> AddedMessages = [];

        private readonly List<SubscriberId> cat1subscribers =
        [
            SubscriberId.New()
        ];

        private readonly List<SubscriberId> cat2subscribers =
        [
            SubscriberId.New(),
            SubscriberId.New()
        ];

        [TestInitialize]
        public void TestInitialize()
        {
            dataAccess = new();
            counters = new();
            serializer = new();
            clock = new();

            clock.SetupGet(c => c.UtcNow).Returns(() => TestData.Now);

            dataAccess.Setup(d => d.GetSubscribers(TestData.DefaultCategory))
                .Returns(Task.FromResult<IEnumerable<SubscriberId>>(cat1subscribers));

            dataAccess.Setup(d => d.GetSubscribers(TestData.DefaultCategory2))
                .Returns(Task.FromResult<IEnumerable<SubscriberId>>(cat2subscribers));

            dataAccess.Setup(d => d.AddMessage(It.IsAny<SubscribedMessage>()))
                .Callback<SubscribedMessage>((msg) =>
                {
                    AddedMessages.Add(msg);
                })
                .Returns(Task.FromResult<Identity>(new(12345)));

            userMessage = TestData.CreateSubscribedUserMessage();

            publisher = new SubscribedPublisher(
                dataAccess.Object,
                TestData.DefaultSubscribedLifespan,
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
                    TestData.DefaultCategory2,
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
                    TestData.DefaultCategory2,
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
                TestData.DefaultCategory,
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
                TestData.DefaultCategory,
                userMessage.GetType(),
                userMessage,
                null);

            Assert.AreEqual(1, AddedMessages.Count);
            Assert.IsTrue(AddedMessages.TrueForAll(m => m.NotBefore == TestData.Now));
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
                TestData.DefaultCategory,
                userMessage.GetType(),
                userMessage,
                notBefore);

            Assert.AreEqual(1, AddedMessages.Count);
            Assert.IsTrue(AddedMessages.TrueForAll(m => m.NotBefore == notBefore));
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
                    TestData.DefaultCategory,
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
                TestData.DefaultCategory,
                userMessage.GetType(),
               userMessage,
                null);

            Assert.AreEqual(1, AddedMessages.Count);
            Assert.IsTrue(AddedMessages.TrueForAll(m => m.Enqueued == TestData.Now));
        }

        /// <summary>
        /// Proves that completed defaults to null
        /// </summary>
        /// <returns></returns>
        [TestMethod]
        public async Task Publish_SetsCompletedToNull()
        {
            await publisher.Publish(
                TestData.DefaultCategory,
                userMessage.GetType(),
                userMessage,
                null);

            Assert.AreEqual(1, AddedMessages.Count);
            Assert.IsTrue(AddedMessages.TrueForAll(m => !m.Completed.HasValue));
        }

        /// <summary>
        /// Proves that failed defaults to null.
        /// </summary>
        /// <returns></returns>
        [TestMethod]
        public async Task Publish_SetsFailedToNull()
        {
            await publisher.Publish(
                TestData.DefaultCategory,
                userMessage.GetType(),
               userMessage,
                null);

            Assert.AreEqual(1, AddedMessages.Count);
            Assert.IsTrue(AddedMessages.TrueForAll(m => !m.Failed.HasValue));
        }

        /// <summary>
        /// proves retries defaults to zero.
        /// </summary>
        /// <returns></returns>
        [TestMethod]
        public async Task Publish_SetsRetriesToZero()
        {
            await publisher.Publish(
                TestData.DefaultCategory,
                userMessage.GetType(),
                userMessage,
                null);

            Assert.AreEqual(1, AddedMessages.Count);
            Assert.IsTrue(AddedMessages.TrueForAll(m => m.Retries == 0));
        }

        /// <summary>
        /// Proves headers are serialized.
        /// </summary>
        /// <returns></returns>
        [TestMethod]
        public async Task Publish_UsesHeadersFromSerializer()
        {
            await publisher.Publish(
                TestData.DefaultCategory,
                userMessage.GetType(),
                userMessage,
                null);
            Assert.AreEqual(1, AddedMessages.Count);
            Assert.AreEqual(serializer.SerializeHeadersResult, AddedMessages[0].Headers);
        }

        /// <summary>
        /// Proves Body is serialized
        /// </summary>
        /// <returns></returns>
        [TestMethod]
        public async Task Publish_UsesBodyFromSerializer()
        {
            await publisher.Publish(
                TestData.DefaultCategory,
                userMessage.GetType(),
                userMessage,
                null);

            Assert.AreEqual(1, AddedMessages.Count);
            Assert.AreEqual(1, serializer.SerializedMessages.Count);
            Assert.AreEqual(serializer.SerializeMessageResult, AddedMessages[0].Body);
            Assert.AreEqual(userMessage.GetType(), serializer.SerializedMessages[0].Type);
            Assert.AreEqual(userMessage, serializer.SerializedMessages[0].Object);
        }

        /// <summary>
        /// Proves Perf counters are used.
        /// </summary>
        /// <returns></returns>
        [TestMethod]
        public async Task Publish_CountSentMessages()
        {
            await publisher.Publish(
                TestData.DefaultCategory,
                userMessage.GetType(),
                userMessage,
                null);

            counters.Verify(c => c.SentMessage(), Times.Once);
        }

        /// <summary>
        /// Proves DataAccess is used
        /// </summary>
        /// <returns></returns>
        [TestMethod]
        public async Task Publish_InvokesDataAccess()
        {
            await publisher.Publish(
                TestData.DefaultCategory,
                userMessage.GetType(),
                userMessage,
                null);

            dataAccess.Verify(d => d.AddMessage(It.IsAny<SubscribedMessage>()), Times.Once);
        }

        /// <summary>
        /// Proves messages are publised to multiple subscribers
        /// </summary>
        /// <returns></returns>
        [TestMethod]
        public async Task Publish_PublishesMultiples()
        {
            await publisher.Publish(
                TestData.DefaultCategory2,
                userMessage.GetType(),
                userMessage,
                null);

            Assert.AreEqual(2, AddedMessages.Count);
            counters.Verify(c => c.SentMessage(), Times.Exactly(2));
            dataAccess.Verify(d => d.AddMessage(It.IsAny<SubscribedMessage>()), Times.Exactly(2));

            CollectionAssert.AreEquivalent(
                cat2subscribers,
                AddedMessages.Select(m => m.SubscriberId).ToList());

            Assert.AreEqual(1, serializer.SerializedHeaders.Count, "Headers only need to be serialized once.");
            Assert.AreEqual(1, serializer.SerializedMessages.Count, "Body only needs to be serialzied once.");
        }

        [TestMethod]
        public async Task Given_MessageIsNotISubscribedMessage_When_WriteMessage_Then_ThrowsUsefulException()
        {
            await Assert.ThrowsExceptionAsync<TypeIsNotISubscribedMessageException>(() =>
                publisher.Publish(TestData.DefaultCategory2, typeof(object), new object(), null));
        }


        [TestMethod]
        public async Task Given_Priority_When_Publish_Then_PriorityIsSet()
        {
            await publisher.Publish(
                TestData.DefaultCategory,
                userMessage.GetType(),
                userMessage,
                priority: 100);

            Assert.AreEqual(1, AddedMessages.Count);
            Assert.AreEqual(100, AddedMessages[0].Priority);
        }

        [TestMethod]
        public async Task Given_UserHeaders_When_Publish_Then_UserHeadersAreUsed()
        {
            await publisher.Publish(
                TestData.DefaultCategory,
                userMessage.GetType(),
                userMessage,
                userHeaders: TestData.DefaultUserHeaders);

            Assert.AreEqual(1, serializer.SerializedHeaders.Count);
            Assert.AreSame(TestData.DefaultUserHeaders, serializer.SerializedHeaders[0].UserHeaders);
        }
    }
}