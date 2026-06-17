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
        Assert.AreNotEqual(Guid.Empty, Db.InstanceId);
    }

    #region When_Dispose

    [TestMethod]
    [DataRow(true)]
    [DataRow(false)]
    public void Given_DenyDispose_When_Dispose_Then_Disposed(bool deny)
    {
        Db.Reconnect();
        Db.DenyDispose = deny;
        Db.Dispose();
        Assert.AreEqual(!deny, LastConnection.Disposed);
    }

    [TestMethod]
    public void Given_ATransactionIsStarted_When_Dispose_Then_TransactionIsDisposed()
    {
        Db.BeginTransaction();
        Assert.IsNotNull(LastConnection.LastTransaction);
        Db.DenyDispose = false;
        Db.Dispose();
        Assert.IsTrue(LastConnection.LastTransaction.Disposed);
        Assert.IsTrue(TransactionConsumed);
    }

    [TestMethod]
    public void Given_AConnectionIsMade_When_Dispose_Then_ConnectionIsDisposed()
    {
        Db.Reconnect();
        Assert.IsNotNull(LastConnection);
        Db.DenyDispose = false;
        Db.Dispose();
        Assert.IsTrue(LastConnection.Disposed);
    }

    #endregion

    #region When_RollbackTransaction

    [TestMethod]
    public void Given_ThereIsNoTransaction_When_RollbackTransaction_Then_Throws()
    {
        Assert.IsNull(Db.Transaction);
        Assert.ThrowsExactly<SharedDatabaseException>(Db.RollbackTransaction);
    }

    [TestMethod]
    public void Given_ATransactionIsStarted_When_RollbackTransaction_Then_RolledBack_And_TransactionIsConsumed()
    {
        Db.BeginTransaction();
        var originalTransaction = LastConnection.LastTransaction;
        Assert.IsNotNull(originalTransaction);
        Db.RollbackTransaction();
        Assert.IsTrue(originalTransaction.RolledBack);
        Assert.IsTrue(TransactionConsumed);
    }

    #endregion

    #region When_Reconnect

    [TestMethod]
    public void Given_ATransactionIsStarted_When_Reconnect_Then_TransactionIsConsumed_And_TransactionIsDisposed()
    {
        Db.BeginTransaction();
        Assert.IsNotNull(LastConnection.LastTransaction);
        var originalTransaction = LastConnection.LastTransaction;
        Db.Reconnect();
        Assert.IsTrue(originalTransaction.Disposed);
        Assert.IsTrue(TransactionConsumed);
    }

    [TestMethod]
    public void Given_AConnectionIsMade_When_Reconnect_Then_ConnectionIsClosed_And_ConnectionIsDisposed_And_ANewConnectionIsMade()
    {
        Db.Reconnect();
        var originalConnection = LastConnection;
        Assert.IsNotNull(originalConnection);
        Db.Reconnect();
        Assert.IsTrue(originalConnection.Disposed);
        Assert.AreNotSame(originalConnection, LastConnection);
    }

    [TestMethod]
    public void Given_AConnectionIsNotMade_When_Reconnect_Then_ANewConnectionIsMade()
    {
        Db.Reconnect();
        Assert.IsNotNull(LastConnection);
        Assert.IsNotNull(Db.Connection);
        Assert.AreSame(LastConnection.Connection, Db.Connection);
    }

    [TestMethod]
    public void Given_DenyDispose_When_Reconnection_Then_Throws()
    {
        Db.DenyDispose = true;
        Assert.ThrowsExactly<SharedDatabaseException>(Db.Reconnect);
    }

    #endregion

    #region When_RollbackToSavepoint

    [TestMethod]
    public void Given_ThereIsNoTransaction_When_RollbackToSavepoint_Then_Throws()
    {
        Db.Reconnect();
        Assert.IsNull(Db.Transaction);
        Assert.ThrowsExactly<SharedDatabaseException>(() =>
            Db.RollbackToSavepoint("Savepoint1"));
    }

    [TestMethod]
    public void Given_ThereIsASavepoint_When_RollbackToSavepoint_Then_Rollback()
    {
        Db.BeginTransaction();
        Assert.IsNotNull(LastConnection.LastTransaction);
        const string savepointName = "Savepoint1";
        Db.CreateSavepoint(savepointName);
        Db.RollbackToSavepoint(savepointName);
        Assert.AreEqual(savepointName, LastConnection.LastTransaction.LastRollbackName);
    }

    #endregion

    #region When_CommitTransaction

    [TestMethod]
    public void Given_ThereIsNoTransaction_When_CommitTransaction_ThenThrows()
    {
        Db.Reconnect();
        Assert.IsNull(LastConnection.LastTransaction);
        Assert.ThrowsExactly<SharedDatabaseException>(Db.CommitTransaction);
    }

    [TestMethod]
    public void Given_ATransactionIsStarted_When_CommitTransaction_Then_TransactionIsCommitted()
    {
        Db.BeginTransaction();
        var originalTransaction = LastConnection.LastTransaction;
        Assert.IsNotNull(originalTransaction);
        Assert.IsFalse(originalTransaction.Committed);
        Db.CommitTransaction();
        Assert.IsTrue(originalTransaction.Committed);
    }

    [TestMethod]
    public void Given_ATransactionIsStarted_When_CommitTransaction_Then_TransactionIsConsumed()
    {
        Db.BeginTransaction();
        Db.CommitTransaction();
        Assert.IsTrue(TransactionConsumed);
    }

    #endregion

    #region When_BeginTransaction

    [TestMethod]
    public void When_BeginTransaction_Then_TransactionStartedEvent()
    {
        Db.BeginTransaction();
        Assert.IsTrue(TransactionStarted);
    }

    [TestMethod]
    public void Given_ConnectionIsNotMade_When_BeginTransaction_Then_ConnectionIsOpened_And_TransactionIsCreated_And_TransactionStartedEvent()
    {
        Db.Reconnect();
        LastConnection.Close();
        Db.BeginTransaction();
        Assert.AreEqual(ConnectionState.Open, LastConnection.State);
        Assert.IsTrue(TransactionStarted);
        Assert.IsNotNull(Db.Transaction);
    }

    [TestMethod]
    public void Given_ConnectionIsMade_When_BeginTransaction_Then_ConnectionIsOpened_And_TransactionIsCreated_And_TransactionStartedEvent()
    {
        Db.Reconnect();
        Assert.AreEqual(ConnectionState.Open, LastConnection.State);
        Assert.IsNull(LastConnection.LastTransaction);
        Db.BeginTransaction();
        Assert.IsTrue(TransactionStarted);
        Assert.IsNotNull(Db.Transaction);
    }

    [TestMethod]
    public void Given_ATransactionIsStarted_When_BeginTransaction_Then_Throws()
    {
        Db.BeginTransaction();
        Assert.ThrowsExactly<SharedDatabaseException>(Db.BeginTransaction);
    }

    #endregion

    #region When_CreateSavepoint

    [TestMethod]
    public void Given_ATransactionIsStarted_When_CreateSavepoint_Then_SavepointCreated()
    {
        Db.BeginTransaction();
        Assert.IsNotNull(LastConnection.LastTransaction);
        Db.CreateSavepoint("Savepoint1");
        Assert.AreEqual("Savepoint1", LastConnection.LastTransaction.LastSaveName);
    }

    [TestMethod]
    public void Given_ThereIsNoTransaction_When_CreateSavepoint_Then_Throws()
    {
        Db.Reconnect();
        Assert.IsNull(LastConnection.LastTransaction);
        Assert.ThrowsExactly<SharedDatabaseException>(() => Db.CreateSavepoint("Savepoint1"));
        Assert.IsNull(LastConnection.LastTransaction);
    }

    #endregion
}