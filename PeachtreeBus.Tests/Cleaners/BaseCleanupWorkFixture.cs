using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using PeachtreeBus.Cleaners;
using PeachtreeBus.Interfaces;
using System;
using System.Threading.Tasks;

namespace PeachtreeBus.Tests.Cleaners
{
    /// <summary>
    /// Proves the behavior of BaseCleanupWork
    /// </summary>
    [TestClass]
    public class BaseCleanupWorkFixture
    {
        private BaseCleanupWork work = default!;
        private Mock<ISystemClock> clock = default!;
        private QueueConfiguration config = default!;
        private Mock<IBaseCleaner> cleaner = default!;

        [TestInitialize]
        public void TestInitialize()
        {
            clock = new Mock<ISystemClock>();
            clock.SetupGet(c => c.UtcNow)
                .Returns(new DateTime(2022, 3, 4, 10, 49, 32, 33, DateTimeKind.Utc));

            config = new()
            {
                QueueName = TestData.DefaultQueueName,
                CleanMaxRows = 5,
                CleanCompleted = true,
                CleanFailed = true,
                CleanInterval = TimeSpan.FromSeconds(60),
                CleanFailedAge = TimeSpan.FromSeconds(120),
                CleanCompleteAge = TimeSpan.FromSeconds(60),
            };

            cleaner = new Mock<IBaseCleaner>();
            cleaner.Setup(c => c.CleanCompleted(It.IsAny<DateTime>(), It.IsAny<int>()))
                .Returns(Task.FromResult(5L));
            cleaner.Setup(c => c.CleanFailed(It.IsAny<DateTime>(), It.IsAny<int>()))
                .Returns(Task.FromResult(5L));

            work = new BaseCleanupWork(config, clock.Object, cleaner.Object);
        }

        /// <summary>
        /// Proves that the cleanup waits the configured interval between cleanups.
        /// </summary>
        /// <returns></returns>
        [TestMethod]
        public async Task DoWork_DoesNotCleanTooOften()
        {
            var start = work.NextClean;

            cleaner.Setup(c => c.CleanCompleted(It.IsAny<DateTime>(), It.IsAny<int>()))
                .Returns(Task.FromResult(0L));
            cleaner.Setup(c => c.CleanFailed(It.IsAny<DateTime>(), It.IsAny<int>()))
                .Returns(Task.FromResult(0L));

            Assert.AreEqual(false, await work.DoWork());

            cleaner.Verify(c => c.CleanCompleted(It.IsAny<DateTime>(), It.IsAny<int>()), Times.Once);
            cleaner.Verify(c => c.CleanFailed(It.IsAny<DateTime>(), It.IsAny<int>()), Times.Once);

            Assert.AreEqual(clock.Object.UtcNow.Add(config.CleanInterval), work.NextClean);

            Assert.AreEqual(false, await work.DoWork());

            cleaner.Verify(c => c.CleanCompleted(It.IsAny<DateTime>(), It.IsAny<int>()), Times.Once);
            cleaner.Verify(c => c.CleanFailed(It.IsAny<DateTime>(), It.IsAny<int>()), Times.Once);

            Assert.AreEqual(clock.Object.UtcNow.Add(config.CleanInterval), work.NextClean);
        }

        /// <summary>
        /// Proves that the olderthan value is correctly computed from the
        /// clock and configuration.
        /// </summary>
        /// <returns></returns>
        [TestMethod]
        public async Task DoWork_UsesCorrectOlderThanParameter()
        {
            cleaner.Setup(c => c.CleanCompleted(It.IsAny<DateTime>(), It.IsAny<int>()))
                .Returns(Task.FromResult(0L));
            cleaner.Setup(c => c.CleanFailed(It.IsAny<DateTime>(), It.IsAny<int>()))
                .Returns(Task.FromResult(0L));

            Assert.AreEqual(false, await work.DoWork());

            var expectedOlderThan = clock.Object.UtcNow.Subtract(config.CleanCompleteAge);
            cleaner.Verify(c => c.CleanCompleted(expectedOlderThan, config.CleanMaxRows), Times.Once);

            expectedOlderThan = clock.Object.UtcNow.Subtract(config.CleanFailedAge);
            cleaner.Verify(c => c.CleanFailed(expectedOlderThan, config.CleanMaxRows), Times.Once);
        }

