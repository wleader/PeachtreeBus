using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using PeachtreeBus.Queues;
using PeachtreeBus.Subscriptions;
using PeachtreeBus.Tasks;
using System.Linq;

namespace PeachtreeBus.Core.Tests.Tasks;

[TestClass]
public class StartersFixture
{
    private Starters _starters = default!;
    private readonly Mock<IUpdateSubscriptionsStarter> _updateSubscriptions = new();
    private readonly Mock<ICleanSubscriptionsStarter> _cleanSubscriptions = new();
    private readonly Mock<ICleanSubscribedPendingStarter> _cleanSubscribedPending = new();
    private readonly Mock<ICleanSubscribedCompletedStarter> _cleanSubscribedCompleted = new();
    private readonly Mock<ICleanSubscribedFailedStarter> _cleanSubscribedFailed = new();
    private readonly Mock<ICleanQueuedCompletedStarter> _cleanQueuedCompleted = new();
    private readonly Mock<ICleanQueuedFailedStarter> _cleanQueuedFailed = new();
    private readonly Mock<IProcessSubscribedStarter> _processSubscribed = new();
    private readonly Mock<IProcessQueuedStarter> _processQueued = new();

    [TestInitialize]
    public void Initialize()
    {
        _starters = new(
            _updateSubscriptions.Object,
            _cleanSubscriptions.Object,
            _cleanSubscribedPending.Object,
            _cleanSubscribedCompleted.Object,
            _cleanSubscribedFailed.Object,
            _cleanQueuedCompleted.Object,
            _cleanQueuedFailed.Object,
            _processSubscribed.Object,
            _processQueued.Object);
    }

    [TestMethod]
    public void Given_Starters_Then_MaintenanceStartersAreCorrect()
    {
        var actual = _starters.GetMaintenanceStarters().ToArray();
        // this order is deliberate. 
        // always do update subscriptions first so that even if the other things fall behind,
        // the instance will continue to recieve subscribed messages.
        Assert.AreEqual(7, actual.Length);
        Assert.AreSame(_updateSubscriptions.Object, actual[0]);
        Assert.AreSame(_cleanSubscriptions.Object, actual[1]);
        Assert.AreSame(_cleanSubscribedPending.Object, actual[2]);
        Assert.AreSame(_cleanSubscribedCompleted.Object, actual[3]);
        Assert.AreSame(_cleanSubscribedFailed.Object, actual[4]);
        Assert.AreSame(_cleanQueuedCompleted.Object, actual[5]);
        Assert.AreSame(_cleanQueuedFailed.Object, actual[6]);
    }

    [TestMethod]
    public void Given_Starters_Then_MesagingStartersAreCorrect()
    {
        var actual = _starters.GetMessagingStarters().ToArray();
        // this order is deliberate. 
        // no one else can process our subscribed messages, so do those first
        // another copy could pick up the queued messages.
        Assert.AreEqual(2, actual.Length);
        Assert.AreSame(_processSubscribed.Object, actual[0]);
        Assert.AreSame(_processQueued.Object, actual[1]);
    }
}
