using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using PeachtreeBus.Cleaners;
using PeachtreeBus.Data;
using System;
using System.Threading.Tasks;

namespace PeachtreeBus.Tests.Cleaners
{
    /// <summary>
    /// Proves the behavior of SubscribedCleaner
    /// </summary>
    [TestClass]
    public class SubscribedCleanerFixture
    {
        private SubscribedCleaner cleaner = default!;
        private Mock<IBusDataAccess> dataAccess = default!;

        [TestInitialize]
        public void TestInitialize()
        {
            dataAccess = new Mock<IBusDataAccess>();
            cleaner = new SubscribedCleaner(dataAccess.Object);
        }

        /// <summary>
        /// Proves that Clean Subscribed Complete is called with correct parameters.
        /// </summary>
        [TestMethod]
        public async Task CleanCompleted_PassesThrough()
        {
            var olderThan = DateTime.UtcNow.AddDays(-1);
            await cleaner.CleanCompleted(olderThan, 5);
            dataAccess.Verify(d => d.CleanSubscribedCompleted(olderThan, 5), Times.Once);
        }

        /// <summary>
        /// Proves that clean subscribed failed is called with correct parameters.
        /// </summary>
        [TestMethod]
        public async Task CleanFailed_PassesThrough()
        {
            var olderThan = DateTime.UtcNow.AddDays(-1);
            await cleaner.CleanFailed(olderThan, 5);
            dataAccess.Verify(d => d.CleanSubscribedFailed(olderThan, 5), Times.Once);
        }
    }
}
