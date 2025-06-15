using Microsoft.VisualStudio.TestTools.UnitTesting;
using PeachtreeBus.Core.Tests.Tasks;
using PeachtreeBus.Queues;

namespace PeachtreeBus.Core.Tests.Queues;

[TestClass]
public class CleanQueuedCompletedStarterFixture : StarterFixtureBase<
    CleanQueuedCompletedStarter,
    ICleanQueuedCompletedRunner,
    ICleanQueuedCompletedTracker>
{
    public override CleanQueuedCompletedStarter CreateStarter()
    {
        return new(_scopeFactory.Object, _tracker.Object, _taskCounter.Object);
    }
}
