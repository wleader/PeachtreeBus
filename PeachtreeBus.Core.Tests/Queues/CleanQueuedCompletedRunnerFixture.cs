using Microsoft.VisualStudio.TestTools.UnitTesting;
using PeachtreeBus.Core.Tests.Tasks;
using PeachtreeBus.Queues;

namespace PeachtreeBus.Core.Tests.Queues;

[TestClass]
public class CleanQueuedCompletedRunnerFixture : RunnerFixtureBase<CleanQueuedCompletedRunner, ICleanQueuedCompletedTask>
{
    protected override CleanQueuedCompletedRunner CreateRunner()
    {
        return new(_dataAccess.Object, _log.Object, _task.Object);
    }

    [TestMethod]
    public void Then_HasName()
    {
        Assert.AreEqual("CleanQueuedCompleted", _testSubject.Name);
    }
}
