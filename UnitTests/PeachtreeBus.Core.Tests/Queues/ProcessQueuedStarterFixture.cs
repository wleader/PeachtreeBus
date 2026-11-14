using Microsoft.VisualStudio.TestTools.UnitTesting;
using PeachtreeBus.Core.Tests.Tasks;
using PeachtreeBus.Queues;
using PeachtreeBus.Tasks;

namespace PeachtreeBus.Core.Tests.Queues;

[TestClass]
public class ProcessQueuedStarterFixture : StarterFixtureBase<
    ProcessQueuedStarter,
    IProcessQueuedRunner,
    IAlwaysRunTracker,
    IProcessQueuedEstimator>
{
    protected override ProcessQueuedStarter CreateStarter()
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
