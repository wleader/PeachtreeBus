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
    IAlwaysOneEstimator>
{
    protected override CleanSubscriptionsStarter CreateStarter()
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
