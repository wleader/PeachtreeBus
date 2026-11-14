using Microsoft.VisualStudio.TestTools.UnitTesting;
using PeachtreeBus.Core.Tests.Tasks;
using PeachtreeBus.Subscriptions;
using PeachtreeBus.Tasks;

namespace PeachtreeBus.Core.Tests.Subscriptions;

[TestClass]
public class UpdateSubscriptionsStarterFixture : StarterFixtureBase<
    UpdateSubscriptionsStarter,
    IUpdateSubscriptionsRunner,
    IUpdateSubscriptionsTracker,
    IAlwaysOneEstimator,
    IScheduledTaskCounter>
{
    protected override UpdateSubscriptionsStarter CreateStarter()
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
