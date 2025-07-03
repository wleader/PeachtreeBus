using Microsoft.VisualStudio.TestTools.UnitTesting;
using PeachtreeBus.Testing;

namespace PeachtreeBus.DatabaseSharing.Tests;

[TestClass]
public class SharedDatabaseExternalConnectionFixture : SharedDatabaseFixtureBase
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
    public void Given_ExternalConnection_When_Reconnect_Then_Throws()
    {
        var connection = SqlServerTesting.CreateConnection();
        var transaction = SqlServerTesting.CreateTransaction();
        _db.SetExternallyManagedConnection(connection, transaction);
        Assert.ThrowsExactly<ExternallyManagedSqlConnectionException>(_db.Reconnect);
    }

    [TestMethod]
    public void When_SetExternalConnection_Then_ObjectsAreSetup()
    {
        var connection = SqlServerTesting.CreateConnection();
        var transaction = SqlServerTesting.CreateTransaction();
        _db.SetExternallyManagedConnection(connection, transaction);
        Assert.AreSame(connection, _db.Connection);
        Assert.AreSame(transaction, _db.Transaction);

        var internalConnection = GetInternalConnection();
        Assert.IsNotNull(internalConnection);
        Assert.IsTrue(internalConnection is ExternallyManagedSqlConnection);

        var internalTransaction = GetInternalTransaction();
        Assert.IsNotNull(internalTransaction);
        Assert.IsTrue(internalTransaction is ExternallyManagedSqlTransaction);
    }

    [TestMethod]
    public void Given_ExternalConnection_When_SetExternalAgain_OrginalsAreDisposed()
    {
        var connection1 = SqlServerTesting.CreateConnection();
        var transaction1 = SqlServerTesting.CreateTransaction();
        _db.SetExternallyManagedConnection(connection1, transaction1);

        var originalConnection = GetInternalConnection();
        Assert.IsNotNull(originalConnection);
        var originalTransaction = GetInternalTransaction();
        Assert.IsNotNull(originalTransaction);

        var connection2 = SqlServerTesting.CreateConnection();
        var transaction2 = SqlServerTesting.CreateTransaction();
        _db.SetExternallyManagedConnection(connection2, transaction2);

        Assert.IsTrue(originalConnection.Disposed);
        Assert.IsTrue(originalTransaction.Disposed);
    }
}
