using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using PeachtreeBus.Data;
using PeachtreeBus.Subscriptions;
using System;
using System.Threading.Tasks;

namespace PeachtreeBus.Tests.Subscriptions
{
    /// <summary>
    /// Proves the behavior of SubscribedThread
    /// </summary>
    [TestClass]
    public class SubscribedThreadFixture
    {
        private SubscribedThread thread = default!;
        private Mock<IProvideShutdownSignal> shutdown = default!;
        private int loopCount = 1;
        private Mock<IBusDataAccess> dataAccess = default!;
        private Mock<ILogger<SubscribedThread>> log = default!;
        private Mock<ISubscribedWork> work = default!;
        private Mock<ISubscriptionUpdateWork> updater = default!;
        private SubscriberConfiguration config = default!;

        [TestInitialize]
        public void TestInitialize()
        {
            updater = new Mock<ISubscriptionUpdateWork>();

            config = new SubscriberConfiguration(
                SubscriberId.New(),
                TimeSpan.FromSeconds(30),
                new("cat1"), new("cat2"));

            shutdown = new Mock<IProvideShutdownSignal>();

            shutdown.SetupGet(s => s.ShouldShutdown)
                .Returns(() => loopCount > 0)
                .Callback(() => loopCount--);

            log = new Mock<ILogger<SubscribedThread>>();

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
            work.VerifySet(p => p.SubscriberId = It.IsAny<SubscriberId>(), Times.Once);
            work.Verify(p => p.DoWork(), Times.Once);
        }
    }
}
