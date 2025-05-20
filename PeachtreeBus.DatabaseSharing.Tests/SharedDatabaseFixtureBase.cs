using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;

namespace PeachtreeBus.DatabaseSharing.Tests;

public abstract class SharedDatabaseFixtureBase
{
    protected SharedDatabase _db = default!;
    protected SqlConnectionFake _lastConnection = default!;
    protected bool _transactionConsumed = false;
    protected bool _transactionStarted = false;

    public virtual void Initialize()
    {
        var connectionFactory = new Mock<ISqlConnectionFactory>();
        connectionFactory.Setup(f => f.GetConnection()).Returns(GetNewConnection);

        _db = new SharedDatabase(connectionFactory.Object);
        _db.TransactionConsumed += TransactionConsumedEventHandler;
        _db.TransactionStarted += TransactionStartedEventHandler;
    }

    public virtual void Cleanup()
    {
        _db.TransactionConsumed -= TransactionConsumedEventHandler;
    }

    private SqlConnectionFake GetNewConnection()
    {
        return _lastConnection = new SqlConnectionFake();
    }

    private void TransactionConsumedEventHandler(object? sender, EventArgs e)
    {
        Assert.IsFalse(_transactionConsumed, "Transaction Consumed Event Fired Twice.");
        _transactionConsumed = true;
    }

    private void TransactionStartedEventHandler(object? sender, EventArgs e)
    {
        Assert.IsFalse(_transactionStarted, "Transaction Started Event Fired Twice.");
        _transactionStarted = true;
    }

    protected ISqlConnection? GetInternalConnection()
    {
        return (ISqlConnection?)
            typeof(SharedDatabase)
            .GetField(
                "_connection",
                System.Reflection.BindingFlags.Instance |
                System.Reflection.BindingFlags.NonPublic)!
            .GetValue(_db);
    }

    protected ISqlTransaction? GetInternalTransaction()
    {
        return (ISqlTransaction?)
            typeof(SharedDatabase)
            .GetField(
                "_transaction",
                System.Reflection.BindingFlags.Instance |
                System.Reflection.BindingFlags.NonPublic)!
            .GetValue(_db);
    }

}
