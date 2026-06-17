using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;

namespace PeachtreeBus.DatabaseSharing.Tests;

public abstract class SharedDatabaseFixtureBase
{
    protected TestableSharedDatabase Db = null!;
    protected TestableConnectionFake? LastConnection;
    protected bool TransactionConsumed;
    protected bool TransactionStarted;

    public virtual void Initialize()
    {
        var connectionFactory = new Mock<ITestableConnectionFactory>();
        connectionFactory.Setup(f => f.GetConnection()).Returns(GetNewConnection);

        Db = new TestableSharedDatabase(connectionFactory.Object);
        Db.TransactionConsumed += TransactionConsumedEventHandler;
        Db.TransactionStarted += TransactionStartedEventHandler;
    }

    public virtual void Cleanup()
    {
        Db.TransactionConsumed -= TransactionConsumedEventHandler;
    }

    private TestableConnectionFake GetNewConnection()
    {
        return LastConnection = new TestableConnectionFake();
    }

    private void TransactionConsumedEventHandler(object? sender, EventArgs e)
    {
        Assert.IsFalse(TransactionConsumed, "Transaction Consumed Event Fired Twice.");
        TransactionConsumed = true;
    }

    private void TransactionStartedEventHandler(object? sender, EventArgs e)
    {
        Assert.IsFalse(TransactionStarted, "Transaction Started Event Fired Twice.");
        TransactionStarted = true;
    }

    protected ITestableConnection? GetInternalConnection()
    {
        return (ITestableConnection?)
            typeof(TestableSharedDatabase).BaseType!
            .GetField(
                "_connection",
                System.Reflection.BindingFlags.Instance |
                System.Reflection.BindingFlags.NonPublic)!
            .GetValue(Db);
    }

    protected ITestableTransaction? GetInternalTransaction()
    {
        return (ITestableTransaction?)
            typeof(TestableSharedDatabase).BaseType!
            .GetField(
                "_transaction",
                System.Reflection.BindingFlags.Instance |
                System.Reflection.BindingFlags.NonPublic)!
            .GetValue(Db);
    }

}
