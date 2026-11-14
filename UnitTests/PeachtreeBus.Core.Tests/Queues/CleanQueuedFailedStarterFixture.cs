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
    IAlwaysOneEstimator>
{
    protected override CleanQueuedFailedStarter CreateStarter()
    {
        return new(
            Log.Object,
            ScopeFactory.Object,
            Tasks.Object,
            Tracker.Object,
            TaskCounter,
            Estimator.Object,
            DataAccess.Object);
    }
}