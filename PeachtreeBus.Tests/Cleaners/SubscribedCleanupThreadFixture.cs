using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using PeachtreeBus.Cleaners;
using PeachtreeBus.Data;
using System.Threading.Tasks;

namespace PeachtreeBus.Tests.Cleaners
{
    /// <summary>
    /// Proves the behavior of SubscribedCleanupThread
    /// </summary>
    [TestClass]
    public class SubscribedCleanupThreadFixture
    {
        private SubscribedCleanupThread thread = default!;
        private Mock<ILogger<SubscribedCleanupThread>> log = default!;
        private Mock<IBusDataAccess> dataAccess = default!;
        private Mock<IProvideShutdownSignal> shutdown = default!;
        private Mock<ISubscribedCleanupWork> cleaner = default!;

        [TestInitialize]
        public void TestInitialize()
        {
            log = new Mock<ILogger<SubscribedCleanupThread>>();
            dataAccess = new Mock<IBusDataAccess>();
            shutdown = new Mock<IProvideShutdownSignal>();
            cleaner = new Mock<ISubscribedCleanupWork>();

            thread = new SubscribedCleanupThread(log.Object,
                dataAccess.Object, shutdown.Object, cleaner.Object);
        }

        /// <summary>
        /// Proves the cleaner is invoked.
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
