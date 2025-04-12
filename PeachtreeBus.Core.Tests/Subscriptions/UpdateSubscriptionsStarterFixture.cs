using Microsoft.VisualStudio.TestTools.UnitTesting;
using PeachtreeBus.Core.Tests.Tasks;
using PeachtreeBus.Subscriptions;

namespace PeachtreeBus.Core.Tests.Subscriptions;

[TestClass]
public class UpdateSubscriptionsStarterFixture : StarterFixtureBase<
    UpdateSubscriptionsStarter,
    IUpdateSubscriptionsRunner,
    IUpdateSubscriptionsTracker>
{
    public override UpdateSubscriptionsStarter CreateStarter()
    {
        return new(_scopeFactory.Object, _tracker.Object);
    }
}
