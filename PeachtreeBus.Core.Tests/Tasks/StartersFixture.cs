using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using PeachtreeBus.Queues;
using PeachtreeBus.Subscriptions;
using PeachtreeBus.Tasks;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace PeachtreeBus.Core.Tests.Tasks;

[TestClass]
public class StartersFixture
{
    private Starters _starters = default!;
    private CancellationTokenSource _cts = default!;
    private readonly Mock<IUpdateSubscriptionsStarter> _updateSubscriptions = new();
    private readonly Mock<ICleanSubscriptionsStarter> _cleanSubscriptions = new();
    private readonly Mock<ICleanSubscribedPendingStarter> _cleanSubscribedPending = new();
    private readonly Mock<ICleanSubscribedCompletedStarter> _cleanSubscribedCompleted = new();
    private readonly Mock<ICleanSubscribedFailedStarter> _cleanSubscribedFailed = new();
    private readonly Mock<ICleanQueuedCompletedStarter> _cleanQueuedCompleted = new();
    private readonly Mock<ICleanQueuedFailedStarter> _cleanQueuedFailed = new();
    private readonly Mock<IProcessSubscribedStarter> _processSubscribed = new();
    private readonly Mock<IProcessQueuedStarter> _processQueued = new();

    private bool _subscriptionsUpdated = false;

    [TestInitialize]
    public void Initialize()
    {
        _cts = new();

        _updateSubscriptions.Setup(s => s.Start(It.IsAny<Action<Task>>(), It.IsAny<CancellationToken>()))
            .Callback((Action<Task> continuteWith, CancellationToken token) =>
            {
                Assert.IsFalse(_subscriptionsUpdated);
                Assert.AreEqual(_cts.Token, token);
                Assert.AreEqual(ContinueWith, continuteWith);
                _subscriptionsUpdated = true;
            });

        SetupBeforeAfter(_updateSubscriptions, _cleanSubscriptions);
        SetupBeforeAfter(_cleanSubscriptions, _cleanSubscribedPending);
        SetupBeforeAfter(_cleanSubscribedPending, _cleanSubscribedCompleted);
        SetupBeforeAfter(_cleanSubscribedCompleted, _cleanSubscribedFailed);
        SetupBeforeAfter(_cleanSubscribedFailed, _cleanQueuedCompleted);
        SetupBeforeAfter(_cleanQueuedCompleted, _cleanQueuedFailed);
        SetupBeforeAfter(_cleanQueuedFailed, _processSubscribed);
        SetupBeforeAfter(_processSubscribed, _processQueued);

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

    private void SetupBeforeAfter<TBefore, TAfter>(Mock<TAfter> before, Mock<TBefore> after)
        where TBefore : class, IStarter
        where TAfter : class, IStarter
    {
        after.Setup(s => s.Start(It.IsAny<Action<Task>>(), It.IsAny<CancellationToken>()))
            .Callback((Action<Task> continuteWith, CancellationToken token) =>
            {
                Assert.IsTrue(_subscriptionsUpdated);
                Assert.AreEqual(_cts.Token, token);
                Assert.AreEqual(ContinueWith, continuteWith);

                continuteWith(Task.CompletedTask); // This just causes the contine to be covered.

                Assert.AreEqual(1, before.Invocations.Count,
                    $"{after.GetType()} should come after {before.GetType()}");
            });
    }

    private void ContinueWith(Task task) { }

    [TestMethod]
    public async Task Given_Starters_WhenRun_Then_InvokeOrderIsCorrect()
    {
        // the setup Before Afters will ensure that they are in the correct order.
        await _starters.RunStarters(ContinueWith, _cts.Token);
    }
}
