using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using PeachtreeBus.Data;
using PeachtreeBus.Subscriptions;
using System.Threading.Tasks;

namespace PeachtreeBus.Tests.Subscriptions;

/// <summary>
/// Proves the behavior of SubscribedThread
/// </summary>
[TestClass]
public class SubscribedThreadFixture : ThreadFixtureBase<SubscribedThread>
{
    private readonly Mock<IBusDataAccess> dataAccess = new();
    private readonly Mock<ILogger<SubscribedThread>> log = new();
    private readonly Mock<ISubscribedWork> work = new();
    private BusConfiguration config = default!;

    [TestInitialize]
    public void TestInitialize()
    {
        config = TestData.CreateBusConfiguration();

        log.Reset();
        dataAccess.Reset();
        work.Reset();

        work.Setup(p => p.DoWork())
            .Callback(CancelToken)
            .ReturnsAsync(true);

        _testSubject = new SubscribedThread(
            log.Object,
            dataAccess.Object,
            config,
            work.Object);
    }

    /// <summary>
    /// Proves the unit of work is invoked.
    /// </summary>
    /// <returns></returns>
    [TestMethod]
    public async Task When_Run_Then_WorkRuns()
    {
        Assert.IsNotNull(config.SubscriptionConfiguration);
        await When_Run();
        work.VerifySet(p => p.SubscriberId = config.SubscriptionConfiguration.SubscriberId, Times.Once);
        work.Verify(p => p.DoWork(), Times.Once);
    }
}
