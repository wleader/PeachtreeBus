using Microsoft.VisualStudio.TestTools.UnitTesting;
using PeachtreeBus.Core.Tests.Tasks;
using PeachtreeBus.Subscriptions;

namespace PeachtreeBus.Core.Tests.Subscriptions;

[TestClass]
public class CleanSubscriptionsStarterFixture : StarterFixtureBase<
    CleanSubscriptionsStarter,
    ICleanSubscriptionsRunner,
    ICleanSubscriptionsTracker>
{
    public override CleanSubscriptionsStarter CreateStarter()
    {
        return new(_scopeFactory.Object, _tracker.Object, _taskCounter.Object);
    }
}
