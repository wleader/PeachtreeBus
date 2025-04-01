using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using PeachtreeBus.Cleaners;
using PeachtreeBus.Data;
using PeachtreeBus.Queues;
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
        private IBusConfiguration config = default!;

        [TestInitialize]
        public void TestInitialize()
        {
            dataAccess = new Mock<IBusDataAccess>();
            config = TestData.CreateBusConfiguration();
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
            dataAccess.Verify(d => d.CleanQueueCompleted(config.QueueConfiguration!.QueueName, olderThan, 5), Times.Once);
        }

        /// <summary>
        /// Proves teh data access clean failed is called with correct parameters.
        /// </summary>
        [TestMethod]
        public async Task CleanFailed_PassesThrough()
        {
            var olderThan = DateTime.UtcNow.AddDays(-1);
            await cleaner.CleanFailed(olderThan, 5);
            dataAccess.Verify(d => d.CleanQueueFailed(config.QueueConfiguration!.QueueName, olderThan, 5), Times.Once);
        }

        [TestMethod]
        public async Task Given_NoQueueConfiguration_When_DoWork_NoWorkIsDone()
        {
            config = new BusConfiguration()
            {
                ConnectionString = "",
                Schema = new("PeachtreeBus"),
            };
            cleaner = new QueueCleaner(config, dataAccess.Object);

            var olderThan = DateTime.UtcNow.AddDays(-1);

            await cleaner.CleanCompleted(olderThan, 5);
            dataAccess.Verify(d => d.CleanQueueCompleted(
                It.IsAny<QueueName>(),
                It.IsAny<UtcDateTime>(),
                It.IsAny<int>()),
                Times.Never);

            await cleaner.CleanFailed(olderThan, 5);
            dataAccess.Verify(d => d.CleanQueueFailed(
                It.IsAny<QueueName>(),
                It.IsAny<UtcDateTime>(),
                It.IsAny<int>()),
                Times.Never);
        }
    }
}
