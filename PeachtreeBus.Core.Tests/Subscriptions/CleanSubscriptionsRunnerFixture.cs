using Microsoft.VisualStudio.TestTools.UnitTesting;
using PeachtreeBus.Core.Tests.Tasks;
using PeachtreeBus.Subscriptions;

namespace PeachtreeBus.Core.Tests.Subscriptions;

[TestClass]
public class CleanSubscriptionsRunnerFixture
    : RunnerFixtureBase<CleanSubscriptionsRunner, ICleanSubscriptionsTask>
{
    protected override CleanSubscriptionsRunner CreateRunner()
    {
        return new(_dataAccess.Object, _log.Object, _task.Object);
    }
}
