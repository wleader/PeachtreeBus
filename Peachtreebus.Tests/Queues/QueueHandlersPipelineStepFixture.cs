using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Peachtreebus.Tests.Sagas;
using PeachtreeBus;
using PeachtreeBus.Data;
using PeachtreeBus.Queues;
using PeachtreeBus.Sagas;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Peachtreebus.Tests.Queues
{
    [TestClass]
    public class QueueHandlersPipelineStepFixture
    {
        public class MessageWithoutInterface { };

        private QueueHandlersPipelineStep _testSubject;
        private Mock<IFindQueueHandlers> _findHandlers;
        private Mock<ILogger<QueueHandlersPipelineStep>> _log;
        private Mock<ISagaMessageMapManager> _sagaMessageMapManager;
        private Mock<IQueueReader> _queueReader;

        private TestSaga _testSaga;

        [TestInitialize] 
        public void Initialize()
        {
            _testSaga = new();

            _findHandlers = new();
            _log = new();
            _sagaMessageMapManager = new();
            _queueReader = new();

            _testSubject = new(
                _findHandlers.Object,
                _log.Object,
                _sagaMessageMapManager.Object,
                _queueReader.Object);
        }

        [TestMethod]
        [ExpectedException(typeof(MissingInterfaceException))]
        public async Task Given_MessageIsNotIQueuedMessage_Then_ThrowsUsefulException()
        {
            var context = GetContext<TestSagaMessage1>();
            context.Headers.MessageClass = typeof(MessageWithoutInterface).AssemblyQualifiedName;
            await _testSubject.Invoke(context, null);
        }

        /// <summary>
        /// Proves behavior when the handler is a saga
        /// </summary>
        /// <returns></returns>
        [TestMethod]
        public async Task Given_MessageIsHandledBySaga_When_Invoke_Then_SagaHandlesMessage()
        {
            _findHandlers.Setup(f => f.FindHandlers<TestSagaMessage1>())
                .Returns(GetSagaHandler());

            var context = GetContext<TestSagaMessage1>();

            _queueReader.Setup(r => r.LoadSaga(It.IsAny<object>(), It.IsAny<InternalQueueContext>()))
                .Callback<object, InternalQueueContext>((s, c) =>
                {
                    c.SagaData = new PeachtreeBus.Model.SagaData
                    {
                        Blocked = false
                    };
                });

            await _testSubject.Invoke(context, null);

            _queueReader.Verify(r => r.LoadSaga(It.IsAny<object>(), It.IsAny<InternalQueueContext>()), Times.Once);
            _testSaga.AssertInvocations(1);
            _testSaga.AssertInvoked(context, (TestSagaMessage1)context.Message);
            _queueReader.Verify(r => r.SaveSaga(It.IsAny<object>(), It.IsAny<InternalQueueContext>()), Times.Once);

            Assert.IsFalse(context.SagaBlocked);
        }

        [TestMethod]
        public async Task Given_MessageHasMultipleHandlersAndMultipleSagas_When_Invoke_Then_AllHandlersAreInvoked()
        {
            var saga1 = new TestSaga();
            var saga2 = new TestSaga();
            var handler1 = new Mock<IHandleQueueMessage<TestSagaMessage1>>();
            var handler2 = new Mock<IHandleQueueMessage<TestSagaMessage1>>();

            List<IHandleQueueMessage<TestSagaMessage1>> handlers = [saga1, saga2, handler1.Object, handler2.Object];

            _findHandlers.Setup(f => f.FindHandlers<TestSagaMessage1>()).Returns(handlers);

            _queueReader.Setup(r => r.LoadSaga(It.IsAny<object>(), It.IsAny<InternalQueueContext>()))
                .Callback<object, InternalQueueContext>((s, c) =>
                {
                    c.SagaData = new PeachtreeBus.Model.SagaData
                    {
                        Blocked = false
                    };
                });

            var context = GetContext<TestSagaMessage1>();

            await _testSubject.Invoke(context, null);

            saga1.AssertInvocations(1);
            saga1.AssertInvoked(context, (TestSagaMessage1)context.Message);
            saga2.AssertInvocations(1);
            saga2.AssertInvoked(context, (TestSagaMessage1)context.Message);
            handler1.Verify(h => h.Handle(context, (TestSagaMessage1)context.Message), Times.Once);
            handler2.Verify(h => h.Handle(context, (TestSagaMessage1)context.Message), Times.Once);

            _queueReader.Verify(r => r.LoadSaga(It.IsAny<object>(), It.IsAny<InternalQueueContext>()), Times.Exactly(2));
            _queueReader.Verify(r => r.SaveSaga(It.IsAny<object>(), It.IsAny<InternalQueueContext>()), Times.Exactly(2));
        }

        /// <summary>
        /// Proves the behavior when the saga is blocked.
        /// </summary>
        /// <returns></returns>
        [TestMethod]
        public async Task Given_SagaIsBlocked_When_Invoke_Then_Return()
        {
            _findHandlers.Setup(f => f.FindHandlers<TestSagaMessage1>())
                .Returns(GetSagaHandler());

            var context = GetContext<TestSagaMessage1>();

            _queueReader.Setup(r => r.LoadSaga(It.IsAny<object>(), It.IsAny<InternalQueueContext>()))
                .Callback<object, InternalQueueContext>((s, c) =>
                {
                    c.SagaData = new PeachtreeBus.Model.SagaData
                    {
                        Blocked = true
                    };
                });

            await _testSubject.Invoke(context, null);

            // the saga data will get loaded.
            _queueReader.Verify(r => r.LoadSaga(It.IsAny<object>(), It.IsAny<InternalQueueContext>()), Times.Once);

            // the saga data will not be saved.
            _queueReader.Verify(r => r.SaveSaga(It.IsAny<object>(), It.IsAny<InternalQueueContext>()), Times.Never);

            // a block will be reported.
            Assert.IsTrue(context.SagaBlocked);

            // the saga handler should not be called.
            _testSaga.AssertInvocations(0);
        }

        /// <summary>
        /// Proves behavior when a saga has not started yet.
        /// </summary>
        /// <returns></returns>
        [TestMethod]
        [ExpectedException(typeof(SagaNotStartedException))]
        public async Task Given_SagaNotStarted_And_MessageIsNotAStart_When_Invoke_Then_Throw()
        {
            // message is handled by the saga
            _findHandlers.Setup(f => f.FindHandlers<TestSagaMessage1>())
                .Returns(GetSagaHandler());

            // the handler for this message is not IHandleSagaStartMessage<>
            var context = GetContext<TestSagaMessage1>();

            // returning null saga data means that the saga has not been started.
            _queueReader.Setup(r => r.LoadSaga(It.IsAny<object>(), It.IsAny<InternalQueueContext>()))
                .Callback<object, InternalQueueContext>((s, c) =>
                {
                    c.SagaData = null;
                });

            // this should throw
            await _testSubject.Invoke(context, null);
        }

        /// <summary>
        /// Proves behavior when a message has no handlers.
        /// </summary>
        /// <returns></returns>
        [TestMethod]
        [ExpectedException(typeof(QueueMessageNoHandlerException))]
        public async Task Given_MessageHasNoHandlers_When_Invoke_Then_Throws()
        {
            _findHandlers.Setup(f => f.FindHandlers<TestSagaMessage1>())
                .Returns(GetNoHandlers());

            var context = GetContext<TestSagaMessage1>();
            await _testSubject.Invoke(context, null);
        }

        /// <summary>
        /// Proves behavior when message type unrecognized.
        /// </summary>
        /// <returns></returns>
        [TestMethod]
        [ExpectedException(typeof(QueueMessageClassNotRecognizedException))]
        public async Task Given_AMessageContextWithAnUnrecognizedMessageType_When_Invoke_Then_Throws()
        {
            var context = CreateContextWithUnrecognizedMessageType();
            await _testSubject.Invoke(context, null);
        }

        [TestMethod]
        [ExpectedException(typeof(MissingInterfaceException))]
        public async Task Given_AMessageThatDoesNotImplementIQueueMessage_When_Invoke_Then_Throws()
        {
            var context = GetContext<object>();// object does not implement IQueueMessage
            await _testSubject.Invoke(context, null);
        }

        private IEnumerable<IHandleQueueMessage<TestSagaMessage1>> GetSagaHandler()
        {
            List<IHandleQueueMessage<TestSagaMessage1>> result = [_testSaga];
            return result;
        }

        private static IEnumerable<IHandleQueueMessage<TestSagaMessage1>> GetNoHandlers()
        {
            var list = new List<IHandleQueueMessage<TestSagaMessage1>>();
            return list;
        }

        private static InternalQueueContext GetContext<TMessage>()
            where TMessage : new()
        {
            var type = typeof(TMessage);
            return new InternalQueueContext()
            {
                Headers = new()
                {
                    MessageClass = type.FullName + ", " + type.Assembly.GetName().Name,
                },
                Message = new TMessage(),
                MessageData = new(),
                SavepointName = "BeforeMessageHandler",
            };
        }

        private static InternalQueueContext CreateContextWithUnrecognizedMessageType()
        {
            return new InternalQueueContext()
            {
                MessageData = new PeachtreeBus.Model.QueueMessage
                {
                    MessageId = Guid.NewGuid(),
                },
                Headers = new Headers
                {
                    MessageClass = "Peachtreebus.Tests.Sagas.NotARealMessageType, Peachtreebus.Tests"
                },
                Message = new TestSagaMessage1()
            };
        }
    }
}
