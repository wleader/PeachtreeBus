using Microsoft.VisualStudio.TestTools.UnitTesting;
using PeachtreeBus.Core.Tests.Tasks;
using PeachtreeBus.Subscriptions;
using PeachtreeBus.Tasks;

namespace PeachtreeBus.Core.Tests.Subscriptions;

[TestClass]
public class CleanSubscriptionsStarterFixture : StarterFixtureBase<
    CleanSubscriptionsStarter,
    ICleanSubscriptionsRunner,
    ICleanSubscriptionsTracker,
    IAlwaysOneEstimator,
    IScheduledTaskCounter>
{
    protected override CleanSubscriptionsStarter CreateStarter()
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
