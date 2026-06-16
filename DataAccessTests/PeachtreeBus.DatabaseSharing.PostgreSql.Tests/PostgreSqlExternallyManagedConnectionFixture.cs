using System.Data;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using PeachtreeBus.DatabaseTesting;

namespace PeachtreeBus.DatabaseSharing.PostgreSql.Tests;

[TestClass]
public class PostgreSqlExternallyManagedConnectionFixture
{
    private IServiceScope _scope = null!;
    
    [TestInitialize]
    public void Initialize()
    {
        _scope = TestServices.ServiceProvider.CreateScope();
    }

    [TestMethod]
    public void Given_SharedDatabaseWithExternalConnection_When_Dispose_Then_ExternalConnectionIsNotDisposed()
    {
        var factory = _scope.ServiceProvider.GetRequiredService<INpgSqlConnectionFactory>();
        var connection = factory.GetConnection();
        connection.Open();
        var transaction = connection.BeginTransaction();
        var sharedDb = _scope.ServiceProvider.GetRequiredService<NpgSqlSharedDatabase>();
        sharedDb.SetExternallyManagedConnection(connection.Connection, transaction.Transaction);
        
        sharedDb.Dispose();
        
        Assert.AreEqual(ConnectionState.Open, connection.State);
        Assert.IsFalse(transaction.Disposed);
        Assert.IsFalse(connection.Disposed);
    }
}