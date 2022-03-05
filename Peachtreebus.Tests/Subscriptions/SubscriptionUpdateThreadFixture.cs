using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using PeachtreeBus;
using PeachtreeBus.Data;
using PeachtreeBus.Subscriptions;
using System.Threading.Tasks;

namespace Peachtreebus.Tests.Subscriptions
{
    /// <summary>
    /// Proves the behavior of SubscriptionUpdateThread
    /// </summary>
    [TestClass]
    public class SubscriptionUpdateThreadFixture
    {
        private SubscriptionUpdateThread thread;
        private Mock<IProvideShutdownSignal> shutdown;
        private int loopCount = 1;
        private Mock<ILog<SubscriptionUpdateThread>> log;
        private Mock<IBusDataAccess> dataAccess;
        private Mock<ISubscriptionUpdateWork> updater;

        [TestInitialize]
        public void TestInitialize()
        {
            shutdown = new Mock<IProvideShutdownSignal>();
            log = new Mock<ILog<SubscriptionUpdateThread>>();
            dataAccess = new Mock<IBusDataAccess>();
            updater = new Mock<ISubscriptionUpdateWork>();

            shutdown.SetupGet(s => s.ShouldShutdown)
                .Returns(() => loopCount > 0)
                .Callback(() => loopCount--);

            thread = new SubscriptionUpdateThread(shutdown.Object, log.Object, dataAccess.Object, updater.Object);
        }

        /// <summary>
        /// Proves the unit of work is invoked
        /// </summary>
        /// <returns></returns>
        [TestMethod]
        public async Task Run_CallsUpdate()
        {
            await thread.Run();
            updater.Verify(u => u.DoWork(), Times.Once);
        }
    }
}
