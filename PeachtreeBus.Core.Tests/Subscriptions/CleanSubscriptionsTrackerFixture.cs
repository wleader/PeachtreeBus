using Microsoft.VisualStudio.TestTools.UnitTesting;
using PeachtreeBus.Core.Tests.Fakes;
using PeachtreeBus.Core.Tests.Tasks;
using PeachtreeBus.Subscriptions;

namespace PeachtreeBus.Core.Tests.Subscriptions;

[TestClass]
public class CleanSubscriptionsTrackerFixture
    : IntervalRunTrackerFixtureBase<CleanSubscriptionsTracker>
{
    protected override CleanSubscriptionsTracker CreateTracker()
    {
        return new(_clock, _busConfiguration.Object);
    }

    protected override void Given_Configuration() =>
        _busConfiguration.Given_SubscriptionConfiguration();

    protected override void Given_NoConfiguration() =>
        _busConfiguration.Given_NoSubscriptionConfiguration();
}
