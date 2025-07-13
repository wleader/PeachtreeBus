using Microsoft.VisualStudio.TestTools.UnitTesting;
using PeachtreeBus.Core.Tests.Tasks;
using PeachtreeBus.Queues;
using PeachtreeBus.Tasks;

namespace PeachtreeBus.Core.Tests.Queues;

[TestClass]
public class CleanQueuedFailedStarterFixture : StarterFixtureBase<
    CleanQueuedFailedStarter,
    ICleanQueuedFailedRunner,
    ICleanQueuedFailedTracker,
    IAlwaysOneEstimator,
    IScheduledTaskCounter>
{
    public override CleanQueuedFailedStarter CreateStarter()
    {
        return new(
            _scopeFactory.Object,
            _tracker.Object,
            _taskCounter.Object,
            _estimator.Object);
    }
}