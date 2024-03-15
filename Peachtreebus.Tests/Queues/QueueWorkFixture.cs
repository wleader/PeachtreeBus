using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Peachtreebus.Tests.Sagas;
using PeachtreeBus;
using PeachtreeBus.Data;
using PeachtreeBus.Queues;
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
        private Mock<ILogger<QueueWork>> log;
        private Mock<IPerfCounters> counters;
        private Mock<IQueueReader> reader;
        private Mock<IBusDataAccess> dataAccess;
        private InternalQueueContext context;
        private Mock<IQueuePipelineInvoker> pipelineInvoker;

        [TestInitialize]
        public void TestInitialize()
        {
            log = new Mock<ILogger<QueueWork>>();
            counters = new Mock<IPerfCounters>();
            pipelineInvoker = new();

            reader = new Mock<IQueueReader>();
            dataAccess = new Mock<IBusDataAccess>();

            context = CreateContext();

            reader.Setup(r => r.GetNext("TestQueue"))
                .Returns(Task.FromResult(context));

            work = new QueueWork(log.Object,
                counters.Object,
                reader.Object,
                dataAccess.Object,
                pipelineInvoker.Object);

            work.QueueName = "TestQueue";
        }

        /// <summary>
        /// Proves that the thread is told to sleep when there are no mesages.
        /// </summary>
        /// <returns></returns>
        [TestMethod]
        public async Task Given_NoPendingMessages_When_DoWork_ThenReturnFalse()
        {
            reader.Setup(r => r.GetNext("TestQueue"))
                .Returns(Task.FromResult<InternalQueueContext>(null));

            var result = await work.DoWork();

            Assert.IsFalse(result);
        }

        [TestMethod]
        public async Task Given_AMessage_When_DoWork_Then_PipelineInvoked()
        {
            List<string> invocations = [];

            dataAccess.Setup(d => d.CreateSavepoint("BeforeMessageHandler"))
                .Callback(() => invocations.Add("Savepoint"));

            pipelineInvoker.Setup(p => p.Invoke(context))
                .Callback(() => invocations.Add("Pipeline"));

            reader.Setup(r => r.Complete(context))
                .Callback(() => invocations.Add("Complete"));

            var result = await work.DoWork();

            List<string> expected = ["Savepoint", "Pipeline", "Complete"];
            CollectionAssert.AreEqual(expected, invocations);

            dataAccess.Verify(d => d.RollbackToSavepoint(It.IsAny<string>()), Times.Never);
            reader.Verify(r => r.DelayMessage(It.IsAny<InternalQueueContext>(), It.IsAny<int>()), Times.Never);
            reader.Verify(r => r.Fail(It.IsAny<InternalQueueContext>(), It.IsAny<Exception>()), Times.Never);
        }

        [TestMethod]
        public async Task Given_PipelineWillThrow_When_DoWork_Then_ExceptionIsHandled()
        {
            List<string> invocations = [];

            var exception = new TestException();
            pipelineInvoker.Setup(i => i.Invoke(It.IsAny<InternalQueueContext>())).Throws(exception);

            dataAccess.Setup(d => d.RollbackToSavepoint("BeforeMessageHandler"))
                .Callback(() => invocations.Add("Rollback"));

            reader.Setup(r => r.Fail(context, exception))
                .Callback(() => invocations.Add("Fail"));

            var result = await work.DoWork();
            Assert.IsTrue(result);

            List<string> expected = ["Rollback", "Fail"];
            CollectionAssert.AreEqual(expected, invocations);
        }

        /// <summary>
        /// Proves that performance counters are updated.
        /// </summary>
        /// <returns></returns>
        [TestMethod]
        public async Task Given_AMessage_When_DoWork_Then_IncrementCounters()
        {
            var result = await work.DoWork();
            counters.Verify(c => c.StartMessage(), Times.Once);
            counters.Verify(c => c.FinishMessage(It.IsAny<DateTime>()), Times.Once);
        }

        /// <summary>
        /// Proves that a savepoint is always created.
        /// </summary>
        /// <returns></returns>
        [TestMethod]
        public async Task Given_AMessage_When_DoWork_Then_CreatesSavepoint()
        {
            var result = await work.DoWork();
            dataAccess.Verify(d => d.CreateSavepoint("BeforeMessageHandler"), Times.Once);
        }

        private static InternalQueueContext CreateContext()
        {
            return new InternalQueueContext()
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
    }
}
