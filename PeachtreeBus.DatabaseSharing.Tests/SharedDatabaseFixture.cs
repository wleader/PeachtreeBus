using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Data;

namespace PeachtreeBus.DatabaseSharing.Tests;

[TestClass]
public class SharedDatabaseFixture : SharedDatabaseFixtureBase
{
    [TestInitialize]
    public override void Initialize()
    {
        base.Initialize();
    }

    [TestCleanup]
    public override void Cleanup()
    {
        base.Cleanup();
    }

    [TestMethod]
    public void When_New_Then_HasInstanceId()
    {
        Assert.AreNotEqual(Guid.Empty, _db.InstanceId);
    }

    #region When_Dispose

    [TestMethod]
    [DataRow(true)]
    [DataRow(false)]
    public void Given_DenyDispose_When_Dispose_Then_Disposed(bool deny)
    {
        _db.Reconnect();
        _db.DenyDispose = deny;
        _db.Dispose();
        Assert.AreEqual(!deny, _lastConnection.Disposed);
    }

    [TestMethod]
    public void Given_ATransactionIsStarted_When_Dispose_Then_TransactionIsDisposed()
    {
        _db.BeginTransaction();
        Assert.IsNotNull(_lastConnection.LastTransaction);
        _db.DenyDispose = false;
        _db.Dispose();
        Assert.IsTrue(_lastConnection.LastTransaction.Disposed);
        Assert.IsTrue(_transactionConsumed);
    }

    [TestMethod]
    public void Given_AConnectionIsMade_When_Dispose_Then_ConnectionIsDisposed()
    {
        _db.Reconnect();
        Assert.IsNotNull(_lastConnection);
        _db.DenyDispose = false;
        _db.Dispose();
        Assert.IsTrue(_lastConnection.Disposed);
    }

    #endregion

    #region When_RollbackTransaction

    [TestMethod]
    public void Given_ThereIsNoTransaction_When_RollbackTransaction_Then_Throws()
    {
        Assert.IsNull(_db.Transaction);
        Assert.ThrowsException<SharedDatabaseException>(_db.RollbackTransaction);
    }

    [TestMethod]
    public void Given_ATransactionIsStarted_When_RollbackTransaction_Then_RolledBack_And_TransactionIsConsumed()
    {
        _db.BeginTransaction();
        var originalTransaction = _lastConnection.LastTransaction;
        Assert.IsNotNull(originalTransaction);
        _db.RollbackTransaction();
        Assert.IsTrue(originalTransaction.RolledBack);
        Assert.IsTrue(_transactionConsumed);
    }

    #endregion

    #region When_Reconnect

    [TestMethod]
    public void Given_ATransactionIsStarted_When_Reconnect_Then_TransactionIsConsumed_And_TransactionIsDiposed()
    {
        _db.BeginTransaction();
        Assert.IsNotNull(_lastConnection.LastTransaction);
        var originalTransaction = _lastConnection.LastTransaction;
        _db.Reconnect();
        Assert.IsTrue(originalTransaction.Disposed);
        Assert.IsTrue(_transactionConsumed);
    }

    [TestMethod]
    public void Given_AConnectionIsMade_When_Reconnect_Then_ConnectionIsClosed_And_ConnectionIsDisposed_And_ANewConnectionIsMade()
    {
        _db.Reconnect();
        var originalConnection = _lastConnection;
        Assert.IsNotNull(originalConnection);
        _db.Reconnect();
        Assert.IsTrue(originalConnection.Disposed);
        Assert.AreNotSame(originalConnection, _lastConnection);
    }

    [TestMethod]
    public void Given_AConnectionIsNotMade_When_Reconnect_Then_ANewConnectionIsMade()
    {
        _db.Reconnect();
        Assert.IsNotNull(_lastConnection);
        Assert.IsNotNull(_db.Connection);
        Assert.AreSame(_lastConnection.Connection, _db.Connection);
    }

    #endregion

    #region When_RollbackToSavepoint

