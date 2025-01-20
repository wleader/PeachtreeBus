using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using PeachtreeBus.Data;
using PeachtreeBus.Queues;
using PeachtreeBus.Tests.Sagas;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace PeachtreeBus.Tests.Queues
{
    /// <summary>
    ///  Proves the behavior of QueueWork
    /// </summary>
    [TestClass]
    public class QueueWorkFixture
    {
        private QueueWork work = default!;
        private Mock<ILogger<QueueWork>> log = default!;
        private Mock<IPerfCounters> counters = default!;
        private Mock<IQueueReader> reader = default!;
        private Mock<IBusDataAccess> dataAccess = default!;
        private InternalQueueContext context = default!;
        private Mock<IQueuePipelineInvoker> pipelineInvoker = default!;
        private readonly QueueName testQueue = new("TestQueue");

        [TestInitialize]
        public void TestInitialize()
        {
            log = new Mock<ILogger<QueueWork>>();
            counters = new Mock<IPerfCounters>();
            pipelineInvoker = new();

            reader = new Mock<IQueueReader>();
            dataAccess = new Mock<IBusDataAccess>();

            context = CreateContext();

            reader.Setup(r => r.GetNext(testQueue))
                .ReturnsAsync(context);

            work = new QueueWork(log.Object,
                counters.Object,
                reader.Object,
                dataAccess.Object,
                pipelineInvoker.Object)
            {
                QueueName = testQueue
            };
        }

        /// <summary>
        /// Proves that the thread is told to sleep when there are no mesages.
        /// </summary>
        /// <returns></returns>
        [TestMethod]
        public async Task Given_NoPendingMessages_When_DoWork_ThenReturnFalse()
        {
            reader.Setup(r => r.GetNext(testQueue))
                .ReturnsAsync((InternalQueueContext)null!);

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

        [TestMethod]
        public async Task Given_SagaIsBlocked_When_DoWork_Then_MessageIsDelayed()
        {
            context.SagaData = new() { Blocked = true };

            bool rolledBack = false;

            dataAccess.Setup(d => d.RollbackToSavepoint("BeforeMessageHandler"))
                .Callback((string s) =>
                {
                    rolledBack = true;
                });

            reader.Setup(r => r.DelayMessage(context, 250))
                .Callback((InternalQueueContext c, int ms) =>
                {
                    Assert.IsTrue(rolledBack, "Rollback did not happen before delaying message.");
                });

            var result = await work.DoWork();

            Assert.IsTrue(result);
            counters.Verify(c => c.SagaBlocked(), Times.Once);
            dataAccess.Verify(d => d.RollbackToSavepoint("BeforeMessageHandler"));
            Assert.AreEqual("RollbackToSavepoint", dataAccess.Invocations[dataAccess.Invocations.Count - 1].Method.Name);
            reader.Verify(r => r.DelayMessage(context, 250), Times.Once);
        }

        private static InternalQueueContext CreateContext()
        {
            return new InternalQueueContext()
            {
                MessageData = new QueueMessage
                {
                    MessageId = Guid.NewGuid(),
                },
                Headers = new Headers
                {
                    MessageClass = "PeachtreeBus.Tests.Sagas.TestSagaMessage1, PeachtreeBus.Tests"
                },
                Message = new TestSagaMessage1(),
            };
        }
    }
}
