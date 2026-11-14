using Microsoft.VisualStudio.TestTools.UnitTesting;
using PeachtreeBus.Core.Tests.Tasks;
using PeachtreeBus.Subscriptions;
using PeachtreeBus.Tasks;

namespace PeachtreeBus.Core.Tests.Subscriptions;

[TestClass]
public class CleanSubscribedPendingStarterFixture : StarterFixtureBase<
    CleanSubscribedPendingStarter,
    ICleanSubscribedPendingRunner,
    ICleanSubscribedPendingTracker,
    IAlwaysOneEstimator,
    IScheduledTaskCounter>
{
    protected override CleanSubscribedPendingStarter CreateStarter()
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

