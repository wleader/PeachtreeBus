using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using PeachtreeBus.Data;
using PeachtreeBus.Tasks;
using PeachtreeBus.Testing;
using System;
using System.Threading;
using System.Threading.Tasks;
using PeachtreeBus.Core.Tests.Fakes;

namespace PeachtreeBus.Core.Tests.Tasks;

public abstract class StarterFixtureBase<TStarter, TRunner, TTracker, TEstimator>
    where TStarter : Starter<TRunner>
    where TRunner : class, IRunner
    where TTracker : class, ITracker
    where TEstimator : class, IEstimator
{
    private TStarter _starter = null!;
    protected readonly Mock<ILogger<TStarter>> Log = new();
    protected readonly Mock<IScopeFactory> ScopeFactory = new();
    protected readonly Mock<TTracker> Tracker = new();
    private readonly FakeServiceProviderAccessor _accessor = new(new());
    private readonly Mock<TRunner> _runner = new();
    protected readonly FakeTaskCounter TaskCounter = new();
    protected readonly Mock<TEstimator> Estimator = new();
    protected readonly Mock<IBusDataAccess> DataAccess = new();
    protected readonly Mock<ICurrentTasks> Tasks = new();

    private CancellationTokenSource _cts = null!;
    private bool _gotRunnerInstance;

    private int _estimate;
    private bool _shouldStart;
    private int _workDone;

    [TestInitialize]
    public virtual void Initialize()
    {
        Log.Reset();
        ScopeFactory.Reset();
        Tracker.Reset();
        _accessor.Reset();
        _runner.Reset();
        TaskCounter.Reset();
        Estimator.Reset();
        DataAccess.Reset();
        Tasks.Reset();

        _cts = new();
        _gotRunnerInstance = false;

        ScopeFactory.Setup(x => x.Create())
            .Returns(() => _accessor.Object);

        _accessor.Add(() => _runner.Object, () => _gotRunnerInstance = true);

        _accessor.Mock.Setup(x => x.Dispose())
            .Callback(() => Assert.IsTrue(_gotRunnerInstance));

        _runner.Setup(r => r.RunRepeatedly(It.IsAny<CancellationToken>()))
            .Returns(() => Task.FromResult(Guid.NewGuid()));

        Estimator.Setup(x => x.EstimateDemand())
            .ReturnsAsync(() => _estimate);

        _shouldStart = true;
        _workDone = 0;
        Tracker.SetupGet(x => x.ShouldStart)
            .Returns(() => _shouldStart);
        Tracker.Setup(x => x.WorkDone())
            .Callback(() => _workDone++);

        _starter = CreateStarter();
    }

    protected abstract TStarter CreateStarter();

    [TestMethod]
    [DataRow(true, 0, 0, 0)]
    [DataRow(true, 1, 1, 1)]
    [DataRow(true, 1, 0, 0)]
    [DataRow(true, 2, 2, 2)]
    [DataRow(true, 2, 1, 1)]
    [DataRow(true, 2, 0, 0)]
    [DataRow(false, 0, 0, 0)]
    [DataRow(false, 1, 1, 0)]
    [DataRow(false, 1, 0, 0)]
    [DataRow(false, 2, 2, 0)]
    [DataRow(false, 2, 1, 0)]
    [DataRow(false, 2, 0, 0)]
    public async Task Given_ShouldStart_And_Available_And_Estimate_When_Start_Then_ExpectedRunners(
        bool shouldStart,
        int available,
        int estimate,
        int expectedRunnerCount)
    {
        TaskCounter.Given_Available(available);
        TaskCounter.Given_ExpectedTaskCount(expectedRunnerCount);
        _estimate = estimate;
        _shouldStart = shouldStart;

        await When_Start();

        Then_RunnersAreStarted(expectedRunnerCount);
    }

    [TestMethod]
    public async Task Given_ZeroAvailability_When_Start_Then_EstimateNotInvoked()
    {
        TaskCounter.Given_Available(0);
        _estimate = 1;
        _shouldStart = true;
        await When_Start();
        Estimator.Verify(x => x.EstimateDemand(), Times.Never);
    }

    [TestMethod]
    public async Task Given_ShouldStart_And_Available_And_Estimate_When_Start_Then_DataAccessIsResetFirst()
    {
        TaskCounter.Given_Available(1);
        _estimate = 1;
        _shouldStart = true;
        bool reconnected = false;
        DataAccess.Setup(x => x.Reconnect())
            .Callback(() => reconnected = true);
        Tracker.SetupGet(x => x.ShouldStart)
            .Callback(() => Assert.IsTrue(reconnected))
            .Returns(true);
        TaskCounter.AvailableCallback = () => Assert.IsTrue(reconnected);
        Estimator.Setup(x => x.EstimateDemand())
            .Callback(() => Assert.IsTrue(reconnected))
            .ReturnsAsync(1);

        await When_Start();

        Assert.IsTrue(reconnected);
    }

    [TestMethod]
    public async Task Given_DataAccessThrows_When_Start_Then_ResultIsEmpty()
    {
        TaskCounter.Given_Available(1);
        _estimate = 1;
        _shouldStart = true;
        DataAccess.Setup(x => x.Reconnect()).Throws<TestException>();
        await When_Start();
        _runner.Verify(r => r.RunRepeatedly(_cts.Token), Times.Never);
    }

    [TestMethod]
    public async Task Given_TrackerThrows_When_Start_Then_ResultIsEmpty()
    {
        TaskCounter.Given_Available(1);
        _estimate = 1;
        _shouldStart = true;
        Tracker.SetupGet(x => x.ShouldStart).Throws<TestException>();
        await When_Start();
        _runner.Verify(r => r.RunRepeatedly(_cts.Token), Times.Never);
    }

    [TestMethod]
    public async Task Given_CounterThrows_When_Start_Then_ResultIsEmpty()
    {
        TaskCounter.Given_Available(1);
        _estimate = 1;
        _shouldStart = true;
        TaskCounter.Given_AvailableThrows(new TestException());
        await When_Start();
        _runner.Verify(r => r.RunRepeatedly(_cts.Token), Times.Never);
    }

    [TestMethod]
    public async Task Given_EstimatorThrows_When_Start_Then_ResultIsEmpty()
    {
        TaskCounter.Given_Available(1);
        _estimate = 1;
        _shouldStart = true;
        Estimator.Setup(x => x.EstimateDemand()).Throws<TestException>();
        await When_Start();
        _runner.Verify(r => r.RunRepeatedly(_cts.Token), Times.Never);
    }

    private Task When_Start() => _starter.Start(_cts.Token);

    private void Then_RunnersAreStarted(int runnerCount)
    {
        Tracker.Verify(t => t.Start(), Times.Exactly(runnerCount));
        SpinWait.SpinUntil(() => _workDone == runnerCount, TimeSpan.FromSeconds(1));
        Tracker.Verify(t => t.WorkDone(), Times.Exactly(runnerCount));
        ScopeFactory.Verify(f => f.Create(), Times.Exactly(runnerCount));
        _accessor.VerifyGetService<TRunner>(runnerCount);
        _accessor.Mock.Verify(s => s.Dispose(), Times.Exactly(runnerCount));
        _runner.Verify(r => r.RunRepeatedly(_cts.Token), Times.Exactly(runnerCount));
        TaskCounter.Then_TasksComplete();
        Tasks.Verify(t => t.Add(It.IsAny<Task>()), Times.Exactly(runnerCount));
    }
}