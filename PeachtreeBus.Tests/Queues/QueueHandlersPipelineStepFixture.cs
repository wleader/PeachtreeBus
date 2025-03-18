using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using PeachtreeBus.Data;
using PeachtreeBus.Exceptions;
using PeachtreeBus.Queues;
using PeachtreeBus.Sagas;
using PeachtreeBus.Tests.Sagas;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace PeachtreeBus.Tests.Queues
{
    [TestClass]
    public class QueueHandlersPipelineStepFixture
    {
        private QueueHandlersPipelineStep _testSubject = default!;
        private Mock<IFindQueueHandlers> _findHandlers = default!;
        private Mock<ILogger<QueueHandlersPipelineStep>> _log = default!;
        private Mock<ISagaMessageMapManager> _sagaMessageMapManager = default!;
        private Mock<IQueueReader> _queueReader = default!;

        private TestSaga _testSaga = default!;
        private QueueContext context = default!;

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

            context = TestData.CreateQueueContext(
                userMessageFunc: () => new TestSagaMessage1());

            Assert.AreEqual(typeof(TestSagaMessage1), context.Message.GetType(),
                "This test suite expects the default user message type to be TestSagaMessage1");
        }

        [TestMethod]
        public async Task Given_MessageIsNotIQueuedMessage_Then_ThrowsUsefulException()
        {
            context = TestData.CreateQueueContext(
                headers: new(typeof(object)));
            await Assert.ThrowsExceptionAsync<TypeIsNotIQueueMessageException>(() => _testSubject.Invoke(context, null));
        }

        /// <summary>
        /// Proves behavior when the handler is a saga
        /// </summary>
        /// <returns></returns>
        [TestMethod]
        public async Task Given_MessageIsHandledBySaga_When_Invoke_Then_SagaHandlesMessage()
        {
            _findHandlers.Setup(f => f.FindHandlers<TestSagaMessage1>())
                .Returns(() => [_testSaga]);

            _queueReader.Setup(r => r.LoadSaga(It.IsAny<object>(), It.IsAny<IQueueContext>()))
                .Callback<object, IQueueContext>((s, c) =>
                {
                    (c as QueueContext)!.SagaData = new SagaData
                    {
                        SagaId = UniqueIdentity.New(),
                        Blocked = false,
                        Key = new("SagaKey"),
                        Data = new("Data")
                    };
                });

            await _testSubject.Invoke(context, null!);

            _queueReader.Verify(r => r.LoadSaga(It.IsAny<object>(), It.IsAny<QueueContext>()), Times.Once);
            _testSaga.AssertInvocations(1);
            _testSaga.AssertInvoked(context, (TestSagaMessage1)context.Message);
            _queueReader.Verify(r => r.SaveSaga(It.IsAny<object>(), It.IsAny<QueueContext>()), Times.Once);

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

            _queueReader.Setup(r => r.LoadSaga(It.IsAny<object>(), It.IsAny<IQueueContext>()))
                .Callback<object, IQueueContext>((s, c) =>
                {
                    (c as QueueContext)!.SagaData = new SagaData
                    {
                        SagaId = UniqueIdentity.New(),
                        Blocked = false,
                        Key = new("SagaKey"),
                        Data = new("Data")
                    };
                });

            await _testSubject.Invoke(context, null);

            saga1.AssertInvocations(1);
            saga1.AssertInvoked(context, (TestSagaMessage1)context.Message);
            saga2.AssertInvocations(1);
            saga2.AssertInvoked(context, (TestSagaMessage1)context.Message);
            handler1.Verify(h => h.Handle(context, (TestSagaMessage1)context.Message), Times.Once);
            handler2.Verify(h => h.Handle(context, (TestSagaMessage1)context.Message), Times.Once);

            _queueReader.Verify(r => r.LoadSaga(It.IsAny<object>(), It.IsAny<QueueContext>()), Times.Exactly(2));
            _queueReader.Verify(r => r.SaveSaga(It.IsAny<object>(), It.IsAny<QueueContext>()), Times.Exactly(2));
        }

        /// <summary>
        /// Proves the behavior when the saga is blocked.
        /// </summary>
        /// <returns></returns>
        [TestMethod]
        public async Task Given_SagaIsBlocked_When_Invoke_Then_Return()
        {
            _findHandlers.Setup(f => f.FindHandlers<TestSagaMessage1>())
                .Returns(() => [_testSaga]);

            _queueReader.Setup(r => r.LoadSaga(It.IsAny<object>(), It.IsAny<IQueueContext>()))
                .Callback<object, IQueueContext>((s, c) =>
                {
                    (c as QueueContext)!.SagaData = new SagaData
                    {
                        SagaId = UniqueIdentity.New(),
                        Blocked = true,
                        Key = new("SagaKey"),
                        Data = new("Data")
                    };
                });

            await _testSubject.Invoke(context, null);

            // the saga data will get loaded.
            _queueReader.Verify(r => r.LoadSaga(It.IsAny<object>(), It.IsAny<QueueContext>()), Times.Once);

            // the saga data will not be saved.
            _queueReader.Verify(r => r.SaveSaga(It.IsAny<object>(), It.IsAny<QueueContext>()), Times.Never);

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
        public async Task Given_SagaNotStarted_And_MessageIsNotAStart_When_Invoke_Then_Throw()
        {
            // message is handled by the saga
            _findHandlers.Setup(f => f.FindHandlers<TestSagaMessage1>())
                .Returns(() => [_testSaga]);

            // the handler for this message is not IHandleSagaStartMessage<>

            // returning null saga data means that the saga has not been started.
            _queueReader.Setup(r => r.LoadSaga(It.IsAny<object>(), It.IsAny<IQueueContext>()))
                .Callback<object, IQueueContext>((s, c) =>
                {
                    var context = (c as QueueContext)!;
                    context.SagaData = null;
                    context.SagaKey = new("SagaKey");
                });

            // this should throw
            await Assert.ThrowsExceptionAsync<SagaNotStartedException>(() =>
                _testSubject.Invoke(context, null));
        }

        /// <summary>
        /// Proves behavior when a message has no handlers.
        /// </summary>
        /// <returns></returns>
        [TestMethod]
        public async Task Given_MessageHasNoHandlers_When_Invoke_Then_Throws()
        {
            _findHandlers.Setup(f => f.FindHandlers<TestSagaMessage1>())
                .Returns(() => []);

            await Assert.ThrowsExceptionAsync<QueueMessageNoHandlerException>(() =>
                _testSubject.Invoke(context, null));
        }

        /// <summary>
        /// Proves behavior when message type unrecognized.
        /// </summary>
        /// <returns></returns>
        [TestMethod]
        public async Task Given_AMessageContextWithAnUnrecognizedMessageType_When_Invoke_Then_Throws()
        {
            var context = TestData.CreateQueueContext(
                headers: TestData.CreateHeadersWithUnrecognizedMessageClass());
            await Assert.ThrowsExceptionAsync<QueueMessageClassNotRecognizedException>(() =>
                _testSubject.Invoke(context, null));
        }

        [TestMethod]
        public async Task Given_AMessageThatDoesNotImplementIQueueMessage_When_Invoke_Then_Throws()
        {
            var context = TestData.CreateQueueContext(
                userMessageFunc: () => new object()); // object does not implement IQueueMessage
            await Assert.ThrowsExceptionAsync<TypeIsNotIQueueMessageException>(() => _testSubject.Invoke(context, null));
        }

        [TestMethod]
        public async Task Given_FindHandlerReturnsNull_When_Invoke_Then_Throws()
        {
            _findHandlers.Setup(f => f.FindHandlers<TestSagaMessage1>())
                .Returns((IEnumerable<IHandleQueueMessage<TestSagaMessage1>>)null!);

            await Assert.ThrowsExceptionAsync<IncorrectImplementationException>(() =>
                _testSubject.Invoke(context, null));
        }
    }
}
