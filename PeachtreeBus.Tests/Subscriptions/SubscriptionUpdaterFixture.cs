using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using PeachtreeBus.Data;
using PeachtreeBus.Interfaces;
using PeachtreeBus.Subscriptions;
using System;
using System.Threading.Tasks;

namespace PeachtreeBus.Tests.Subscriptions
{
    /// <summary>
    /// Proves the behavior SubscriptionUpdater
    /// </summary>
    [TestClass]
    public class SubscriptionUpdaterFixture
    {
        private SubscriptionUpdateWork Updater = default!;
        private BusConfiguration config = default!;
        private Mock<IBusDataAccess> dataAccess = default!;
        private Mock<ISystemClock> clock = default!;

        [TestInitialize]
        public void TestInitialize()
        {
            config = TestData.CreateBusConfiguration();
            //config = new SubscriberConfiguration(
            //    SubscriberId.New(),
            //    TimeSpan.FromSeconds(30),
            //    new("cat1"), new("cat2"));
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
            now = now.Add(config.SubscriptionConfiguration!.Lifespan / 2);
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
            Assert.IsNotNull(config.SubscriptionConfiguration);
            Assert.IsTrue(config.SubscriptionConfiguration.Categories.Count > 1);

            await Updater.DoWork();

            var expectedUntil = clock.Object.UtcNow.Add(config.SubscriptionConfiguration!.Lifespan);
            var expectedSubscriber = config.SubscriptionConfiguration.SubscriberId;
            foreach (var cat in config.SubscriptionConfiguration.Categories)
            {
                dataAccess.Verify(d => d.Subscribe(expectedSubscriber, cat, expectedUntil), Times.Once);
            }
        }
    }
}
