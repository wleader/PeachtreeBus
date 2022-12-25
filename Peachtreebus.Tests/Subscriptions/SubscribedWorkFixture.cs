using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Peachtreebus.Tests.Pipeline;
using PeachtreeBus;
using PeachtreeBus.Data;
using PeachtreeBus.Subscriptions;
using System;
using System.Collections.Generic;
using System.Linq;
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
        private Mock<ILogger<SubscribedWork>> log;
        private Mock<IPerfCounters> counters;
        private Mock<IFindSubscribedHandlers> findHandlers;
        private Mock<ISubscribedReader> reader;
        private Mock<IBusDataAccess> dataAccess;
        private Mock<IFindSubscribedPipelineSteps> findPipelineSteps;

        [TestInitialize]
        public void TestInitialize()
        {
            log = new Mock<ILogger<SubscribedWork>>();
            counters = new Mock<IPerfCounters>();
            findHandlers = new Mock<IFindSubscribedHandlers>();
            reader = new Mock<ISubscribedReader>();
            dataAccess = new Mock<IBusDataAccess>();
            findPipelineSteps = new();

            reader.Setup(r => r.GetNext(It.IsAny<Guid>()))
                .Returns(Task.FromResult(CreateContext()));

            findHandlers.Setup(f => f.FindHandlers<TestMessage>())
                .Returns(GetOneGoodHandler());

            findPipelineSteps.Setup(f => f.FindSteps())
                .Returns(Array.Empty<ISubscribedPipelineStep>().ToList());

            work = new SubscribedWork(
                reader.Object,
                counters.Object,
                log.Object,
                dataAccess.Object,
                findHandlers.Object,
                findPipelineSteps.Object);
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

        [TestMethod]
        public async Task Given_PipelineStepsAreProvided_When_DoWork_Then_StepsAreInvokedInOrder()
        {
            var invocations = new List<string>();

            var steps = new List<ISubscribedPipelineStep>()
            {
                new FakeSubscribedPipelineStep(3, async (c,n) =>
                {
                    invocations.Add("step3");
                    await n(c);
                    invocations.Add("post3");
                }),
                new FakeSubscribedPipelineStep(1, async (c,n) =>
                {
                    invocations.Add("step1");
                    await n(c);
                    invocations.Add("post1");
                }),
                new FakeSubscribedPipelineStep(2, async (c,n) =>
                {
                    invocations.Add("step2");
                    await n(c);
                    invocations.Add("post2");
                }),
            };

            findPipelineSteps.Setup(f => f.FindSteps()).Returns(steps);

            var handler = new Mock<IHandleSubscribedMessage<TestMessage>>();
            findHandlers.Setup(f => f.FindHandlers<TestMessage>())
                .Returns(new List<IHandleSubscribedMessage<TestMessage>>() { handler.Object });

            handler.Setup(h => h.Handle(It.IsAny<SubscribedContext>(), It.IsAny<TestMessage>()))
                .Callback((SubscribedContext c, TestMessage m) =>
                {
                    invocations.Add("handler");
                });

            work.SubscriberId = Guid.NewGuid();
            var result = await work.DoWork();

            // verify that all the steps were invoked in order.
            var expected = new List<string>() { "step1", "step2", "step3", "handler", "post3", "post2", "post1" };
            CollectionAssert.AreEqual(expected, invocations);
        }

        [TestMethod]
        public async Task Given_NoPipelineStepsAreProvided_When_DoWork_Then_InvokesHandlers()
        {
            var invocations = new List<string>();

            findPipelineSteps.Setup(f => f.FindSteps())
                .Returns(Array.Empty<ISubscribedPipelineStep>().ToList());

            var handler = new Mock<IHandleSubscribedMessage<TestMessage>>();
            findHandlers.Setup(f => f.FindHandlers<TestMessage>())
                .Returns(new List<IHandleSubscribedMessage<TestMessage>>() { handler.Object });

            handler.Setup(h => h.Handle(It.IsAny<SubscribedContext>(), It.IsAny<TestMessage>()))
                .Callback((SubscribedContext c, TestMessage m) =>
                {
                    invocations.Add("handler");
                });

            work.SubscriberId = Guid.NewGuid();
            var result = await work.DoWork();

            // verify that all the steps were invoked in order.
            var expected = new List<string>() { "handler" };
            CollectionAssert.AreEqual(expected, invocations);
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
