﻿using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using PeachtreeBus.Data;
using PeachtreeBus.Interfaces;
using PeachtreeBus.Subscriptions;
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
        public class MessageWithoutInterface { }
        public class TestSubscribedMessage : ISubscribedMessage { }

        private static readonly SerializedData SerializedMessageData = new("SerializedMessage");
        private static readonly SerializedData SerializedHeadersData = new("SerializedHeaders");

        private SubscribedPublisher publisher = default!;
        private SubscribedLifespan lifespan = default!;
        private Mock<IBusDataAccess> dataAccess = default!;
        private Mock<IPerfCounters> counters = default!;
        private Mock<ISerializer> serializer = default!;
        private Mock<ISystemClock> clock = default!;

        private readonly List<SubscribedMessage> AddedMessages = [];
        private Headers SerializedHeaders = default!;

        private readonly UtcDateTime _now = new DateTime(2022, 2, 23, 10, 49, 32, 33, DateTimeKind.Utc);

        private readonly List<Guid> cat1subscribers =
        [
            Guid.NewGuid()
        ];

        private readonly List<Guid> cat2subscribers =
        [
            Guid.NewGuid(),
            Guid.NewGuid()
        ];

        [TestInitialize]
        public void TestInitialize()
        {
            dataAccess = new Mock<IBusDataAccess>();
            counters = new Mock<IPerfCounters>();
            serializer = new Mock<ISerializer>();

            lifespan = new SubscribedLifespan(TimeSpan.FromSeconds(60));

            clock = new Mock<ISystemClock>();

            clock.SetupGet(c => c.UtcNow).Returns(() => _now);

            dataAccess.Setup(d => d.GetSubscribers("cat1"))
                .Returns(Task.FromResult<IEnumerable<Guid>>(cat1subscribers));

            dataAccess.Setup(d => d.GetSubscribers("cat2"))
                .Returns(Task.FromResult<IEnumerable<Guid>>(cat2subscribers));


            dataAccess.Setup(d => d.AddMessage(It.IsAny<SubscribedMessage>()))
                .Callback<SubscribedMessage>((msg) =>
                {
                    AddedMessages.Add(msg);
                })
                .Returns(Task.FromResult<long>(12345));

            serializer.Setup(s => s.SerializeHeaders(It.IsAny<Headers>()))
                .Callback<Headers>(h => SerializedHeaders = h)
                .Returns(SerializedHeadersData);

            serializer.Setup(s => s.SerializeMessage(It.IsAny<object>(), It.IsAny<Type>()))
                .Returns(SerializedMessageData);

            //Writer = new QueueWriter(dataAccess.Object, counters.Object, serializer.Object, clock.Object);
            publisher = new SubscribedPublisher(
                dataAccess.Object,
                lifespan,
                serializer.Object,
                counters.Object,
                clock.Object);
        }

        /// <summary>
        /// Proves message cannot be null
        /// </summary>
        /// <returns></returns>
        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task Publish_ThrowsWhenMessageIsNull()
        {
            await publisher.Publish(
                "cat2",
                typeof(TestSagaMessage1),
                null!,
                null);
        }

        /// <summary>
        /// Proves Type cannot be null
        /// </summary>
        /// <returns></returns>
        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task Publish_ThrowsWhenTypeIsNull()
        {
            await publisher.Publish(
                "cat2",
                null!,
                new TestSagaMessage1(),
                null);
        }

        /// <summary>
        /// Proves category cannot be null
        /// </summary>
        /// <returns></returns>
        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public async Task Publish_ThrowsWhenCategoryIsNull()
        {
            await publisher.Publish(
                null!,
                typeof(TestSagaMessage1),
                new TestSagaMessage1(),
                null);
        }

        /// <summary>
        /// Proves category cannot be empty.
        /// </summary>
        /// <returns></returns>
        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public async Task Publish_ThrowsWhenCategoryIsEmpty()
        {
            await publisher.Publish(
                "",
                typeof(TestSagaMessage1),
                new TestSagaMessage1(),
                null);
        }

        /// <summary>
        /// Proves category cannot be whitespace
        /// </summary>
        /// <returns></returns>
        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public async Task Publish_ThrowsWhenCategoryIsWhitespace()
        {
            await publisher.Publish(
                " ",
                typeof(TestSagaMessage1),
                new TestSagaMessage1(),
                null);
        }

        /// <summary>
        /// Proves Header is set.
        /// </summary>
        /// <returns></returns>
        [TestMethod]
        public async Task Publish_SetsMessageClassOfHeaders()
        {
            await publisher.Publish(
                "cat1",
                typeof(TestSubscribedMessage),
                new TestSubscribedMessage(),
                null);

            Assert.IsNotNull(SerializedHeaders);
            Assert.AreEqual("PeachtreeBus.Tests.Subscriptions.SubscriptionPublisherFixture+TestSubscribedMessage, PeachtreeBus.Tests", SerializedHeaders.MessageClass);
        }

        /// <summary>
        /// Proves message id is assigned
        /// </summary>
        /// <returns></returns>
        [TestMethod]
        public async Task Publish_AssignsMessageId()
        {
            await publisher.Publish(
                "cat1",
                typeof(TestSubscribedMessage),
                new TestSubscribedMessage(),
                null);

            Assert.AreEqual(1, AddedMessages.Count);
            Assert.IsTrue(AddedMessages.TrueForAll(m => m.MessageId != Guid.Empty));
        }

        /// <summary>
        /// Proves NotBefore is defaulted
        /// </summary>
        /// <returns></returns>
        [TestMethod]
        public async Task Publish_DefaultsNotBeforeToUtcNow()
        {
            await publisher.Publish(
                            "cat1",
                            typeof(TestSubscribedMessage),
                            new TestSubscribedMessage(),
                            null);

            Assert.AreEqual(1, AddedMessages.Count);
            Assert.IsTrue(AddedMessages.TrueForAll(m => m.NotBefore == _now));
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
                "cat1",
                typeof(TestSubscribedMessage),
                new TestSubscribedMessage(),
                notBefore);

            Assert.AreEqual(1, AddedMessages.Count);
            Assert.IsTrue(AddedMessages.TrueForAll(m => m.NotBefore == notBefore));
        }

        /// <summary>
        /// Proves NotBefore must have DateTimeKind
        /// </summary>
        /// <returns></returns>
        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public async Task Publish_ThrowsWhenNotBeforeKindUnspecified()
        {
            var notBefore = new DateTime(2022, 2, 23, 10, 54, 11, DateTimeKind.Unspecified);
            await publisher.Publish(
                "cat1",
                typeof(TestSagaMessage1),
                new TestSagaMessage1(),
                notBefore);
        }

        /// <summary>
        /// Proves Enqueued is set.
        /// </summary>
        /// <returns></returns>
        [TestMethod]
        public async Task Publish_SetsEnqueuedToUtcNow()
        {
            await publisher.Publish(
                "cat1",
                typeof(TestSubscribedMessage),
                new TestSubscribedMessage(),
                null);

            Assert.AreEqual(1, AddedMessages.Count);
            Assert.IsTrue(AddedMessages.TrueForAll(m => m.Enqueued == _now));
        }

        /// <summary>
        /// Proves that completed defaults to null
        /// </summary>
        /// <returns></returns>
        [TestMethod]
        public async Task Publish_SetsCompletedToNull()
        {
            await publisher.Publish(
                "cat1",
                typeof(TestSubscribedMessage),
                new TestSubscribedMessage(),
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
                "cat1",
                typeof(TestSubscribedMessage),
                new TestSubscribedMessage(),
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
                "cat1",
                typeof(TestSubscribedMessage),
                new TestSubscribedMessage(),
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
                "cat1",
                typeof(TestSubscribedMessage),
                new TestSubscribedMessage(),
                null);
            Assert.AreEqual(1, AddedMessages.Count);
            Assert.IsTrue(AddedMessages.TrueForAll(m => m.Headers == SerializedHeadersData));
        }

        /// <summary>
        /// Proves Body is serialized
        /// </summary>
        /// <returns></returns>
        [TestMethod]
        public async Task Publish_UsesBodyFromSerializer()
        {
            await publisher.Publish(
                "cat1",
                typeof(TestSubscribedMessage),
                new TestSubscribedMessage(),
                null);

            Assert.AreEqual(1, AddedMessages.Count);
            Assert.IsTrue(AddedMessages.TrueForAll(m => m.Body == SerializedMessageData));
        }

        /// <summary>
        /// Proves Perf counters are used.
        /// </summary>
        /// <returns></returns>
        [TestMethod]
        public async Task Publish_CountSentMessages()
        {
            await publisher.Publish(
                "cat1",
                typeof(TestSubscribedMessage),
                new TestSubscribedMessage(),
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
                "cat1",
                typeof(TestSubscribedMessage),
                new TestSubscribedMessage(),
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
                "cat2",
                typeof(TestSubscribedMessage),
                new TestSubscribedMessage(),
                null);

            Assert.AreEqual(2, AddedMessages.Count);
            counters.Verify(c => c.SentMessage(), Times.Exactly(2));
            dataAccess.Verify(d => d.AddMessage(It.IsAny<SubscribedMessage>()), Times.Exactly(2));

            CollectionAssert.AreEquivalent(
                cat2subscribers,
                AddedMessages.Select(m => m.SubscriberId).ToList());
        }

        /// <summary>
        /// Proves that subscriptions are expired so messages are not sent
        /// to invalid subscribers.
        /// </summary>
        /// <returns></returns>
        [TestMethod]
        public async Task Publish_ExpiresSubscriptions()
        {
            await publisher.Publish(
                "cat2",
                typeof(TestSubscribedMessage),
                new TestSubscribedMessage(),
                null);
            dataAccess.Verify(d => d.ExpireSubscriptions(), Times.Once);
        }

        [TestMethod]
        public async Task Given_MessageIsNotISubscribedMessage_When_WriteMessage_Then_ThrowsUsefulException()
        {
            await Assert.ThrowsExceptionAsync<TypeIsNotISubscribedMessageException>(() =>
                publisher.Publish("cat2", typeof(MessageWithoutInterface), new MessageWithoutInterface(), null));
        }
    }
}