using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using PeachtreeBus.Tasks;
using PeachtreeBus.Testing;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace PeachtreeBus.Core.Tests.Tasks;

public abstract class StarterFixtureBase<TStarter, TRunner, TTracker, TEstimator>
    where TStarter : Starter<TRunner>
    where TRunner : class, IRunner
    where TTracker : class, ITracker
    where TEstimator : class, IEstimator
{
    protected TStarter _starter = default!;
    protected Mock<IScopeFactory> _scopeFactory = new();
    protected Mock<TTracker> _tracker = new();
    protected FakeServiceProviderAccessor _accessor = new(new());
    protected Mock<TRunner> _runner = new();
    protected Mock<ITaskCounter> _taskCounter = new();
    protected Mock<TEstimator> _estimator = new();

    protected List<Task> _runnerTasks = default!;
    protected List<Task> _continuedTasks = default!;
    protected List<Task> _actualTasks = default!;
    protected CancellationTokenSource _cts = default!;
    private bool _gotRunnerInstance;

    [TestInitialize]
    public virtual void Intialize()
    {
        _scopeFactory.Reset();
        _tracker.Reset();
        _accessor.Reset();
        _runner.Reset();
        _taskCounter.Reset();
        _estimator.Reset();

        _runnerTasks = [];
        _continuedTasks = [];
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

        _starter = CreateStarter();
    }

    public abstract TStarter CreateStarter();

    public void ContinueWith(Task task)
    {
        _continuedTasks.Add(task);
    }


    [TestMethod]
    [DataRow(0, 0, 0)]
    [DataRow(1, 1, 1)]
    [DataRow(1, 0, 0)]
    [DataRow(2, 2, 2)]
    [DataRow(2, 1, 1)]
    [DataRow(2, 0, 0)]
    public async Task Given_ShouldStart_And_Available_When_Start_Then_Result(int available, int estimate, int expectedResult)
    {
        _tracker.SetupGet(t => t.ShouldStart).Returns(true);

        await When_Run(available, estimate);

        Then_RunnersAreStarted(expectedResult);
    }

    [TestMethod]
    [DataRow(0)]
    [DataRow(1)]
    [DataRow(2)]
    public async Task Given_ShouldNotStart_And_Available_When_Start_Then_Result(int available)
    {
        _tracker.SetupGet(t => t.ShouldStart).Returns(false);
        await When_Run(available, 1);
        Then_RunnersAreStarted(0);
    }

    [TestMethod]
    public async Task Given_ZeroAvailability_When_Start_Then_EstimateNotInvoked()
    {
        await When_Run(0, 1);
        _estimator.Verify(x => x.EstimateDemand(), Times.Never);
    }

    protected async Task When_Run(int available, int estimate)
    {
        _taskCounter.Setup(x => x.Available()).Returns(available);
        _estimator.Setup(x => x.EstimateDemand()).ReturnsAsync(estimate);
        _actualTasks = await _starter.Start(ContinueWith, _cts.Token);
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
        Assert.AreEqual(runnerCount, _continuedTasks.Count);
        Assert.AreEqual(runnerCount, _actualTasks.Count);
    }
}