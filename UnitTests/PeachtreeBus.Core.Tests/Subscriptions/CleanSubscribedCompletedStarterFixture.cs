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
    IAlwaysOneEstimator>
{
    protected override CleanSubscribedCompletedStarter CreateStarter()
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

