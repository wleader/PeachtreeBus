using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using PeachtreeBus;
using PeachtreeBus.Data;
using PeachtreeBus.Subscriptions;
using System;
using System.Threading.Tasks;

namespace Peachtreebus.Tests.Subscriptions
{
    /// <summary>
    /// Proves the behavior of SubscribedThread
    /// </summary>
    [TestClass]
    public class SubscribedThreadFixture
    {
        private SubscribedThread thread;
        private Mock<IProvideShutdownSignal> shutdown;
        private int loopCount = 1;
        private Mock<IBusDataAccess> dataAccess;
        private Mock<ILog<SubscribedThread>> log;
        private Mock<ISubscribedWork> work;
        private Mock<ISubscriptionUpdateWork> updater;
        private SubscriberConfiguration config;

        [TestInitialize]
        public void TestInitialize()
        {
            updater = new Mock<ISubscriptionUpdateWork>();

            config = new SubscriberConfiguration(
                Guid.NewGuid(),
                TimeSpan.FromSeconds(30),
                "cat1", "cat2");

            shutdown = new Mock<IProvideShutdownSignal>();

            shutdown.SetupGet(s => s.ShouldShutdown)
                .Returns(() => loopCount > 0)
                .Callback(() => loopCount--);

            log = new Mock<ILog<SubscribedThread>>();

            dataAccess = new Mock<IBusDataAccess>();

            work = new Mock<ISubscribedWork>();

            work.Setup(p => p.DoWork())
                .Returns(Task.FromResult(true));

            updater.Setup(u => u.DoWork()).Returns(Task.FromResult(true));

            thread = new SubscribedThread(
                shutdown.Object,
                log.Object,
                dataAccess.Object,
                config,
                work.Object);
        }

        /// <summary>
        /// Proves the unit of work is invoked.
        /// </summary>
        /// <returns></returns>
        [TestMethod]
        public async Task Run_CallsWorkDoDork()
        {
            await thread.Run();
            work.VerifySet(p => p.SubscriberId = It.IsAny<Guid>(), Times.Once);
            work.Verify(p => p.DoWork(), Times.Once);
        }
    }
}
