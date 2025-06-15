using Microsoft.VisualStudio.TestTools.UnitTesting;
using PeachtreeBus.Core.Tests.Fakes;
using PeachtreeBus.Core.Tests.Tasks;
using PeachtreeBus.Subscriptions;

namespace PeachtreeBus.Core.Tests.Subscriptions;

[TestClass]
public class CleanSubscribedPendingRunnerFixture : RunnerFixtureBase<CleanSubscribedPendingRunner, ICleanSubscribedPendingTask>
{
    protected override CleanSubscribedPendingRunner CreateRunner()
    {
        return new(
            _dataAccess.Object,
            FakeLog.Create<CleanSubscribedPendingRunner>(),
            _task.Object);
    }
}
