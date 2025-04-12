using Microsoft.VisualStudio.TestTools.UnitTesting;
using PeachtreeBus.Core.Tests.Tasks;
using PeachtreeBus.Subscriptions;

namespace PeachtreeBus.Core.Tests.Subscriptions;

[TestClass]
public class CleanSubscribedCompletedStarterFixture : StarterFixtureBase<
    CleanSubscribedCompletedStarter,
    ICleanSubscribedCompletedRunner,
    ICleanSubscribedCompletedTracker>
{
    public override CleanSubscribedCompletedStarter CreateStarter()
    {
        return new(_scopeFactory.Object, _tracker.Object);
    }
}