        /// <summary>
        /// Proves that the cleaner does not attempt to clean up too many rows.
        /// Proves that the cleaner reports that it wants to run again when it deleted the max.
        /// </summary>
        /// <returns></returns>
        [TestMethod]
        public async Task DoWork_WhenCleanCompleteIsMax()
        {
            var start = work.NextClean;

            cleaner.Setup(c => c.CleanCompleted(It.IsAny<DateTime>(), It.IsAny<int>()))
                .Returns(Task.FromResult(5L));
            cleaner.Setup(c => c.CleanFailed(It.IsAny<DateTime>(), It.IsAny<int>()))
                .Returns(Task.FromResult(5L));

            Assert.AreEqual(true, await work.DoWork());

            Assert.AreEqual(start, work.NextClean);

            cleaner.Verify(c => c.CleanCompleted(It.IsAny<DateTime>(), It.IsAny<int>()), Times.Once);
            cleaner.Verify(c => c.CleanFailed(It.IsAny<DateTime>(), It.IsAny<int>()), Times.Never);
        }

        /// <summary>
        /// Proves that the cleaner does not attempt to clean up too many rows.
        /// Proves that the cleaner reports that it wants to run again when it deleted the max.
        /// </summary>
        /// <returns></returns>
        [TestMethod]
        public async Task DoWork_WhenCleanCompleteIsGreaterThanMax()
        {
            var start = work.NextClean;

            cleaner.Setup(c => c.CleanCompleted(It.IsAny<DateTime>(), It.IsAny<int>()))
                .Returns(Task.FromResult(5L));
            cleaner.Setup(c => c.CleanFailed(It.IsAny<DateTime>(), It.IsAny<int>()))
                .Returns(Task.FromResult(5L));

            Assert.AreEqual(true, await work.DoWork());

            Assert.AreEqual(start, work.NextClean);

            cleaner.Verify(c => c.CleanCompleted(It.IsAny<DateTime>(), It.IsAny<int>()), Times.Once);
            cleaner.Verify(c => c.CleanFailed(It.IsAny<DateTime>(), It.IsAny<int>()), Times.Never);
        }

        /// <summary>
        /// Proves that the cleaner does not attempt to clean up too many rows.
        /// </summary>
        /// <returns></returns>
        [TestMethod]
        public async Task DoWork_WhenCleanCompleteIsLessThanMax()
        {
            cleaner.Setup(c => c.CleanCompleted(It.IsAny<DateTime>(), It.IsAny<int>()))
                .Returns(Task.FromResult(4L));
            cleaner.Setup(c => c.CleanFailed(It.IsAny<DateTime>(), It.IsAny<int>()))
                .Returns(Task.FromResult(5L));

            await work.DoWork();

            cleaner.Verify(c => c.CleanCompleted(It.IsAny<DateTime>(), It.IsAny<int>()), Times.Once);
            cleaner.Verify(c => c.CleanFailed(It.IsAny<DateTime>(), It.IsAny<int>()), Times.Once);
        }

        /// <summary>
        /// Proves that the cleaner does not attempt to clean up too many rows.
        /// Proves that the cleaner wants to run again immediately.
        /// </summary>
        /// <returns></returns>
        [TestMethod]
        public async Task DoWork_WhenCleanFailedIsMax()
        {
            var start = work.NextClean;

            cleaner.Setup(c => c.CleanCompleted(It.IsAny<DateTime>(), It.IsAny<int>()))
                .Returns(Task.FromResult(0L));
            cleaner.Setup(c => c.CleanFailed(It.IsAny<DateTime>(), It.IsAny<int>()))
                .Returns(Task.FromResult(5L));

            Assert.IsTrue(await work.DoWork());

            cleaner.Verify(c => c.CleanCompleted(It.IsAny<DateTime>(), It.IsAny<int>()), Times.Once);
            cleaner.Verify(c => c.CleanFailed(It.IsAny<DateTime>(), It.IsAny<int>()), Times.Once);

            Assert.AreEqual(start, work.NextClean);
        }

        /// <summary>
        /// Proves that the cleaner is ok when the DataAccess cleans more than expected.
        /// </summary>
        /// <returns></returns>
        [TestMethod]
        public async Task DoWork_WhenCleanFailedIsGreaterThanMax()
        {
            var start = work.NextClean;

            cleaner.Setup(c => c.CleanCompleted(It.IsAny<DateTime>(), It.IsAny<int>()))
                .Returns(Task.FromResult(0L));
            cleaner.Setup(c => c.CleanFailed(It.IsAny<DateTime>(), It.IsAny<int>()))
                .Returns(Task.FromResult(6L));

            Assert.IsTrue(await work.DoWork());

            cleaner.Verify(c => c.CleanCompleted(It.IsAny<DateTime>(), It.IsAny<int>()), Times.Once);
            cleaner.Verify(c => c.CleanFailed(It.IsAny<DateTime>(), It.IsAny<int>()), Times.Once);

            Assert.AreEqual(start, work.NextClean);
        }

