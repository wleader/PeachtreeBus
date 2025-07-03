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

public abstract class RunnerFixtureBase<TRunner, TTask>
    where TRunner : Runner<TTask>
    where TTask : class, IBaseTask
{
    protected TRunner _testSubject = default!;
    protected Mock<IBusDataAccess> _dataAccess = new();
    protected Mock<ILogger<TRunner>> _log = new();
    protected Mock<TTask> _task = new();

    protected CancellationTokenSource _cts = new();

    protected abstract TRunner CreateRunner();

    private readonly Queue<Func<Task<bool>>> _tasks = new();
    private bool _dataConnected = false;
    private bool _transactionStarted = false;

    [TestInitialize]
    public void Intitialize()
    {
        _log.Reset();
        _dataAccess.Reset();
        _task.Reset();

        _task.Setup(t => t.RunOne()).Returns(() =>
         _tasks.TryDequeue(out var result)
                ? result()
                : Task.FromResult(false)
        );

        _dataConnected = false;
        _transactionStarted = false;

        _dataAccess.Setup(x => x.Reconnect())
            .Callback(() =>
            {
                _dataConnected = true;
                _transactionStarted = false;
            });
        _dataAccess.Setup(x => x.BeginTransaction())
            .Callback(() =>
            {
                Assert.IsTrue(_dataConnected, "Attempt to begin transaction before connecting.");
                Assert.IsFalse(_transactionStarted, "Attempt to begin a second transaction.");
                _transactionStarted = true;
            });
        _dataAccess.Setup(x => x.CommitTransaction())
            .Callback(() =>
            {
                Assert.IsTrue(_dataConnected, "Attempt to commit while disconnected.");
                Assert.IsTrue(_transactionStarted, "Attempt to commit before starting.");
                _transactionStarted = false;
            });
        _dataAccess.Setup(x => x.RollbackTransaction())
            .Callback(() =>
            {
                Assert.IsTrue(_dataConnected, "Attempt to rollback while disconnected.");
                Assert.IsTrue(_transactionStarted, "Attempt to rollback before starting.");
                _transactionStarted = false;
            });

        _testSubject = CreateRunner();
    }

    protected async Task CancelToken()
    {
        _cts.Cancel();
        await Task.Delay(5);
    }

    protected Task When_Run()
    {
        _cts = new();
        var t = Task.Run(() => _testSubject.RunRepeatedly(_cts.Token));
        return t;
    }

    protected async Task Then_TaskIsComplete(Task t)
    {
        await t;
        Assert.IsTrue(t.IsCompleted);
    }

    private Task<bool> RunForever()
    {
        _tasks.Enqueue(RunForever);
        return Task.FromResult(true);
    }

    private Task<bool> RunNTimes(int count)
    {
        _tasks.Enqueue(() => RunNTimes(count - 1));
        return Task.FromResult(count > 0);
    }

    private void Given_Tasks(params Func<Task<bool>>[] tasks)
    {
        foreach (var t in tasks) _tasks.Enqueue(t);
    }

    [TestMethod]
    public async Task Given_Running_When_TokenCancelled_Then_RunnerCompletes()
    {
        Given_Tasks(RunForever);
        var t = When_Run();
        Assert.IsFalse(t.IsCompleted);
        await Task.Delay(10);
        await CancelToken();
        await Then_TaskIsComplete(t);
    }

    [TestMethod]
    [DataRow(1)]
    [DataRow(2)]
    public async Task Given_WillRunNTimes_When_Run_Then_TransactionAreUsed(int runcount)
    {
        Given_Tasks(() => RunNTimes(runcount));
        await When_Run();
        // + 1 beacause it will begin a third time, but rollback 
        _dataAccess.Verify(d => d.Reconnect(), Times.Exactly(runcount + 1));
        _dataAccess.Verify(d => d.BeginTransaction(), Times.Exactly(runcount + 1));
        _task.Verify(t => t.RunOne(), Times.Exactly(runcount + 1));
        _dataAccess.Verify(d => d.CommitTransaction(), Times.Exactly(runcount));
        _dataAccess.Verify(d => d.RollbackTransaction(), Times.Once);
    }

    [TestMethod]
    public async Task Given_TaskThrows_When_Run_Then_Rollback()
    {
        Given_Tasks(() => throw new TestException());
        await When_Run();
        _dataAccess.Verify(d => d.Reconnect(), Times.Once);
        _dataAccess.Verify(d => d.BeginTransaction(), Times.Once);
        _task.Verify(t => t.RunOne(), Times.Once);
        _dataAccess.Verify(d => d.CommitTransaction(), Times.Never);
        _dataAccess.Verify(d => d.RollbackTransaction(), Times.Once);
    }

    [TestMethod]
    public async Task Given_TaskRunsThenThrows_When_Run_Then_Rollback()
    {
        Given_Tasks(
            () => Task.FromResult(true),
            () => throw new TestException());
        await When_Run();
        _dataAccess.Verify(d => d.Reconnect(), Times.Exactly(2));
        _dataAccess.Verify(d => d.BeginTransaction(), Times.Exactly(2));
        _task.Verify(t => t.RunOne(), Times.Exactly(2));
        _dataAccess.Verify(d => d.CommitTransaction(), Times.Once);
        _dataAccess.Verify(d => d.RollbackTransaction(), Times.Once);
    }

    [TestMethod]
    public async Task Given_ReconnectThrows_When_Run_Then_Rollback()
    {
        _dataAccess.Setup(d => d.Reconnect()).Throws(new InvalidOperationException("Unable to connect to database."));

        await When_Run();

        _dataAccess.Verify(d => d.Reconnect(), Times.Once);
        _dataAccess.Verify(d => d.BeginTransaction(), Times.Never);
        _task.Verify(t => t.RunOne(), Times.Never);
        _dataAccess.Verify(d => d.CommitTransaction(), Times.Never);
        _dataAccess.Verify(d => d.RollbackTransaction(), Times.Once);
    }

    [TestMethod]
    public async Task Given_BeginTransactionThrows_When_Run_Then_Rollback()
    {
        _dataAccess.Setup(d => d.BeginTransaction()).Throws(new InvalidOperationException("Unable to connect to database."));

        await When_Run();

        _dataAccess.Verify(d => d.Reconnect(), Times.Once);
        _dataAccess.Verify(d => d.BeginTransaction(), Times.Once);
        _task.Verify(t => t.RunOne(), Times.Never);
        _dataAccess.Verify(d => d.CommitTransaction(), Times.Never);
        _dataAccess.Verify(d => d.RollbackTransaction(), Times.Once);
    }

    [TestMethod]
    public async Task Given_CommitTransactionThrows_When_Run_Then_Rollback()
    {
        Given_Tasks(() => Task.FromResult(true));
        _dataAccess.Setup(d => d.CommitTransaction()).Throws(new InvalidOperationException("Unable to connect to database."));

        await When_Run();

        _dataAccess.Verify(d => d.Reconnect(), Times.Once);
        _dataAccess.Verify(d => d.BeginTransaction(), Times.Once);
        _task.Verify(t => t.RunOne(), Times.Once);
        _dataAccess.Verify(d => d.CommitTransaction(), Times.Once);
        _dataAccess.Verify(d => d.RollbackTransaction(), Times.Once);
    }
}
