using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using PeachtreeBus;
using PeachtreeBus.Data;
using PeachtreeBus.Subscriptions;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Peachtreebus.Tests.Subscriptions
{
    /// <summary>
    /// Proves the behavior of Subscribed Work
    /// </summary>
    [TestClass]
    public class SubscribedWorkFixture
    {
        public class TestMessage : ISubscribedMessage
        {

        }

        private SubscribedWork work;
        private Mock<ILog<SubscribedWork>> log;
        private Mock<IPerfCounters> counters;
        private Mock<IFindSubscribedHandlers> findHandlers;
        private Mock<ISubscribedReader> reader;
        private Mock<IBusDataAccess> dataAccess;

        [TestInitialize]
        public void TestInitialize()
        {
            log = new Mock<ILog<SubscribedWork>>();
            counters = new Mock<IPerfCounters>();
            findHandlers = new Mock<IFindSubscribedHandlers>();
            reader = new Mock<ISubscribedReader>();
            dataAccess = new Mock<IBusDataAccess>();

            reader.Setup(r => r.GetNext(It.IsAny<Guid>()))
                .Returns(Task.FromResult(CreateContext()));

            findHandlers.Setup(f => f.FindHandlers<TestMessage>())
                .Returns(GetOneGoodHandler());

            work = new SubscribedWork(
                reader.Object,
                counters.Object,
                log.Object,
                dataAccess.Object,
                findHandlers.Object);
        }

        /// <summary>
        /// Proves when no message
        /// </summary>
        /// <returns></returns>
        [TestMethod]
        public async Task DoWork_WhenNoMessage_ThenReturnFalse()
        {
            reader.Setup(r => r.GetNext(It.IsAny<Guid>()))
                .Returns(Task.FromResult<SubscribedContext>(null));

            work.SubscriberId = Guid.NewGuid();
            var result = await work.DoWork();

            Assert.IsFalse(result);
        }

        /// <summary>
        /// Proves performance counters are invoked
        /// </summary>
        /// <returns></returns>
        [TestMethod]
        public async Task DoWork_WhenMessage_IncrementCounters()
        {
            work.SubscriberId = Guid.NewGuid();
            var result = await work.DoWork();

            counters.Verify(c => c.StartMessage(), Times.Once);
            counters.Verify(c => c.FinishMessage(It.IsAny<DateTime>()), Times.Once);
        }

        /// <summary>
        /// Proves that a savepoint is always created
        /// </summary>
        /// <returns></returns>
        [TestMethod]
        public async Task DoWork_WhenMessage_CreatesSavepoint()
        {
            work.SubscriberId = Guid.NewGuid();
            var result = await work.DoWork();

            dataAccess.Verify(d => d.CreateSavepoint("BeforeSubscriptionHandler"), Times.Once);
        }

        /// <summary>
        /// Proves when message type unrecognized.
        /// </summary>
        /// <returns></returns>
        [TestMethod]
        public async Task DoWork_WhenMessageTypeUnrecognized()
        {
            reader.Setup(r => r.GetNext(It.IsAny<Guid>()))
                .Returns(CreateContextWithUnrecognizedMessageType);

            work.SubscriberId = Guid.NewGuid();
            var result = await work.DoWork();

            dataAccess.Verify(d => d.RollbackToSavepoint("BeforeSubscriptionHandler"), Times.Once);
            reader.Verify(r => r.Fail(It.IsAny<SubscribedContext>(), It.IsAny<Exception>()), Times.Once);
            counters.Verify(c => c.FinishMessage(It.IsAny<DateTime>()), Times.Once);
            Assert.IsTrue(result);
        }

        /// <summary>
        /// Proves when a message has no handlers.
        /// </summary>
        /// <returns></returns>
        [TestMethod]
        public async Task DoWork_WhenMessageAndNoHandlers()
        {
            findHandlers.Setup(f => f.FindHandlers<TestMessage>())
                .Returns(GetNoHandlers());

            work.SubscriberId = Guid.NewGuid();
            var result = await work.DoWork();

            dataAccess.Verify(d => d.RollbackToSavepoint("BeforeSubscriptionHandler"), Times.Once);
            reader.Verify(r => r.Fail(It.IsAny<SubscribedContext>(), It.IsAny<Exception>()), Times.Once);
            counters.Verify(c => c.FinishMessage(It.IsAny<DateTime>()), Times.Once);
            Assert.IsTrue(result);
        }

        /// <summary>
        /// Proves when there is one handler
        /// </summary>
        /// <returns></returns>
        [TestMethod]
        public async Task DoWork_WhenMesageAndOneHandlers()
        {
            work.SubscriberId = Guid.NewGuid();
            var result = await work.DoWork();

            dataAccess.Verify(d => d.RollbackToSavepoint("BeforeSubscriptionHandler"), Times.Never);
            reader.Verify(r => r.Complete(It.IsAny<SubscribedContext>()), Times.Once);
            counters.Verify(c => c.FinishMessage(It.IsAny<DateTime>()), Times.Once);
            Assert.IsTrue(result);
        }

        /// <summary>
        /// proves when there are multiple handlers.
        /// </summary>
        /// <returns></returns>
        [TestMethod]
        public async Task DoWork_WhenMesageAndMultipleHandlers()
        {
            findHandlers.Setup(f => f.FindHandlers<TestMessage>())
                .Returns(GetTwoHandlers());

            work.SubscriberId = Guid.NewGuid();
            var result = await work.DoWork();

            dataAccess.Verify(d => d.RollbackToSavepoint("BeforeSubscriptionHandler"), Times.Never);
            reader.Verify(r => r.Complete(It.IsAny<SubscribedContext>()), Times.Once);
            counters.Verify(c => c.FinishMessage(It.IsAny<DateTime>()), Times.Once);
            Assert.IsTrue(result);
        }

        private SubscribedContext CreateContext()
        {
            return new SubscribedContext()
            {
                MessageData = new PeachtreeBus.Model.SubscribedMessage
                {
                    MessageId = Guid.NewGuid(),
                },
                Headers = new Headers()
                { 
                    MessageClass = "Peachtreebus.Tests.Subscriptions.SubscribedWorkFixture+TestMessage, Peachtreebus.Tests",
                }
            };
        }

        private Task<SubscribedContext> CreateContextWithUnrecognizedMessageType()
        {
            return Task.FromResult(new SubscribedContext()
            {
                MessageData = new PeachtreeBus.Model.SubscribedMessage
                {
                    MessageId = Guid.NewGuid(),
                },
                Headers = new Headers()
                {
                    MessageClass = "Peachtreebus.Tests.Subscriptions.SubscriptionProcessOneFixture+NotARealClassTestMessage, Peachtreebus.Tests",
                }
            });
        }

        private IEnumerable<IHandleSubscribedMessage<TestMessage>> GetOneGoodHandler()
        {
            var list = new List<IHandleSubscribedMessage<TestMessage>>
            {
                new Mock<IHandleSubscribedMessage<TestMessage>>().Object
            };
            return list;
        }

        private IEnumerable<IHandleSubscribedMessage<TestMessage>> GetNoHandlers()
        {
            var list = new List<IHandleSubscribedMessage<TestMessage>>
            {
            };
            return list;
        }

        private IEnumerable<IHandleSubscribedMessage<TestMessage>> GetTwoHandlers()
        {
            var list = new List<IHandleSubscribedMessage<TestMessage>>
            {
                new Mock<IHandleSubscribedMessage<TestMessage>>().Object,
                new Mock<IHandleSubscribedMessage<TestMessage>>().Object
            };
            return list;
        }
    }
}