        /// <summary>
        /// Proves the cleaner doesn't clean too many rows.
        /// proves the cleaner doesn't want to run again right away.
        /// </summary>
        /// <returns></returns>
        [TestMethod]
        public async Task DoWork_WhenCleanFailedIsLessThanMax()
        {
            cleaner.Setup(c => c.CleanCompleted(It.IsAny<DateTime>(), It.IsAny<int>()))
                .Returns(Task.FromResult(0L));
            cleaner.Setup(c => c.CleanFailed(It.IsAny<DateTime>(), It.IsAny<int>()))
                .Returns(Task.FromResult(4L));

            Assert.IsTrue(await work.DoWork());

            cleaner.Verify(c => c.CleanCompleted(It.IsAny<DateTime>(), It.IsAny<int>()), Times.Once);
            cleaner.Verify(c => c.CleanFailed(It.IsAny<DateTime>(), It.IsAny<int>()), Times.Once);

            Assert.AreEqual(clock.Object.UtcNow.Add(config.CleanInterval), work.NextClean);
        }

        /// <summary>
        /// Proves the cleaner does not want to run again right away
        /// </summary>
        /// <returns></returns>
        [TestMethod]
        public async Task DoWork_WhenNothingIsDeleted()
        {
            cleaner.Setup(c => c.CleanCompleted(It.IsAny<DateTime>(), It.IsAny<int>()))
                .Returns(Task.FromResult(0L));
            cleaner.Setup(c => c.CleanFailed(It.IsAny<DateTime>(), It.IsAny<int>()))
                .Returns(Task.FromResult(0L));

            Assert.IsFalse(await work.DoWork());

            cleaner.Verify(c => c.CleanCompleted(It.IsAny<DateTime>(), It.IsAny<int>()), Times.Once);
            cleaner.Verify(c => c.CleanFailed(It.IsAny<DateTime>(), It.IsAny<int>()), Times.Once);

            Assert.AreEqual(clock.Object.UtcNow.Add(config.CleanInterval), work.NextClean);
        }

        /// <summary>
        /// Proves that completed is not cleaned when not enabled.
        /// </summary>
        /// <returns></returns>
        [TestMethod]
        public async Task DoWork_DoesNotCleanCompletedWhenTurnedOff()
        {
            cleaner.Setup(c => c.CleanCompleted(It.IsAny<DateTime>(), It.IsAny<int>()))
                .Returns(Task.FromResult(0L));
            cleaner.Setup(c => c.CleanFailed(It.IsAny<DateTime>(), It.IsAny<int>()))
                .Returns(Task.FromResult(0L));

            config = new()
            {
                QueueName = TestData.DefaultQueueName,
                CleanCompleted = false,
            };
            work = new BaseCleanupWork(config, clock.Object, cleaner.Object);
            Assert.IsFalse(await work.DoWork());
            cleaner.Verify(c => c.CleanCompleted(It.IsAny<DateTime>(), It.IsAny<int>()), Times.Never);
        }

        /// <summary>
        /// Proves that failed is not cleaned with not enabled.
        /// </summary>
        /// <returns></returns>
        [TestMethod]
        public async Task DoWork_DoesNotCleanFailedWhenTurnedOff()
        {
            cleaner.Setup(c => c.CleanCompleted(It.IsAny<DateTime>(), It.IsAny<int>()))
                            .Returns(Task.FromResult(0L));
            cleaner.Setup(c => c.CleanFailed(It.IsAny<DateTime>(), It.IsAny<int>()))
                .Returns(Task.FromResult(0L));

            config = new()
            {
                QueueName = TestData.DefaultQueueName,
                CleanFailed = false,
            };
            work = new BaseCleanupWork(config, clock.Object, cleaner.Object);
            Assert.IsFalse(await work.DoWork());
            cleaner.Verify(c => c.CleanFailed(It.IsAny<DateTime>(), It.IsAny<int>()), Times.Never);
        }
    }
}
