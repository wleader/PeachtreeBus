using Microsoft.VisualStudio.TestTools.UnitTesting;
using PeachtreeBus.Core.Tests.Tasks;
using PeachtreeBus.Subscriptions;
using PeachtreeBus.Tasks;

namespace PeachtreeBus.Core.Tests.Subscriptions;

[TestClass]
public class ProcessSubscribedStarterFixture : StarterFixtureBase<
    ProcessSubscribedStarter,
    IProcessSubscribedRunner,
    IAlwaysRunTracker,
    IProcessSubscribedEstimator,
    IMessagingTaskCounter>
{
    public override ProcessSubscribedStarter CreateStarter()
    {
        return new(
            _scopeFactory.Object,
            _tracker.Object,
            _taskCounter.Object,
            _estimator.Object);
    }
}
