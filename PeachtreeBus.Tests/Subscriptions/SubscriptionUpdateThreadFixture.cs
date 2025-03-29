using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using PeachtreeBus.Data;
using PeachtreeBus.Subscriptions;
using System.Threading.Tasks;

namespace PeachtreeBus.Tests.Subscriptions
{
    /// <summary>
    /// Proves the behavior of SubscriptionUpdateThread
    /// </summary>
    [TestClass]
    public class SubscriptionUpdateThreadFixture : ThreadFixtureBase<SubscriptionUpdateThread>
    {
        private readonly Mock<ILogger<SubscriptionUpdateThread>> log = new();
        private readonly Mock<IBusDataAccess> dataAccess = new();
        private readonly Mock<ISubscriptionUpdateWork> updater = new();

        [TestInitialize]
        public void TestInitialize()
        {
            log.Reset();
            dataAccess.Reset();
            updater.Reset();

            updater.Setup(u => u.DoWork())
                .Callback(CancelToken)
                .ReturnsAsync(true);

            _testSubject = new SubscriptionUpdateThread(log.Object, dataAccess.Object, updater.Object);
        }

        /// <summary>
        /// Proves the unit of work is invoked
        /// </summary>
        /// <returns></returns>
        [TestMethod]
        public async Task When_Run_Then_WorkRuns()
        {
            await When_Run();
            updater.Verify(u => u.DoWork(), Times.Once);
        }
    }
}
