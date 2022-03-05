using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using PeachtreeBus.Cleaners;
using PeachtreeBus.Data;
using System.Threading.Tasks;

namespace Peachtreebus.Tests.Cleaners
{

    /// <summary>
    /// Proves the behavior of SubscriptionCleanupWork
    /// </summary>
    [TestClass]
    public class SubscriptionCleanupWorkFixture
    {
        private Mock<IBusDataAccess> dataAccess;
        private SubscriptionCleanupWork work;

        [TestInitialize]
        public void TestInitialize()
        {
            dataAccess = new Mock<IBusDataAccess>();
            work = new SubscriptionCleanupWork(dataAccess.Object);
        }

        /// <summary>
        /// Proves that the correct Data Access Method is invoked.
        /// </summary>
        /// <returns></returns>
        [TestMethod]
        public async Task DoWork_DoesWork()
        {
            await work.DoWork();
            dataAccess.Verify(d => d.ExpireSubscriptions(), Times.Once);
        }
    }
}
