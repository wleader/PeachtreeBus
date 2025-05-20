using Microsoft.Data.SqlClient;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Runtime.CompilerServices;

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

    private static T GetUninitialzed<T>()
    {
        return (T)RuntimeHelpers.GetUninitializedObject(typeof(T));
    }

    [TestMethod]
    public void Given_ExternalConnection_When_Reconnect_Then_Throws()
    {
        var connection = GetUninitialzed<SqlConnection>();
        var transaction = GetUninitialzed<SqlTransaction>();
        _db.SetExternallyManagedConnection(connection, transaction);
        Assert.ThrowsExactly<ExternallyManagedSqlConnectionException>(_db.Reconnect);
    }

    [TestMethod]
    public void When_SetExternalConnection_Then_ObjectsAreSetup()
    {
        var connection = GetUninitialzed<SqlConnection>();
        var transaction = GetUninitialzed<SqlTransaction>();
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
        var connection1 = GetUninitialzed<SqlConnection>();
        var transaction1 = GetUninitialzed<SqlTransaction>();
        _db.SetExternallyManagedConnection(connection1, transaction1);

        var originalConnection = GetInternalConnection();
        Assert.IsNotNull(originalConnection);
        var originalTransaction = GetInternalTransaction();
        Assert.IsNotNull(originalTransaction);

        var connection2 = GetUninitialzed<SqlConnection>();
        var transaction2 = GetUninitialzed<SqlTransaction>();
        _db.SetExternallyManagedConnection(connection2, transaction2);

        Assert.IsTrue(originalConnection.Disposed);
        Assert.IsTrue(originalTransaction.Disposed);
    }
}
