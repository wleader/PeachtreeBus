using Microsoft.VisualStudio.TestTools.UnitTesting;
using PeachtreeBus.Core.Tests.Tasks;
using PeachtreeBus.Subscriptions;
using PeachtreeBus.Tasks;

namespace PeachtreeBus.Core.Tests.Subscriptions;

[TestClass]
public class CleanSubscribedFailedStarterFixture : StarterFixtureBase<
    CleanSubscribedFailedStarter,
    ICleanSubscribedFailedRunner,
    ICleanSubscribedFailedTracker,
    IAlwaysOneEstimator>
{
    protected override CleanSubscribedFailedStarter CreateStarter()
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

