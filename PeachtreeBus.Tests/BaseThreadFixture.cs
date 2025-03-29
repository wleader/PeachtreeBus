using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using PeachtreeBus.Data;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace PeachtreeBus.Tests;

[TestClass]
public class BaseThreadFixture : ThreadFixtureBase<BaseThreadFixture.TestThread>
{
    public class TestThread(
        string name,
        int delayMs,
        ILogger log,
        IBusDataAccess dataAccess)
        : BaseThread(name, delayMs, log, dataAccess)
    {
        public Queue<Func<Task<bool>>> WorkQueue { get; } = new();

        public override async Task<bool> DoUnitOfWork()
        {
            if (!WorkQueue.TryDequeue(out var work)) return false;
            return await work();
        }
    }

    private readonly Mock<IBusDataAccess> dataAccess = new();
    private readonly Mock<ILogger> log = new();
    private bool dataConnected = false;
    private bool transactionStarted = false;

    [TestInitialize]
    public void TestInitialize()
    {
        log.Reset();
        dataAccess.Reset();

        dataConnected = false;
        transactionStarted = false;

        dataAccess.Setup(x => x.Reconnect())
            .Callback(() =>
            {
                dataConnected = true;
                transactionStarted = false;
            });
        dataAccess.Setup(x => x.BeginTransaction())
            .Callback(() =>
            {
                Assert.IsTrue(dataConnected, "Attempt to begin transaction before connecting.");
                Assert.IsFalse(transactionStarted, "Attempt to begin a second transaction.");
                transactionStarted = true;
            });
        dataAccess.Setup(x => x.CommitTransaction())
            .Callback(() =>
            {
                Assert.IsTrue(dataConnected, "Attempt to commit while disconnected.");
                Assert.IsTrue(transactionStarted, "Attempt to commit before starting.");
                transactionStarted = false;
            });
        dataAccess.Setup(x => x.RollbackTransaction())
            .Callback(() =>
            {
                Assert.IsTrue(dataConnected, "Attempt to rollback while disconnected.");
                Assert.IsTrue(transactionStarted, "Attempt to rollback before starting.");
                transactionStarted = false;
            });

        _testSubject = new TestThread("Test", 100, log.Object, dataAccess.Object);
    }

    [TestMethod]
    public async Task Given_Running_When_TokenCancelled_TaskCompleted()
    {
        Given_Work(() => ReturnForever(false));
        var t = Task.Run(() => _testSubject.Run(_cts.Token));
        await Task.Delay(10);
        Assert.IsFalse(t.IsCompleted);

        _cts.Cancel();

        await Task.Delay(10);
        Assert.IsTrue(t.IsCompleted);
        await t;
    }

    [TestMethod]
    public async Task Given_Work_When_Run_Then_TransactionIsStartedForWork()
    {
        Given_Work(
            () => CancelAndReturn(true));
        await When_Run();
        dataAccess.Verify(d => d.BeginTransaction(), Times.Once);

        Given_Work(
            () => Task.FromResult(true),
            () => CancelAndReturn(true));
        await When_Run();
        dataAccess.Verify(d => d.BeginTransaction(), Times.Exactly(2));
    }

    [TestMethod]
    public async Task Given_WorkReturnsTrue_When_Run_Then_TransactionIsCommitted()
    {
        // this test could be improved by having checking that begin transaction
        // was done first.
        Given_Work(
            () => CancelAndReturn(true));
        await When_Run();
        dataAccess.Verify(d => d.CommitTransaction(), Times.Once);

        Given_Work(
            () => Task.FromResult(true),
            () => CancelAndReturn(true));
        await When_Run();
        dataAccess.Verify(d => d.CommitTransaction(), Times.Exactly(2));
    }

    [TestMethod]
    public async Task Given_WorkReturnsFalse_When_Run_Then_TransactionRollback()
    {
        // this could be improved by checking the transaction was started before
        // rollback.

        Given_Work(
            () => CancelAndReturn(false));
        await When_Run();
        dataAccess.Verify(d => d.RollbackTransaction(), Times.Once);

        Given_Work(
            () => Task.FromResult(false),
            () => CancelAndReturn(false));
        await When_Run();
        dataAccess.Verify(d => d.RollbackTransaction(), Times.Exactly(2));
    }

    [TestMethod]
    public async Task Given_WorkThrows_When_Run_Then_TransactionRollback()
    {
        Given_Work(
            () => CancelAndThrow());
        await When_Run();
        dataAccess.Verify(d => d.RollbackTransaction(), Times.Once);

        Given_Work(
            () => Throw(),
            () => CancelAndThrow());
        await When_Run();
        dataAccess.Verify(d => d.RollbackTransaction(), Times.Exactly(2));
    }

    [TestMethod]
    public async Task Given_WorkThrows_And_RollbackThrows_When_Run_Then_Reconnect()
    {
        dataAccess.Setup(d => d.RollbackTransaction()).Throws(() =>
        {
            dataConnected = false;
            return new InvalidOperationException("This SqlTransaction has completed; it is no longer usable.");
        });

        Given_Work(
            () => CancelAndThrow());
        await When_Run();
        dataAccess.Verify(d => d.Reconnect(), Times.Exactly(1));

        Given_Work(
            () => Throw(),
            () => CancelAndThrow());
        await When_Run();
        dataAccess.Verify(d => d.Reconnect(), Times.Exactly(2));
    }

    [TestMethod]
    public async Task Given_ConnectThrows_When_Run_Then_NoBeginTransaction_And_Rollback()
    {
        Given_Work(
            () => Throw());

        dataAccess.Setup(d => d.RollbackTransaction()).Throws(() =>
        {
            dataConnected = false;
            return new InvalidOperationException("This SqlTransaction has completed; it is no longer usable.");
        });
        dataAccess.Setup(d => d.Reconnect()).Throws(new InvalidOperationException("Unable to connect to database."));

        int counter = 0;
        dataAccess.Setup(d => d.Reconnect()).Callback(() =>
        {
            counter++;
            if (counter > 1)
                _cts.Cancel();
        })
        .Throws(new InvalidOperationException("Unable to connect to database."));

        await When_Run();

        dataAccess.Verify(d => d.Reconnect(), Times.Exactly(2));
        dataAccess.Verify(d => d.BeginTransaction(), Times.Never);
        dataAccess.Verify(d => d.RollbackTransaction(), Times.Exactly(2));
    }

    private Task<bool> ReturnForever(bool value)
    {
        _testSubject.WorkQueue.Enqueue(() => ReturnForever(value));
        return Task.FromResult(value);
    }

    private Task<bool> CancelAndReturn(bool value)
    {
        _cts.Cancel();
        return Task.FromResult(value);
    }

    private Task<bool> CancelAndThrow(Exception? ex = null)
    {
        _cts.Cancel();
        return Throw(ex);
    }

    private static Task<bool> Throw(Exception? ex = null) =>
        throw ex ?? new ApplicationException("Work Exception.");


    private void Given_Work(params Func<Task<bool>>[] work)
    {
        foreach (var item in work)
        {
            _testSubject.WorkQueue.Enqueue(item);
        }
    }

    protected override Task When_Run()
    {
        dataAccess.Invocations.Clear();
        return base.When_Run();
    }
}
