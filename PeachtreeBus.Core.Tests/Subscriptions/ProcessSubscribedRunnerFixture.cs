using Microsoft.VisualStudio.TestTools.UnitTesting;
using PeachtreeBus.Core.Tests.Fakes;
using PeachtreeBus.Core.Tests.Tasks;
using PeachtreeBus.Subscriptions;

namespace PeachtreeBus.Core.Tests.Subscriptions;

[TestClass]
public class ProcessSubscribedRunnerFixture : RunnerFixtureBase<ProcessSubscribedRunner, IProcessSubscribedTask>
{
    protected override ProcessSubscribedRunner CreateRunner()
    {
        return new(
            _dataAccess.Object,
            FakeLog.Create<ProcessSubscribedRunner>(),
            _task.Object);
    }

    [TestMethod]
    public void Then_HasName()
    {
        Assert.AreEqual("ProcessSubscribed", _testSubject.Name);
    }
}
