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
    /// <summary>
    ///  Proves the behavior of QueueWork
    /// </summary>
    [TestClass]
    public class QueueWorkFixture
    {
        private QueueWork work;
        private Mock<ILog<QueueWork>> log;
        private Mock<IPerfCounters> counters;
        private Mock<IFindQueueHandlers> findHandlers;
        private Mock<IQueueReader> reader;
        private Mock<IBusDataAccess> dataAccess;

        [TestInitialize]
        public void TestInitialize()
        {
            log = new Mock<ILog<QueueWork>>();
            counters = new Mock<IPerfCounters>();
            findHandlers = new Mock<IFindQueueHandlers>();
            reader = new Mock<IQueueReader>();
            dataAccess = new Mock<IBusDataAccess>();

            reader.Setup(r => r.GetNext(It.IsAny<string>()))
                .Returns(Task.FromResult(CreateContext()));

            findHandlers.Setup(f => f.FindHandlers<TestSagaMessage1>())
                .Returns(GetOneGoodHandler());

            work = new QueueWork(log.Object,
                counters.Object,
                findHandlers.Object,
                reader.Object,
                dataAccess.Object,
                new SagaMessageMapManager());
        }

        /// <summary>
        /// Proves that the thread is told to sleep when there are no mesages.
        /// </summary>
        /// <returns></returns>
        [TestMethod]
        public async Task DoWork_WhenNoMessage_ThenReturnFalse()
        {
            reader.Setup(r => r.GetNext(It.IsAny<string>()))
                .Returns(Task.FromResult<QueueContext>(null));

            work.QueueName = "TestQueue";
            var result = await work.DoWork();

            Assert.IsFalse(result);
        }

        /// <summary>
        /// Proves that performance counters are updated.
        /// </summary>
        /// <returns></returns>
        [TestMethod]
        public async Task DoWork_WhenMessage_IncrementCounters()
        {
            work.QueueName = "TestQueue";
            var result = await work.DoWork();

            counters.Verify(c => c.StartMessage(), Times.Once);
            counters.Verify(c => c.FinishMessage(It.IsAny<DateTime>()), Times.Once);
        }

        /// <summary>
        /// Proves that a savepoint is always created.
        /// </summary>
        /// <returns></returns>
        [TestMethod]
        public async Task DoWork_WhenMessage_CreatesSavepoint()
        {
            work.QueueName = "TestQueue";
            var result = await work.DoWork();

            dataAccess.Verify(d => d.CreateSavepoint("BeforeMessageHandler"), Times.Once);
        }

        /// <summary>
        /// Proves behavior when message type unrecognized.
        /// </summary>
        /// <returns></returns>
        [TestMethod]
        public async Task DoWork_WhenMessageTypeUnrecognized()
        {
            reader.Setup(r => r.GetNext(It.IsAny<string>()))
                .Returns(Task.FromResult(CreateContextWithUnrecognizedMessageType()));

            work.QueueName = "TestQueue";
            var result = await work.DoWork();

            dataAccess.Verify(d => d.RollbackToSavepoint("BeforeMessageHandler"), Times.Once);
            reader.Verify(r => r.Fail(It.IsAny<QueueContext>(), It.IsAny<Exception>()), Times.Once);
            counters.Verify(c => c.FinishMessage(It.IsAny<DateTime>()), Times.Once);
            Assert.IsTrue(result);
        }

        /// <summary>
        /// Proves behavior when a message has no handlers.
        /// </summary>
        /// <returns></returns>
        [TestMethod]
        public async Task DoWork_WhenMessageAndNoHandlers()
        {
            findHandlers.Setup(f => f.FindHandlers<TestSagaMessage1>())
                .Returns(GetNoHandlers());

            work.QueueName = "TestQueue";
            var result = await work.DoWork();

            dataAccess.Verify(d => d.RollbackToSavepoint("BeforeMessageHandler"), Times.Once);
            reader.Verify(r => r.Fail(It.IsAny<QueueContext>(), It.IsAny<Exception>()), Times.Once);
            counters.Verify(c => c.FinishMessage(It.IsAny<DateTime>()), Times.Once);
            Assert.IsTrue(result);
        }

        /// <summary>
        /// Proves behavior when a message has on handler.
        /// </summary>
        /// <returns></returns>
        [TestMethod]
        public async Task DoWork_WhenMesageAndOneHandlers()
        {
            work.QueueName = "TestQueue";
            var result = await work.DoWork();

            dataAccess.Verify(d => d.RollbackToSavepoint("BeforeMessageHandler"), Times.Never);
            reader.Verify(r => r.Complete(It.IsAny<QueueContext>()), Times.Once);
            counters.Verify(c => c.FinishMessage(It.IsAny<DateTime>()), Times.Once);
            Assert.IsTrue(result);
        }

        /// <summary>
        /// Proves behavior when a message has multiple handlers
        /// </summary>
        /// <returns></returns>
        [TestMethod]
        public async Task DoWork_WhenMesageAndMultipleHandlers()
        {
            findHandlers.Setup(f => f.FindHandlers<TestSagaMessage1>())
                .Returns(GetTwoHandlers());

            work.QueueName = "TestQueue";
            var result = await work.DoWork();

            dataAccess.Verify(d => d.RollbackToSavepoint("BeforeMessageHandler"), Times.Never);
            reader.Verify(r => r.Complete(It.IsAny<QueueContext>()), Times.Once);
            counters.Verify(c => c.FinishMessage(It.IsAny<DateTime>()), Times.Once);
            Assert.IsTrue(result);
        }

        /// <summary>
        /// Proves behavior when the handler is a saga
        /// </summary>
        /// <returns></returns>
        [TestMethod]
        public async Task DoWork_WhenHandlerIsSaga()
        {
            findHandlers.Setup(f => f.FindHandlers<TestSagaMessage1>())
                .Returns(GetSagaHandler());

            reader.Setup(r => r.LoadSaga(It.IsAny<object>(), It.IsAny<QueueContext>()))
                .Callback<object, QueueContext>((s, c) =>
                {
                    c.SagaData = new PeachtreeBus.Model.SagaData
                    {
                        Blocked = false
                    };
                });

            work.QueueName = "TestQueue";
            var result = await work.DoWork();

            reader.Verify(r => r.LoadSaga(It.IsAny<object>(), It.IsAny<QueueContext>()), Times.Once);
            reader.Verify(r => r.SaveSaga(It.IsAny<object>(), It.IsAny<QueueContext>()), Times.Once);
            dataAccess.Verify(d => d.RollbackToSavepoint("BeforeMessageHandler"), Times.Never);
            reader.Verify(r => r.Complete(It.IsAny<QueueContext>()), Times.Once);
            Assert.IsTrue(result);
        }

        /// <summary>
        /// Proves the behavior when the saga is blocked.
        /// </summary>
        /// <returns></returns>
        [TestMethod]
        public async Task DoWork_WhenSagaBlocked()
        {
            findHandlers.Setup(f => f.FindHandlers<TestSagaMessage1>())
                .Returns(GetSagaHandler());

            reader.Setup(r => r.LoadSaga(It.IsAny<object>(), It.IsAny<QueueContext>()))
                .Callback<object, QueueContext>((s, c) =>
                {
                    c.SagaData = new PeachtreeBus.Model.SagaData
                    {
                        Blocked = true
                    };
                });

            work.QueueName = "TestQueue";
            var result = await work.DoWork();

            reader.Verify(r => r.LoadSaga(It.IsAny<object>(), It.IsAny<QueueContext>()), Times.Once);
            reader.Verify(r => r.SaveSaga(It.IsAny<object>(), It.IsAny<QueueContext>()), Times.Never);
            dataAccess.Verify(d => d.RollbackToSavepoint("BeforeMessageHandler"), Times.Once);
            reader.Verify(r => r.Complete(It.IsAny<QueueContext>()), Times.Never);
            reader.Verify(r => r.DelayMessage(It.IsAny<QueueContext>(), 250), Times.Once);
            Assert.IsTrue(result);
        }

        /// <summary>
        /// Proves behavior when a saga has not started yet.
        /// </summary>
        /// <returns></returns>
        [TestMethod]
        public async Task DoWork_WhenNoSagaDataAndNotStartHandler()
        {
            findHandlers.Setup(f => f.FindHandlers<TestSagaMessage1>())
                .Returns(GetSagaHandler());

            reader.Setup(r => r.LoadSaga(It.IsAny<object>(), It.IsAny<QueueContext>()))
                .Callback<object, QueueContext>((s, c) =>
                {
                    c.SagaData = null;
                });

            work.QueueName = "TestQueue";
            var result = await work.DoWork();

            dataAccess.Verify(d => d.RollbackToSavepoint("BeforeMessageHandler"), Times.Once);
            reader.Verify(r => r.Fail(It.IsAny<QueueContext>(), It.IsAny<Exception>()), Times.Once);
            counters.Verify(c => c.FinishMessage(It.IsAny<DateTime>()), Times.Once);
            Assert.IsTrue(result);
        }

        private QueueContext CreateContext()
        {
            return new QueueContext()
            {
                MessageData = new PeachtreeBus.Model.QueueMessage
                {
                    MessageId = Guid.NewGuid(),
                },
                Headers = new Headers
                {
                    MessageClass = "Peachtreebus.Tests.Sagas.TestSagaMessage1, Peachtreebus.Tests"
                },
                Message = new TestSagaMessage1()
            };
        }

        private QueueContext CreateContextWithUnrecognizedMessageType()
        {
            return new QueueContext()
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

        private IEnumerable<IHandleQueueMessage<TestSagaMessage1>> GetOneGoodHandler()
        {
            var list = new List<IHandleQueueMessage<TestSagaMessage1>>
            {
                new Mock<IHandleQueueMessage<TestSagaMessage1>>().Object
            };
            return list;
        }

        private IEnumerable<IHandleQueueMessage<TestSagaMessage1>> GetNoHandlers()
        {
            var list = new List<IHandleQueueMessage<TestSagaMessage1>>();
            return list;
        }

        private IEnumerable<IHandleQueueMessage<TestSagaMessage1>> GetTwoHandlers()
        {
            var list = new List<IHandleQueueMessage<TestSagaMessage1>>
            {
                new Mock<IHandleQueueMessage<TestSagaMessage1>>().Object,
                new Mock<IHandleQueueMessage<TestSagaMessage1>>().Object
            };
            return list;
        }

        private IEnumerable<IHandleQueueMessage<TestSagaMessage1>> GetSagaHandler()
        {
            var list = new List<IHandleQueueMessage<TestSagaMessage1>>
            {
                new TestSaga()
            };
            return list;
        }
    }
}
