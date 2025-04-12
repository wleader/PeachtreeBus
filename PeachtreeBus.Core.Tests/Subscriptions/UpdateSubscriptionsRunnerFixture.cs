using Microsoft.VisualStudio.TestTools.UnitTesting;
using PeachtreeBus.Core.Tests.Tasks;
using PeachtreeBus.Subscriptions;

namespace PeachtreeBus.Core.Tests.Subscriptions;

[TestClass]
public class UpdateSubscriptionsRunnerFixture() : RunnerFixtureBase<UpdateSubscriptionsRunner, IUpdateSubscriptionsTask>
{
    protected override UpdateSubscriptionsRunner CreateRunner()
    {
        return new(_dataAccess.Object, _log.Object, _task.Object);
    }

    [TestMethod]
    public void Then_HasName()
    {
        Assert.AreEqual("SubscriptionUpdate", _testSubject.Name);
    }
}
