using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using PeachtreeBus.Core.Tests.Fakes;
using PeachtreeBus.Core.Tests.Tasks;
using PeachtreeBus.Data;
using PeachtreeBus.Queues;
using PeachtreeBus.Tasks;
using System.Threading.Tasks;

namespace PeachtreeBus.Core.Tests.Queues;

[TestClass]
public class ProcessQueuedStarterFixture : StarterFixtureBase<
    ProcessQueuedStarter,
    IProcessQueuedRunner,
    IAlwaysRunTracker>
{
    private readonly Mock<IBusDataAccess> _dataAccess = new();
    private readonly Mock<IBusConfiguration> _busConfiguration = new();

    [TestInitialize]
    public override void Intialize()
    {
        _dataAccess.Reset();
        _busConfiguration.Reset();

        _busConfiguration.Given_QueueConfiguration();

        base.Intialize();
    }

    public override ProcessQueuedStarter CreateStarter()
    {
        return new(
            _scopeFactory.Object,
            _tracker.Object,
            _dataAccess.Object,
            _busConfiguration.Object);
    }

    public override int SetupEstimate(int estimate)
    {
        _dataAccess.Setup(d => d.EstimateQueuePending(
            _busConfiguration.Object.QueueConfiguration!.QueueName))
            .ReturnsAsync(estimate);
        return estimate;
    }

    [TestMethod]
    public async Task Given_NoConfiguration_When_Run_Then_Result()
    {
        _busConfiguration.Given_NoQueueConfiguration();

        Assert.AreEqual(0, await When_Run(1));

        Then_RunnersAreStarted(0);
    }
}
