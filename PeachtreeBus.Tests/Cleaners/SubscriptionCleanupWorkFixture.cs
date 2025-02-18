using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using PeachtreeBus.Cleaners;
using PeachtreeBus.Data;
using PeachtreeBus.Subscriptions;
using System.Threading.Tasks;

namespace PeachtreeBus.Tests.Cleaners
{

    /// <summary>
    /// Proves the behavior of SubscriptionCleanupWork
    /// </summary>
    [TestClass]
    public class SubscriptionCleanupWorkFixture
    {
        private Mock<IBusDataAccess> dataAccess = default!;
        private Mock<IBusConfiguration> busConfig = default!;
        private SubscriptionCleanupWork work = default!;
        private SubscriptionConfiguration? config = default!;

        [TestInitialize]
        public void TestInitialize()
        {
            dataAccess = new();
            busConfig = new();

            config = new()
            {
                Topics = [],
                SubscriberId = SubscriberId.New(),
                CleanMaxRows = 100
            };

            busConfig.SetupGet(c => c.SubscriptionConfiguration).Returns(() => config);

            work = new SubscriptionCleanupWork(
                dataAccess.Object,
                busConfig.Object);
        }

        /// <summary>
        /// Proves that the correct Data Access Method is invoked.
        /// </summary>
        /// <returns></returns>
        [TestMethod]
        public async Task DoWork_DoesWork()
        {
            await work.DoWork();
            dataAccess.Verify(d => d.ExpireSubscriptions(100), Times.Once);
            dataAccess.Verify(d => d.ExpireSubscriptionMessages(100), Times.Once);
        }

        [TestMethod]
        public async Task Given_NoSubscriptionConfiguration_When_DoWork_ReturnsFalse()
        {
            busConfig.SetupGet(c => c.SubscriptionConfiguration).Returns((SubscriptionConfiguration?)null);
            await work.DoWork();
            dataAccess.Verify(d => d.ExpireSubscriptions(It.IsAny<int>()), Times.Never);
            dataAccess.Verify(d => d.ExpireSubscriptionMessages(It.IsAny<int>()), Times.Never);
        }
    }
}
