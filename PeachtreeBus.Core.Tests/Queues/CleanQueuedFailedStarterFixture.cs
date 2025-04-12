using Microsoft.VisualStudio.TestTools.UnitTesting;
using PeachtreeBus.Core.Tests.Tasks;
using PeachtreeBus.Queues;

namespace PeachtreeBus.Core.Tests.Queues;

[TestClass]
public class CleanQueuedFailedStarterFixture : StarterFixtureBase<
    CleanQueuedFailedStarter,
    ICleanQueuedFailedRunner,
    ICleanQueuedFailedTracker>
{
    public override CleanQueuedFailedStarter CreateStarter()
    {
        return new(_scopeFactory.Object, _tracker.Object);
    }
}