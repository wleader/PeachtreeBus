using Microsoft.VisualStudio.TestTools.UnitTesting;
using PeachtreeBus.Core.Tests.Tasks;
using PeachtreeBus.Subscriptions;

namespace PeachtreeBus.Core.Tests.Subscriptions;

[TestClass]
public class CleanSubscribedFailedStarterFixture : StarterFixtureBase<
    CleanSubscribedFailedStarter,
    ICleanSubscribedFailedRunner,
    ICleanSubscribedFailedTracker>
{
    public override CleanSubscribedFailedStarter CreateStarter()
    {
        return new(_scopeFactory.Object, _tracker.Object, _taskCounter.Object);
    }
}

