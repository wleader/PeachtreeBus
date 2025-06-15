using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using PeachtreeBus.Core.Tests.Fakes;
using PeachtreeBus.Core.Tests.Tasks;
using PeachtreeBus.Data;
using PeachtreeBus.Subscriptions;
using PeachtreeBus.Tasks;
using System.Threading.Tasks;

namespace PeachtreeBus.Core.Tests.Subscriptions;

[TestClass]
public class ProcessSubscribedStarterFixture : StarterFixtureBase<
    ProcessSubscribedStarter,
    IProcessSubscribedRunner,
    IAlwaysRunTracker>
{
    private readonly Mock<IBusDataAccess> _dataAccess = new();
    private readonly Mock<IBusConfiguration> _busConfiguration = new();

    [TestInitialize]
    public override void Intialize()
    {
        _dataAccess.Reset();
        _busConfiguration.Reset();

        _busConfiguration.Given_SubscriptionConfiguration();

        base.Intialize();
    }

    public override ProcessSubscribedStarter CreateStarter()
    {
        return new(
            _scopeFactory.Object,
            _tracker.Object,
            _dataAccess.Object,
            _busConfiguration.Object,
            _taskCounter.Object);
    }

    public override int SetupEstimate(int estimate)
    {
        _dataAccess.Setup(d => d.EstimateSubscribedPending(
            _busConfiguration.Object.SubscriptionConfiguration!.SubscriberId))
            .ReturnsAsync(estimate);
        return estimate;
    }

    [TestMethod]
    public async Task Given_NoConfiguration_When_Run_Then_Result()
    {
        _busConfiguration.Given_NoSubscriptionConfiguration();

        await When_Run(1);

        Then_RunnersAreStarted(0);
    }
}
