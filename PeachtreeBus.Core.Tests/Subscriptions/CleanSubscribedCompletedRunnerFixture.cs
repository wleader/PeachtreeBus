using Microsoft.VisualStudio.TestTools.UnitTesting;
using PeachtreeBus.Core.Tests.Fakes;
using PeachtreeBus.Core.Tests.Tasks;
using PeachtreeBus.Subscriptions;

namespace PeachtreeBus.Core.Tests.Subscriptions;

[TestClass]
public class CleanSubscribedCompletedRunnerFixture : RunnerFixtureBase<CleanSubscribedCompletedRunner, ICleanSubscribedCompletedTask>
{
    protected override CleanSubscribedCompletedRunner CreateRunner()
    {
        return new(
            _dataAccess.Object,
            FakeLog.Create<CleanSubscribedCompletedRunner>(),
            _task.Object);
    }

    [TestMethod]
    public void Then_HasName()
    {
        Assert.AreEqual("CleanSubscribedCompleted", _testSubject.Name);
    }
}
