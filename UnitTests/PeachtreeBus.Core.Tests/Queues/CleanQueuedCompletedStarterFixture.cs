using Microsoft.VisualStudio.TestTools.UnitTesting;
using PeachtreeBus.Core.Tests.Tasks;
using PeachtreeBus.Queues;
using PeachtreeBus.Tasks;

namespace PeachtreeBus.Core.Tests.Queues;

[TestClass]
public class CleanQueuedCompletedStarterFixture : StarterFixtureBase<
    CleanQueuedCompletedStarter,
    ICleanQueuedCompletedRunner,
    ICleanQueuedCompletedTracker,
    IAlwaysOneEstimator,
    IScheduledTaskCounter>
{
    protected override CleanQueuedCompletedStarter CreateStarter()
    {
        return new(
            Log.Object,
            ScopeFactory.Object,
            Tasks.Object,
            Tracker.Object,
            TaskCounter.Object,
            Estimator.Object,
            DataAccess.Object);
    }
}
