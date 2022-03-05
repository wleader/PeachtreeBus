using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using PeachtreeBus;
using PeachtreeBus.Data;
using PeachtreeBus.Queues;
using System.Threading.Tasks;

namespace Peachtreebus.Tests.Queues
{
    /// <summary>
    /// Proves the behavior of QueueThread
    /// </summary>
    [TestClass]
    public class QueueThreadFixture
    {
        private QueueThread thread;
        private Mock<IProvideShutdownSignal> shutdown;
        private int loopCount = 1;
        private Mock<IBusDataAccess> dataAccess;
        private Mock<ILog<QueueThread>> log;
        private Mock<IQueueWork> work;
        private QueueConfiguration config;

        [TestInitialize]
        public void TestInitialize()
        {
            config = new QueueConfiguration("QueueName");

            shutdown = new Mock<IProvideShutdownSignal>();

            shutdown.SetupGet(s => s.ShouldShutdown)
                .Returns(() => loopCount > 0)
                .Callback(() => loopCount--);

            log = new Mock<ILog<QueueThread>>();

            dataAccess = new Mock<IBusDataAccess>();

            work = new Mock<IQueueWork>();

            work.Setup(p => p.DoWork())
                .Returns(Task.FromResult(true));

            thread = new QueueThread(shutdown.Object, dataAccess.Object, log.Object, work.Object, config);
        }

        /// <summary>
        /// Proves the unit of work is invoked
        /// </summary>
        /// <returns></returns>
        [TestMethod]
        public async Task Run_CallsWorkDoWork()
        {
            await thread.Run();
            work.Verify(p => p.DoWork(), Times.Once);
        }
    }
}
