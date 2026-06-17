using Microsoft.Data.SqlClient;
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
        var connection = UninitializedObjects.Create<SqlConnection>();
        var transaction = UninitializedObjects.Create<SqlTransaction>();
        Db.SetExternallyManagedConnection(connection, transaction);
        Assert.ThrowsExactly<ExternallyManagedConnectionException>(Db.Reconnect);
    }

    [TestMethod]
    public void When_SetExternalConnection_Then_ObjectsAreSetup()
    {
        var connection = UninitializedObjects.Create<SqlConnection>();
        var transaction = UninitializedObjects.Create<SqlTransaction>();
        Db.SetExternallyManagedConnection(connection, transaction);
        Assert.AreSame(connection, Db.Connection);
        Assert.AreSame(transaction, Db.Transaction);

        var internalConnection = GetInternalConnection();
        Assert.IsNotNull(internalConnection);
        Assert.IsTrue(internalConnection is ExternallyManagedTestableConnection);

        var internalTransaction = GetInternalTransaction();
        Assert.IsNotNull(internalTransaction);
        Assert.IsTrue(internalTransaction is ExternallyManagedTestableTransaction);
    }

    [TestMethod]
    public void Given_ExternalConnection_When_SetExternalAgain_OriginalsAreDisposed()
    {
        var connection1 = UninitializedObjects.Create<SqlConnection>();
        var transaction1 = UninitializedObjects.Create<SqlTransaction>();
        Db.SetExternallyManagedConnection(connection1, transaction1);

        var originalConnection = GetInternalConnection();
        Assert.IsNotNull(originalConnection);
        var originalTransaction = GetInternalTransaction();
        Assert.IsNotNull(originalTransaction);

        var connection2 = UninitializedObjects.Create<SqlConnection>();
        var transaction2 = UninitializedObjects.Create<SqlTransaction>();
        Db.SetExternallyManagedConnection(connection2, transaction2);

        Assert.IsTrue(originalConnection.Disposed);
        Assert.IsTrue(originalTransaction.Disposed);
    }
}
