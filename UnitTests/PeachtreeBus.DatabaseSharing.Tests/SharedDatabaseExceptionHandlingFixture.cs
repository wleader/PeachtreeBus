using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using Moq.Language.Flow;

namespace PeachtreeBus.DatabaseSharing.Tests;

[TestClass]
public class SharedDatabaseExceptionHandlingFixture
{
    private SharedDatabase _db = null!;
    private Mock<ISqlConnectionFactory> _connectionFactory = null!;
    private Mock<ISqlConnection> _connection = null!;
    private Mock<ISqlTransaction> _transaction = null!;

    private Exception? _transactionStartedHandlerException;
    private Exception? _transactionConsumedHandlerException;

    [TestInitialize]
    public void Initialize()
    {
        _transaction = new();

        _connection = new();
        _connection.Setup(c => c.BeginTransaction())
            .Returns(() => _transaction.Object);

        _connectionFactory = new();
        _connectionFactory.Setup(f => f.GetConnection())
            .Returns(() => _connection.Object);

        _db = new(_connectionFactory.Object);
        _db.TransactionStarted += TransactionStartedEventHandler;
        _db.TransactionConsumed += TransactionConsumedEventHandler;
    }

    [TestCleanup]
    public void Cleanup()
    {
        _db.TransactionStarted -= TransactionStartedEventHandler;
        _db.TransactionConsumed -= TransactionConsumedEventHandler;
    }

    private void TransactionConsumedEventHandler(object? sender, EventArgs e)
    {
        if (_transactionConsumedHandlerException is not null)
            throw _transactionConsumedHandlerException;
    }

    private void TransactionStartedEventHandler(object? sender, EventArgs e)
    {
        if (_transactionStartedHandlerException is not null)
            throw _transactionStartedHandlerException;
    }

    [TestMethod]
    public void Given_NothingThrows_When_Used_Then_NoThrows()
    {
        _db.Reconnect();
        _db.BeginTransaction();
        _db.CreateSavepoint("Savepoint1");
        _db.RollbackToSavepoint("Savepoint1");
        _db.RollbackTransaction();
        _db.BeginTransaction();
        _db.CommitTransaction();
        _db.Dispose();
    }

    [TestMethod]
    public void Given_ConnectionFactoryThrows_When_BeginTransaction_Then_Throws()
    {
        var ex = new Exception("Test Exception");
        _connectionFactory.Setup(f => f.GetConnection()).Throws(ex);
        var thrown = Assert.Throws<Exception>(_db.BeginTransaction);
        Assert.AreSame(ex, thrown);
    }

    [TestMethod]
    public void Given_ConnectionBeginTransactionThrows_When_BeginTransaction_Then_Throws()
    {
        Given_Setup_When_Action_Then_Throws(
            _connection.Setup(c => c.BeginTransaction()),
            _db.BeginTransaction);
    }

    [TestMethod]
    public void Given_TransactionStartedHandlerThrows_When_BeginTransaction_Then_Throws()
    {
        _transactionStartedHandlerException = new();
        var thrown = Assert.Throws<Exception>(_db.BeginTransaction);
        Assert.AreSame(_transactionStartedHandlerException, thrown);
    }

    [TestMethod]
    public void Given_TransactionCommitThrows_When_CommitTransaction_Then_Throws()
    {
        _db.BeginTransaction();
        Given_Setup_When_Action_Then_Throws(
            _transaction.Setup(t => t.Commit()),
            _db.CommitTransaction);
    }

    [TestMethod]
    public void Given_TransactionConsumedHandlerThrows_When_CommitTransaction_Then_Throws()
    {
        _db.BeginTransaction();
        Given_TransactionConsumed_When_Action_Then_Throws(_db.CommitTransaction);
    }

    [TestMethod]
    public void Given_TransactionSaveThrows_When_CreateSavepoint_Then_Throws()
    {
        _db.BeginTransaction();
        Given_Setup_When_Action_Then_Throws(
            _transaction.Setup(t => t.Save(It.IsAny<string>())),
            () => _db.CreateSavepoint("Savepoint1"));
    }

    [TestMethod]
    public void Given_TransactionRollbackToSavepointThrows_When_RollbackToSavepoint_Then_Throws()
    {
        _db.BeginTransaction();
        Given_Setup_When_Action_Then_Throws(
            _transaction.Setup(t => t.Rollback(It.IsAny<string>())),
            () => _db.RollbackToSavepoint("Savepoint1"));
    }

    [TestMethod]
    public void Given_TransactionRollbackThrows_When_RollbackTransaction_Then_Throws()
    {
        _db.BeginTransaction();
        Given_Setup_When_Action_Then_Throws(
            _transaction.Setup(t => t.Rollback()),
            _db.RollbackTransaction);
    }

    [TestMethod]
    public void Given_TransactionConsumedHandlerThrows_When_RollbackTransaction_Then_Throws()
    {
        _db.BeginTransaction();
        Given_TransactionConsumed_When_Action_Then_Throws(_db.RollbackTransaction);
    }

    [TestMethod]
    public void Given_TransactionConsumedHandlerThrows_When_Reconnect_Then_Throws()
    {
        _db.BeginTransaction();
        Given_TransactionConsumed_When_Action_Then_Throws(_db.Reconnect);
    }

    [TestMethod]
    public void Given_ConnectionFactoryThrows_When_Reconnect_Then_Throws()
    {
        Given_Setup_When_Action_Then_Throws(
            _connectionFactory.Setup(f => f.GetConnection()),
            _db.Reconnect);
    }

    [TestMethod]
    public void Given_ConnectionOpenThrows_When_Reconnect_Then_Throws()
    {
        Given_Setup_When_Action_Then_Throws(
            _connection.Setup(c => c.Open()),
            _db.Reconnect);
    }

    [TestMethod]
    public void Given_TransactionConsumedHandlerThrows_When_Dispose_Then_Throws()
    {
        _db.BeginTransaction();
        Given_TransactionConsumed_When_Action_Then_Throws(_db.Dispose);
    }

    private static void Given_Setup_When_Action_Then_Throws<T>(ISetup<T> setup, Action action)
        where T : class
    {
        var ex = new Exception("Test Exception");
        setup.Throws(ex);
        var thrown = Assert.Throws<Exception>(action);
        Assert.AreSame(ex, thrown);
    }
    
    private static void Given_Setup_When_Action_Then_Throws<T, TResult>(ISetup<T, TResult> setup, Action action)
        where T : class
    {
        var ex = new Exception("Test Exception");
        setup.Throws(ex);
        var thrown = Assert.Throws<Exception>(action);
        Assert.AreSame(ex, thrown);
    }
    
    private void Given_TransactionConsumed_When_Action_Then_Throws(Action action)
    {
        _transactionConsumedHandlerException = new();
        var thrown = Assert.Throws<Exception>(action);
        Assert.AreSame(_transactionConsumedHandlerException, thrown);
    }
}
