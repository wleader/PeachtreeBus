using Microsoft.VisualStudio.TestTools.UnitTesting;
using PeachtreeBus.Core.Tests.Fakes;
using PeachtreeBus.Core.Tests.Tasks;
using PeachtreeBus.Subscriptions;

namespace PeachtreeBus.Core.Tests.Subscriptions;

[TestClass]
public class CleanSubscribedFailedRunnerFixture : RunnerFixtureBase<CleanSubscribedFailedRunner, ICleanSubscribedFailedTask>
{
    protected override CleanSubscribedFailedRunner CreateRunner()
    {
        return new(
            _dataAccess.Object,
            FakeLog.Create<CleanSubscribedFailedRunner>(),
            _task.Object);
    }

    [TestMethod]
    public void Then_HasName()
    {
        Assert.AreEqual("CleanSubscribedFailed", _testSubject.Name);
    }
}
