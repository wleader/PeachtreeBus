using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using PeachtreeBus.Data;
using PeachtreeBus.Interfaces;
using PeachtreeBus.Subscriptions;
using System;
using System.Threading.Tasks;

namespace Peachtreebus.Tests.Subscriptions
{
    /// <summary>
    /// Proves the behavior SubscriptionUpdater
    /// </summary>
    [TestClass]
    public class SubscriptionUpdaterFixture
    {
        private SubscriptionUpdateWork Updater;
        private SubscriberConfiguration config;
        private Mock<IBusDataAccess> dataAccess;
        private Mock<ISystemClock> clock;

        [TestInitialize]
        public void TestInitialize()
        {
            config = new SubscriberConfiguration(
                Guid.NewGuid(),
                TimeSpan.FromSeconds(30),
                "cat1", "cat2");
            dataAccess = new Mock<IBusDataAccess>();
            clock = new Mock<ISystemClock>();

            clock.SetupGet(c => c.UtcNow)
                .Returns(new DateTime(2022, 2, 23, 10, 49, 32, 33, DateTimeKind.Utc));

            Updater = new SubscriptionUpdateWork(
                dataAccess.Object,
                config,
                clock.Object);
        }

        /// <summary>
        /// Proves initial state
        /// </summary>
        [TestMethod]
        public void LastUpdateDefaultsToTheBeginingOfTime()
        {
            Assert.AreEqual(DateTime.MinValue, Updater.LastUpdate);
        }

        /// <summary>
        /// Proves the updates are not too frequent.
        /// </summary>
        /// <returns></returns>
        [TestMethod]
        public async Task DoWork_DoesNotUpdateTooFrequently()
        {
            // update once to bring the clock up to 'now'
            Assert.IsTrue(await Updater.DoWork());

            // update a second time Right away and see that it
            // does not update.
            Assert.IsFalse(await Updater.DoWork());

            // advance the clock.
            var now = clock.Object.UtcNow;
            now = now.Add(config.Lifespan / 2);
            clock.SetupGet(c => c.UtcNow).Returns(now);

            // see that it can update again.
            Assert.IsTrue(await Updater.DoWork());
        }

        /// <summary>
        /// Proves that all configured categories are subscribed.
        /// </summary>
        /// <returns></returns>
        [TestMethod]
        public async Task DoWork_SubscribesToCategories()
        {
            await Updater.DoWork();

            var unitl = clock.Object.UtcNow.Add(config.Lifespan);
            dataAccess.Verify(d => d.Subscribe(config.SubscriberId, "cat1", unitl), Times.Once);
            dataAccess.Verify(d => d.Subscribe(config.SubscriberId, "cat2", unitl), Times.Once);
        }

    }
}
