using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;

namespace PeachtreeBus.DatabaseSharing.Tests;

[TestClass]
public class SharedDatabaseExceptionHandlingFixture
{
    private SharedDatabase _db = default!;
    private Mock<ISqlConnectionFactory> _connectionFactory = default!;
    private Mock<ISqlConnection> _connection = default!;
    private Mock<ISqlTransaction> _transaction = default!;

    private Exception? TransactionStartedHandlerException = null;
    private Exception? TransactionConsumedHandlerException = null;

    [TestInitialize]
    public void Intialize()
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
        if (TransactionConsumedHandlerException is not null)
            throw TransactionConsumedHandlerException;
    }

    private void TransactionStartedEventHandler(object? sender, EventArgs e)
    {
        if (TransactionStartedHandlerException is not null)
            throw TransactionStartedHandlerException;
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
        _connectionFactory.Setup(f => f.GetConnection()).Throws<Exception>();
        Assert.ThrowsException<Exception>(_db.BeginTransaction);
    }

    [TestMethod]
    public void Given_ConnectionBeginTransactionThrows_When_BeginTransaction_Then_Throws()
    {
        _connection.Setup(c => c.BeginTransaction()).Throws<Exception>();
        Assert.ThrowsException<Exception>(_db.BeginTransaction);
    }

    [TestMethod]
    public void Given_TransactionStartedHandlerThrows_When_BeginTransaction_Then_Throws()
    {
        TransactionStartedHandlerException = new();
        Assert.ThrowsException<Exception>(_db.BeginTransaction);
    }

    [TestMethod]
    public void Given_TransactionCommitThrows_When_CommitTransaction_Then_Throws()
    {
        _transaction.Setup(t => t.Commit()).Throws<Exception>();
        _db.BeginTransaction();
        Assert.ThrowsException<Exception>(_db.CommitTransaction);
    }

    [TestMethod]
    public void Given_TransactionConsumedHandlerThrows_When_CommitTransaction_Then_Throws()
    {
        _db.BeginTransaction();
        TransactionConsumedHandlerException = new();
        Assert.ThrowsException<Exception>(_db.CommitTransaction);
    }

    [TestMethod]
    public void Given_TransactionSaveThrows_When_CreateSavepoint_Then_Throws()
    {
        _transaction.Setup(t => t.Save(It.IsAny<string>())).Throws<Exception>();
        _db.BeginTransaction();
        Assert.ThrowsException<Exception>(() => _db.CreateSavepoint("Savepoint1"));
    }

    [TestMethod]
    public void Given_TransactionRollbackToSavepointThrows_When_RollbackToSavepoint_Then_Throws()
    {
        _transaction.Setup(t => t.Rollback(It.IsAny<string>())).Throws<Exception>();
        _db.BeginTransaction();
        Assert.ThrowsException<Exception>(() => _db.RollbackToSavepoint("Savepoint1"));
    }

    [TestMethod]
    public void Given_TransactionRollbackThrows_When_RollbackTransaction_Then_Throws()
    {
        _transaction.Setup(t => t.Rollback()).Throws<Exception>();
        _db.BeginTransaction();
        Assert.ThrowsException<Exception>(_db.RollbackTransaction);
    }

    [TestMethod]
    public void Given_TransactionConsumedHandlerThrows_When_RollbackTransaction_Then_Throws()
    {
        TransactionConsumedHandlerException = new();
        _db.BeginTransaction();
        Assert.ThrowsException<Exception>(_db.RollbackTransaction);
    }

    [TestMethod]
    public void Given_TransactionConsumedHandlerThrows_When_Reconnect_Then_Throws()
    {
        _db.BeginTransaction();
        TransactionConsumedHandlerException = new();
        Assert.ThrowsException<Exception>(_db.Reconnect);
    }

    [TestMethod]
    public void Given_ConnectionFactoryThrows_When_Reconnect_Then_Throws()
    {
        _connectionFactory.Setup(f => f.GetConnection()).Throws<Exception>();
        Assert.ThrowsException<Exception>(_db.Reconnect);
    }

    [TestMethod]
    public void Given_ConnectionOpenThrows_When_Reconnect_Then_Throws()
    {
        _connection.Setup(c => c.Open()).Throws<Exception>();
        Assert.ThrowsException<Exception>(_db.Reconnect);
    }

    [TestMethod]
    public void Given_TransactionConsumedHandlerThrows_When_Dispose_Then_Throws()
    {
        _db.BeginTransaction();
        TransactionConsumedHandlerException = new();
        Assert.ThrowsException<Exception>(_db.Dispose);
    }
}
