using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using PeachtreeBus.Core.Tests.Fakes;
using PeachtreeBus.Tasks;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace PeachtreeBus.Core.Tests.Tasks;

public abstract class StarterFixtureBase<TStarter, TRunner, TTracker>
    where TStarter : Starter<TRunner>
    where TRunner : class, IRunner
    where TTracker : class, ITracker
{
    protected TStarter _starter = default!;
    protected Mock<IScopeFactory> _scopeFactory = new();
    protected Mock<TTracker> _tracker = new();
    protected FakeServiceProviderAccessor _accessor = new();
    protected Mock<TRunner> _runner = new();
    protected Mock<ITaskCounter> _taskCounter = new();

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

        _runnerTasks = [];
        _continuedTasks = [];
        _cts = new();
        _gotRunnerInstance = false;

        _scopeFactory.Setup(x => x.Create())
            .Returns(() => _accessor);

        _accessor.ServiceProviderMock.Setup(x => x.GetService(typeof(TRunner)))
            .Callback(() => _gotRunnerInstance = true)
            .Returns(() => _runner.Object);

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

    public virtual int SetupEstimate(int estimate)
    {
        return Math.Min(estimate, 1);
    }

    [TestMethod]
    [DataRow(0, 0)]
    [DataRow(1, 1)]
    [DataRow(2, 2)]
    public async Task Given_ShouldStart_And_Available_When_Start_Then_Result(int available, int expectedResult)
    {
        expectedResult = SetupEstimate(expectedResult);

        _tracker.SetupGet(t => t.ShouldStart).Returns(true);

        await When_Run(available);

        Then_RunnersAreStarted(expectedResult);
    }

    [TestMethod]
    [DataRow(0)]
    [DataRow(1)]
    [DataRow(2)]
    public async Task Given_ShouldNotStart_And_Available_When_Start_Then_Result(int available)
    {
        _tracker.SetupGet(t => t.ShouldStart).Returns(false);
        await When_Run(available);
        Then_RunnersAreStarted(0);
    }

    protected async Task When_Run(int available)
    {
        _taskCounter.Setup(x => x.Available()).Returns(available);
        _actualTasks = await _starter.Start(ContinueWith, _cts.Token);
        await Task.Delay(10); // give time for continuations
        Task.WaitAll([.. _runnerTasks]);
    }

    protected void Then_RunnersAreStarted(int runnerCount)
    {
        _tracker.Verify(t => t.Start(), Times.Exactly(runnerCount));
        _tracker.Verify(t => t.WorkDone(), Times.Exactly(runnerCount));
        _scopeFactory.Verify(f => f.Create(), Times.Exactly(runnerCount));
        _accessor.ServiceProviderMock.Verify(s => s.GetService(typeof(TRunner)), Times.Exactly(runnerCount));
        _accessor.Mock.Verify(s => s.Dispose(), Times.Exactly(runnerCount));
        _runner.Verify(r => r.RunRepeatedly(_cts.Token), Times.Exactly(runnerCount));
        _taskCounter.Verify(c => c.Increment(), Times.Exactly(runnerCount));
        _taskCounter.Verify(c => c.Decrement(), Times.Exactly(runnerCount));
        Assert.AreEqual(runnerCount, _continuedTasks.Count);
        Assert.AreEqual(runnerCount, _actualTasks.Count);
    }
}