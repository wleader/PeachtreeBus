﻿using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using PeachtreeBus;
using PeachtreeBus.Cleaners;
using PeachtreeBus.Data;
using PeachtreeBus.Interfaces;
using System;
using System.Threading.Tasks;

namespace Peachtreebus.Tests.Cleaners
{
    /// <summary>
    /// Proves the behavor of SubscriptionCleanupThread
    /// </summary>
    [TestClass]
    public class SubscriptionCleanupThreadFixture
    {
        private Mock<ISystemClock> clock;
        private Mock<ISubscriptionCleanupWork> cleaner;
        private Mock<IProvideShutdownSignal> shutdown;
        private Mock<ILogger<SubscriptionCleanupThread>> log;
        private Mock<IBusDataAccess> dataAccess;
        private SubscriptionCleanupThread thread;

        [TestInitialize]
        public void TestInitialize()
        {
            clock = new Mock<ISystemClock>();
            clock.SetupGet(c => c.UtcNow)
                .Returns(new DateTime(2022, 3, 4, 10, 49, 32, 33, DateTimeKind.Utc));


            cleaner = new Mock<ISubscriptionCleanupWork>();
            shutdown = new Mock<IProvideShutdownSignal>();
            log = new Mock<ILogger<SubscriptionCleanupThread>>();
            dataAccess = new Mock<IBusDataAccess>();

            thread = new SubscriptionCleanupThread(
                dataAccess.Object,
                log.Object,
                shutdown.Object,
                cleaner.Object,
                clock.Object);
        }

        /// <summary>
        /// Proves the cleanup thread doesn't clean too often
        /// </summary>
        /// <returns></returns>
        [TestMethod]
        public async Task DoUnitOfWork_DoesntRunTooOften()
        {
            thread.LastCleaned = clock.Object.UtcNow;
            Assert.IsFalse(await thread.DoUnitOfWork());
            cleaner.Verify(c => c.DoWork(), Times.Never);

            clock.SetupGet(c => c.UtcNow)
                .Returns(thread.LastCleaned.AddSeconds(16));

            Assert.IsTrue(await thread.DoUnitOfWork());
            cleaner.Verify(c => c.DoWork(), Times.Once);

            Assert.AreEqual(clock.Object.UtcNow, thread.LastCleaned);
        }
    }
}
