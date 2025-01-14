using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using PeachtreeBus.Cleaners;
using PeachtreeBus.Data;
using System;
using System.Threading.Tasks;

namespace PeachtreeBus.Tests.Cleaners
{
    /// <summary>
    /// Proves the behavior of QueueCleaner
    /// </summary>
    [TestClass]
    public class QueueCleanerFixture
    {
        private QueueCleaner cleaner = default!;
        private Mock<IBusDataAccess> dataAccess = default!;
        private QueueCleanerConfiguration config = default!;

        [TestInitialize]
        public void TestInitialize()
        {
            dataAccess = new Mock<IBusDataAccess>();
            config = new QueueCleanerConfiguration("DefaultQueue",
                10, true, true, TimeSpan.FromSeconds(1), TimeSpan.FromSeconds(1));
            cleaner = new QueueCleaner(config, dataAccess.Object);
        }

        /// <summary>
        /// Proves teh data access clean completed is called with correct parameters.
        /// </summary>
        [TestMethod]
        public async Task CleanCompleted_PassesThrough()
        {
            var olderThan = DateTime.UtcNow.AddDays(-1);
            await cleaner.CleanCompleted(olderThan, 5);
            dataAccess.Verify(d => d.CleanQueueCompleted("DefaultQueue", olderThan, 5), Times.Once);
        }

        /// <summary>
        /// Proves teh data access clean failed is called with correct parameters.
        /// </summary>
        [TestMethod]
        public async Task CleanFailed_PassesThrough()
        {
            var olderThan = DateTime.UtcNow.AddDays(-1);
            await cleaner.CleanFailed(olderThan, 5);
            dataAccess.Verify(d => d.CleanQueueFailed("DefaultQueue", olderThan, 5), Times.Once);
        }
    }
}
