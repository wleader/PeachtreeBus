using Microsoft.VisualStudio.TestTools.UnitTesting;
using PeachtreeBus.Core.Tests.Tasks;
using PeachtreeBus.Subscriptions;

namespace PeachtreeBus.Core.Tests.Subscriptions;

[TestClass]
public class CleanSubscribedPendingStarterFixture : StarterFixtureBase<
    CleanSubscribedPendingStarter,
    ICleanSubscribedPendingRunner,
    ICleanSubscribedPendingTracker>
{
    public override CleanSubscribedPendingStarter CreateStarter()
    {
        return new(_scopeFactory.Object, _tracker.Object, _taskCounter.Object);
    }
}