    [TestMethod]
    public void Given_ThereIsNoTransaction_When_RollbackToSavepoint_Then_Throws()
    {
        _db.Reconnect();
        Assert.IsNull(_db.Transaction);
        Assert.ThrowsException<SharedDatabaseException>(() =>
            _db.RollbackToSavepoint("Savepoint1"));
    }

    [TestMethod]
    public void Given_ThereIsASavepoint_When_RollbackToSavepoint_Then_Rollback()
    {
        _db.BeginTransaction();
        Assert.IsNotNull(_lastConnection.LastTransaction);
        const string SavepointName = "Savepoint1";
        _db.CreateSavepoint(SavepointName);
        _db.RollbackToSavepoint(SavepointName);
        Assert.AreEqual(SavepointName, _lastConnection.LastTransaction.LastRollbackName);
    }

    #endregion

    #region When_CommitTransaction

    [TestMethod]
    public void Given_ThereIsNoTransaction_When_CommitTransaction_ThenThrows()
    {
        _db.Reconnect();
        Assert.IsNull(_lastConnection.LastTransaction);
        Assert.ThrowsException<SharedDatabaseException>(_db.CommitTransaction);
    }

    [TestMethod]
    public void Given_ATransactionIsStarted_When_CommitTransaction_Then_TransactionIsCommitted()
    {
        _db.BeginTransaction();
        var originalTransaction = _lastConnection.LastTransaction;
        Assert.IsNotNull(originalTransaction);
        Assert.IsFalse(originalTransaction.Committed);
        _db.CommitTransaction();
        Assert.IsTrue(originalTransaction.Committed);
    }

    [TestMethod]
    public void Given_ATransactionIsStarted_When_CommitTransaction_Then_TransactionIsConsumed()
    {
        _db.BeginTransaction();
        _db.CommitTransaction();
        Assert.IsTrue(_transactionConsumed);
    }

    #endregion

    #region When_BeginTransaction

    [TestMethod]
    public void When_BeginTransaction_Then_TransactionStartedEvent()
    {
        _db.BeginTransaction();
        Assert.IsTrue(_transactionStarted);
    }

    [TestMethod]
    public void Given_ConnectionIsNotMade_When_BeginTransaction_Then_ConnectionIsOpened_And_TransactionIsCreated_And_TransactionStartedEvent()
    {
        _db.Reconnect();
        _lastConnection.Close();
        _db.BeginTransaction();
        Assert.AreEqual(ConnectionState.Open, _lastConnection.State);
        Assert.IsTrue(_transactionStarted);
        Assert.IsNotNull(_db.Transaction);
    }

    [TestMethod]
    public void Given_ConnectionIsMade_When_BeginTransaction_Then_ConnectionIsOpened_And_TransactionIsCreated_And_TransactionStartedEvent()
    {
        _db.Reconnect();
        Assert.AreEqual(ConnectionState.Open, _lastConnection.State);
        Assert.IsNull(_lastConnection.LastTransaction);
        _db.BeginTransaction();
        Assert.IsTrue(_transactionStarted);
        Assert.IsNotNull(_db.Transaction);
    }

    [TestMethod]
    public void Given_ATransactionIsStarted_When_BeginTransaction_Then_Throws()
    {
        _db.BeginTransaction();
        Assert.ThrowsException<SharedDatabaseException>(_db.BeginTransaction);
    }

    #endregion

    #region When_CreateSavepoint

    [TestMethod]
    public void Given_ATransactionIsStarted_When_CreateSavepoint_Then_SavepointCreated()
    {
        _db.BeginTransaction();
        Assert.IsNotNull(_lastConnection?.LastTransaction);
        _db.CreateSavepoint("Savepoint1");
        Assert.AreEqual("Savepoint1", _lastConnection.LastTransaction.LastSaveName);
    }

    [TestMethod]
    public void Given_ThereIsNoTransaction_When_CreateSavepoint_Then_Throws()
    {
        _db.Reconnect();
        Assert.IsNull(_lastConnection.LastTransaction);
        Assert.ThrowsException<SharedDatabaseException>(() => _db.CreateSavepoint("Savepoint1"));
        Assert.IsNull(_lastConnection.LastTransaction);
    }

    #endregion
}