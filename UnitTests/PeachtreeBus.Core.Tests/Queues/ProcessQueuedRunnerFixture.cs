using Microsoft.VisualStudio.TestTools.UnitTesting;
using PeachtreeBus.Core.Tests.Tasks;
using PeachtreeBus.Queues;

namespace PeachtreeBus.Core.Tests.Queues;

[TestClass]
public class ProcessQueuedRunnerFixture : RunnerFixtureBase<ProcessQueuedRunner, IProcessQueuedTask>
{
    protected override ProcessQueuedRunner CreateRunner()
    {
        return new ProcessQueuedRunner(_dataAccess.Object, _log.Object, _task.Object);
    }
}
