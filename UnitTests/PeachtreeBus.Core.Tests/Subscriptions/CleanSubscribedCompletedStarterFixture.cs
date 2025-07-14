using Microsoft.VisualStudio.TestTools.UnitTesting;
using PeachtreeBus.Core.Tests.Tasks;
using PeachtreeBus.Subscriptions;
using PeachtreeBus.Tasks;

namespace PeachtreeBus.Core.Tests.Subscriptions;

[TestClass]
public class CleanSubscribedCompletedStarterFixture : StarterFixtureBase<
    CleanSubscribedCompletedStarter,
    ICleanSubscribedCompletedRunner,
    ICleanSubscribedCompletedTracker,
    IAlwaysOneEstimator,
    IScheduledTaskCounter>
{
    public override CleanSubscribedCompletedStarter CreateStarter()
    {
        return new(
            _log.Object,
            _scopeFactory.Object,
            _tasks.Object,
            _tracker.Object,
            _taskCounter.Object,
            _estimator.Object,
            _dataAccess.Object);
    }
}

