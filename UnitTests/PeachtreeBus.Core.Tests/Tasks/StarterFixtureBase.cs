using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using PeachtreeBus.Data;
using PeachtreeBus.Tasks;
using PeachtreeBus.Testing;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace PeachtreeBus.Core.Tests.Tasks;

public abstract class StarterFixtureBase<TStarter, TRunner, TTracker, TEstimator, TCounter>
    where TStarter : Starter<TRunner>
    where TRunner : class, IRunner
    where TTracker : class, ITracker
    where TEstimator : class, IEstimator
    where TCounter : class, ITaskCounter
{
    protected TStarter _starter = default!;
    protected Mock<ILogger<TStarter>> _log = new();
    protected Mock<IScopeFactory> _scopeFactory = new();
    protected Mock<TTracker> _tracker = new();
    protected FakeServiceProviderAccessor _accessor = new(new());
    protected Mock<TRunner> _runner = new();
    protected Mock<TCounter> _taskCounter = new();
    protected Mock<TEstimator> _estimator = new();
    protected Mock<IBusDataAccess> _dataAccess = new();
    protected Mock<ICurrentTasks> _tasks = new();

    protected List<Task> _runnerTasks = default!;
    protected int _actualStartCount;
    protected CancellationTokenSource _cts = default!;
    private bool _gotRunnerInstance;

    private int _available;
    private int _estimate;
    private bool _shouldStart;

    [TestInitialize]
    public virtual void Intialize()
    {
        _log.Reset();
        _scopeFactory.Reset();
        _tracker.Reset();
        _accessor.Reset();
        _runner.Reset();
        _taskCounter.Reset();
        _estimator.Reset();
        _dataAccess.Reset();
        _tasks.Reset();

        _runnerTasks = [];
        _cts = new();
        _gotRunnerInstance = false;

        _scopeFactory.Setup(x => x.Create())
            .Returns(() => _accessor.Object);

        _accessor.Add(() => _runner.Object, () => _gotRunnerInstance = true);

        _accessor.Mock.Setup(x => x.Dispose())
            .Callback(() => Assert.IsTrue(_gotRunnerInstance));

        _runner.Setup(r => r.RunRepeatedly(It.IsAny<CancellationToken>()))
            .Returns(() =>
            {
                var result = Task.FromResult(Guid.NewGuid());
                _runnerTasks.Add(result);
                return result;
            });

        _taskCounter.Setup(x => x.Available())
            .Returns(() => _available);

        _estimator.Setup(x => x.EstimateDemand())
            .ReturnsAsync(() => _estimate);

        _shouldStart = true;
        _tracker.SetupGet(x => x.ShouldStart)
            .Returns(() => _shouldStart);

        _starter = CreateStarter();
    }

    public abstract TStarter CreateStarter();

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
        _available = available;
        _estimate = estimate;
        _shouldStart = shouldStart;
        
        await When_Start();

        Then_RunnersAreStarted(expectedRunnerCount);
    }

    [TestMethod]
    public async Task Given_ZeroAvailability_When_Start_Then_EstimateNotInvoked()
    {
        _available = 0;
        _estimate = 1;
        _shouldStart = true;
        await When_Start();
        _estimator.Verify(x => x.EstimateDemand(), Times.Never);
    }

    [TestMethod]
    public async Task Given_ShouldStart_And_Available_And_Estimate_When_Start_Then_DataAccessIsResetFirst()
    {
        _available = 1;
        _estimate = 1;
        _shouldStart = true;
        bool reconnected = false;
        _dataAccess.Setup(x => x.Reconnect())
            .Callback(() => reconnected = true);
        _tracker.SetupGet(x => x.ShouldStart)
            .Callback(() => Assert.IsTrue(reconnected))
            .Returns(true);
        _taskCounter.Setup(x => x.Available())
            .Callback(() => Assert.IsTrue(reconnected))
            .Returns(1);
        _estimator.Setup(x => x.EstimateDemand())
            .Callback(() => Assert.IsTrue(reconnected))
            .ReturnsAsync(1);

        await When_Start();

        Assert.IsTrue(reconnected);
    }

    [TestMethod]
    public async Task Given_DataAccessThrows_When_Start_Then_ResultIsEmpty()
    {
        _available = 1;
        _estimate = 1;
        _shouldStart = true; 
        _dataAccess.Setup(x => x.Reconnect()).Throws<TestException>();
        await When_Start();
        Assert.AreEqual(0, _actualStartCount);
    }

    [TestMethod]
    public async Task Given_TrackerThrows_When_Start_Then_ResultIsEmpty()
    {
        _available = 1;
        _estimate = 1;
        _shouldStart = true;
        _tracker.SetupGet(x => x.ShouldStart).Throws<TestException>();
        await When_Start();
        Assert.AreEqual(0, _actualStartCount);
    }

    [TestMethod]
    public async Task Given_CounterThrows_When_Start_Then_ResultIsEmpty()
    {
        _available = 1;
        _estimate = 1;
        _shouldStart = true;
        _taskCounter.Setup(x => x.Available()).Throws<TestException>();
        await When_Start();
        Assert.AreEqual(0, _actualStartCount);
    }

    [TestMethod]
    public async Task Given_EstimatorThrows_When_Start_Then_ResultIsEmpty()
    {
        _available = 1;
        _estimate = 1;
        _shouldStart = true;
        _estimator.Setup(x => x.EstimateDemand()).Throws<TestException>();
        await When_Start();
        Assert.AreEqual(0, _actualStartCount);
    }

    protected async Task When_Start()
    {
        _actualStartCount = await _starter.Start(_cts.Token);
        await Task.Delay(10); // give time for continuations
        Task.WaitAll([.. _runnerTasks]);
    }

    protected void Then_RunnersAreStarted(int runnerCount)
    {
        _tracker.Verify(t => t.Start(), Times.Exactly(runnerCount));
        _tracker.Verify(t => t.WorkDone(), Times.Exactly(runnerCount));
        _scopeFactory.Verify(f => f.Create(), Times.Exactly(runnerCount));
        _accessor.VerifyGetService<TRunner>(runnerCount);
        _accessor.Mock.Verify(s => s.Dispose(), Times.Exactly(runnerCount));
        _runner.Verify(r => r.RunRepeatedly(_cts.Token), Times.Exactly(runnerCount));
        _taskCounter.Verify(c => c.Increment(), Times.Exactly(runnerCount));
        _taskCounter.Verify(c => c.Decrement(), Times.Exactly(runnerCount));
        Assert.AreEqual(runnerCount, _actualStartCount);
        _tasks.Verify(t => t.Add(It.IsAny<Task>()), Times.Exactly(runnerCount));
    }
}