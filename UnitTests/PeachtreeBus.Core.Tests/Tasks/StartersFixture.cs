using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using PeachtreeBus.Queues;
using PeachtreeBus.Subscriptions;
using PeachtreeBus.Tasks;
using PeachtreeBus.Testing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace PeachtreeBus.Core.Tests.Tasks;

[TestClass]
public class StartersFixture
{
    private Starters _starters = default!;
    private CancellationTokenSource _cts = default!;

    private readonly Dictionary<Type, object> _mocks = [];
    private readonly Dictionary<Type, List<Task>> _startResults = [];
    private readonly List<Type> _invocationOrder = [];

    private static readonly List<Type> _expectedInvocationOrder =
    [
        // always update subscriptions. If this is not done often enough,
        // the subscription could expire, and a message for a topic may not be delivered.
        typeof(IUpdateSubscriptionsStarter),

        // cleanup before processing. This way
        // the cleanup tasks should never get out of hand.
        typeof(ICleanSubscriptionsStarter),
        typeof(ICleanSubscribedPendingStarter),
        typeof(ICleanSubscribedCompletedStarter),
        typeof(ICleanSubscribedFailedStarter),
        typeof(ICleanQueuedCompletedStarter),
        typeof(ICleanQueuedFailedStarter),

        // process subscribed messages first because this subscriber is the only process
        // that can process its messages.
        typeof(IProcessSubscribedStarter),

        // processes queued is lowest priority as this task is shared among each copy of the endpoint.
        typeof(IProcessQueuedStarter),
];

    [TestInitialize]
    public void Initialize()
    {
        _startResults.Clear();
        _invocationOrder.Clear();
        _mocks.Clear();

        _cts = new();

        _starters = new(
            SetupMock<IUpdateSubscriptionsStarter>().Object,
            SetupMock<ICleanSubscriptionsStarter>().Object,
            SetupMock<ICleanSubscribedPendingStarter>().Object,
            SetupMock<ICleanSubscribedCompletedStarter>().Object,
            SetupMock<ICleanSubscribedFailedStarter>().Object,
            SetupMock<ICleanQueuedCompletedStarter>().Object,
            SetupMock<ICleanQueuedFailedStarter>().Object,
            SetupMock<IProcessSubscribedStarter>().Object,
            SetupMock<IProcessQueuedStarter>().Object);
    }

    private void AssertStartParameters<T>(Action<Task> continueWith, CancellationToken token)
    {
        Assert.AreEqual(_cts.Token, token, $"The cancellation token was not passed to the {typeof(T)}.");
        Assert.AreEqual(ContinueWith, continueWith, $"The continuation was not passed to the {typeof(T)}.");
        _invocationOrder.Add(typeof(T));
    }

    private List<Task> GetResult<T>()
    {
        return _startResults.TryGetValue(typeof(T), out var result) ? result : [];
    }

    private Mock<T> SetupMock<T>() where T : class, IStarter
    {
        var result = new Mock<T>();
        result.Setup(t => t.Start(It.IsAny<Action<Task>>(), It.IsAny<CancellationToken>()))
            .Callback(AssertStartParameters<T>)
            .ReturnsAsync(GetResult<T>);
        _mocks.Add(typeof(T), result);
        return result;
    }

    private void ContinueWith(Task task) { }

    public static IEnumerable<object[]> GetStarterTypes =>
        _expectedInvocationOrder.Select(x => new object[] { x });

    private async Task RunMethodOnMock(Type type, string methodName)
    {
        var o = _mocks.TryGetValue(type, out var match) ? match : null;
        Assert.IsNotNull(o);
        var method = this.GetType().GetMethod(methodName,
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        Assert.IsNotNull(method);
        var genericMethod = method.MakeGenericMethod(type);
        var t = genericMethod.Invoke(this, [o]) as Task;
        Assert.IsNotNull(t);
        await t;
    }

    [TestMethod]
    public async Task Given_Starters_When_RunStarters_Then_InvokeOrderIsCorrect()
    {
        await _starters.RunStarters(ContinueWith, _cts.Token);
        CollectionAssert.AreEqual(_expectedInvocationOrder, _invocationOrder);
    }

    [TestMethod]
    [DynamicData("GetStarterTypes")]
    public async Task Given_StarterWillThrow_When_RunStarters_Then_Throws(Type type)
    {
        await RunMethodOnMock(type, nameof(Given_MockWillThrow_When_RunStarters_Then_Throws));
    }

    private async Task Given_MockWillThrow_When_RunStarters_Then_Throws<T>(Mock<T> mock)
        where T : class, IStarter
    {
        var ex = new TestException();
        mock.Setup(m => m.Start(It.IsAny<Action<Task>>(), It.IsAny<CancellationToken>()))
            .Throws(ex);
        var thrown = await Assert.ThrowsExactlyAsync<TestException>(() =>
            _starters.RunStarters(ContinueWith, _cts.Token));
        Assert.AreSame(ex, thrown);
    }

    [TestMethod]
    [DynamicData("GetStarterTypes")]
    public async Task Given_StarterReturnsTasks_When_RunStarters_Then_ResultContainsTasks(Type type)
    {
        await RunMethodOnMock(type, nameof(Given_MockReturns_When_RunStarters_Then_ResultContainsTasks));
    }

    private async Task Given_MockReturns_When_RunStarters_Then_ResultContainsTasks<T>(Mock<T> mock)
    where T : class, IStarter
    {
        var t1 = Task.Delay(1);
        var t2 = Task.Delay(2);

        mock.Setup(m => m.Start(It.IsAny<Action<Task>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync([t1, t2]);
        var actual = await _starters.RunStarters(ContinueWith, _cts.Token);

        CollectionAssert.Contains(actual, t1);
        CollectionAssert.Contains(actual, t2);

        // not needed for the test
        // just good practice to await any task that is started.
        await t1;
        await t2;
    }

}
