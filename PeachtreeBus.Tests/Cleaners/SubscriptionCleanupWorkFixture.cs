using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using PeachtreeBus.Cleaners;
using PeachtreeBus.Data;
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
        private Mock<ISubscribedCleanupConfiguration> configuration = default!;
        private SubscriptionCleanupWork work = default!;

        [TestInitialize]
        public void TestInitialize()
        {
            dataAccess = new();
            configuration = new();

            configuration.SetupGet(c => c.MaxDeleteCount).Returns(100);

            work = new SubscriptionCleanupWork(
                dataAccess.Object,
                configuration.Object);
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
    }
}
