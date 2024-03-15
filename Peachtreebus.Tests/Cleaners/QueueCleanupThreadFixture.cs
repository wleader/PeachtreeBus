using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using PeachtreeBus;
using PeachtreeBus.Cleaners;
using PeachtreeBus.Data;
using System.Threading.Tasks;

namespace Peachtreebus.Tests.Cleaners
{
    /// <summary>
    /// Proves the behavior of QueueCleanupThread
    /// </summary>
    [TestClass]
    public class QueueCleanupThreadFixture
    {
        private QueueCleanupThread thread = default!;
        private Mock<ILogger<QueueCleanupThread>> log = default!;
        private Mock<IBusDataAccess> dataAccess = default!;
        private Mock<IProvideShutdownSignal> shutdown = default!;
        private Mock<IQueueCleanupWork> cleaner = default!;

        [TestInitialize]
        public void TestInitialize()
        {
            log = new Mock<ILogger<QueueCleanupThread>>();
            dataAccess = new Mock<IBusDataAccess>();
            shutdown = new Mock<IProvideShutdownSignal>();
            cleaner = new Mock<IQueueCleanupWork>();

            thread = new QueueCleanupThread(log.Object,
                dataAccess.Object, shutdown.Object, cleaner.Object);
        }

        /// <summary>
        /// Prove the cleaner is invoked.
        /// </summary>
        /// <returns></returns>
        [TestMethod]
        public async Task DoUnitOfwork_DoesWork()
        {
            await thread.DoUnitOfWork();
            cleaner.Verify(c => c.DoWork(), Times.Once);
        }
    }
}
