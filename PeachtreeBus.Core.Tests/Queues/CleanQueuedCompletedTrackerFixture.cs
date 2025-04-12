using Microsoft.VisualStudio.TestTools.UnitTesting;
using PeachtreeBus.Core.Tests.Fakes;
using PeachtreeBus.Core.Tests.Tasks;
using PeachtreeBus.Queues;

namespace PeachtreeBus.Core.Tests.Queues;

[TestClass]
public class CleanQueuedCompletedTrackerFixture
    : IntervalRunTrackerFixtureBase<CleanQueuedCompletedTracker>
{
    protected override CleanQueuedCompletedTracker CreateTracker()
    {
        return new(_clock, _busConfiguration.Object);
    }

    protected override void Given_Configuration() =>
        _busConfiguration.Given_QueueConfiguration();
    protected override void Given_NoConfiguration() =>
        _busConfiguration.Given_NoQueueConfiguration();

}
