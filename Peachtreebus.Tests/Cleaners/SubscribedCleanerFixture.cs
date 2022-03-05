using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using PeachtreeBus.Cleaners;
using PeachtreeBus.Data;
using System;

namespace Peachtreebus.Tests.Cleaners
{
    /// <summary>
    /// Proves the behavior of SubscribedCleaner
    /// </summary>
    [TestClass]
    public class SubscribedCleanerFixture
    {
        private SubscribedCleaner cleaner;
        private Mock<IBusDataAccess> dataAccess;

        [TestInitialize]
        public void TestInitialize()
        {
            dataAccess = new Mock<IBusDataAccess>();
            cleaner = new SubscribedCleaner( dataAccess.Object);
        }

        /// <summary>
        /// Proves that Clean Subscribed Complete is called with correct parameters.
        /// </summary>
        [TestMethod]
        public void CleanCompleted_PassesThrough()
        {
            var olderThan = DateTime.UtcNow.AddDays(-1);
            cleaner.CleanCompleted(olderThan, 5);
            dataAccess.Verify(d => d.CleanSubscribedCompleted(olderThan, 5), Times.Once);
        }

        /// <summary>
        /// Proves that clean subscribed failed is called with correct parameters.
        /// </summary>
        [TestMethod]
        public void CleanFailed_PassesThrough()
        {
            var olderThan = DateTime.UtcNow.AddDays(-1);
            cleaner.CleanFailed(olderThan, 5);
            dataAccess.Verify(d => d.CleanSubscribedFailed(olderThan, 5), Times.Once);
        }
    }
}
