using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using PeachtreeBus.Data;
using PeachtreeBus.Subscriptions;
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
        private BusConfiguration config = default!;

        [TestInitialize]
        public void TestInitialize()
        {
            updater = new();

            config = TestData.CreateBusConfiguration();
            //config = new SubscriberConfiguration(
            //    SubscriberId.New(),
            //    TimeSpan.FromSeconds(30),
            //    new("cat1"), new("cat2"));

            shutdown = new();

            shutdown.SetupGet(s => s.ShouldShutdown)
                .Returns(() => loopCount > 0)
                .Callback(() => loopCount--);

            log = new();

            dataAccess = new();

            work = new();

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
            Assert.IsNotNull(config.SubscriptionConfiguration);

            await thread.Run();
            work.VerifySet(p => p.SubscriberId = config.SubscriptionConfiguration.SubscriberId, Times.Once);
            work.Verify(p => p.DoWork(), Times.Once);
        }
    }
}
