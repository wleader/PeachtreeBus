using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using PeachtreeBus.Data;
using PeachtreeBus.Queues;
using System.Threading.Tasks;

namespace PeachtreeBus.Tests.Queues
{
    /// <summary>
    /// Proves the behavior of QueueThread
    /// </summary>
    [TestClass]
    public class QueueThreadFixture
    {
        private QueueThread thread = default!;
        private Mock<IProvideShutdownSignal> shutdown = default!;
        private int loopCount = 1;
        private Mock<IBusDataAccess> dataAccess = default!;
        private Mock<ILogger<QueueThread>> log = default!;
        private Mock<IQueueWork> work = default!;
        private QueueConfiguration config = default!;

        [TestInitialize]
        public void TestInitialize()
        {
            config = new QueueConfiguration(new("QueueName"));

            shutdown = new Mock<IProvideShutdownSignal>();

            shutdown.SetupGet(s => s.ShouldShutdown)
                .Returns(() => loopCount > 0)
                .Callback(() => loopCount--);

            log = new Mock<ILogger<QueueThread>>();

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
