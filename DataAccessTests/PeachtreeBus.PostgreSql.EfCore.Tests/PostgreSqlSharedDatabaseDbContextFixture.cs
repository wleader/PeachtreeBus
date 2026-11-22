using System;
using System.Data;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Npgsql;
using PeachtreeBus.DatabaseSharing;
using PeachtreeBus.DatabaseTesting;
using PeachtreeBus.DatabaseTesting.PostgreSql;

namespace PeachtreeBus.PostgreSql.EfCore.Tests;

[TestClass]
public class PostgreSqlSharedDatabaseDbContextFixture
{
    private INpgSqlSharedDatabase _postgreSqlSharedDatabase = null!;
    private Mock<INpgSqlConnectionFactory> _connectionFactory = null!;

    private IPostgreSqlTestSettings _testSettings = null!;
    
    [TestInitialize]
    public void TestInitialize()
    {
        _testSettings = TestServices.GetService<IPostgreSqlTestSettings>();
        
        _connectionFactory = new();
        _connectionFactory.Setup(c => c.GetConnection())
            .Returns(() => new NpgSqlConnectionProxy(new(_testSettings.ConnectionString)));

        _postgreSqlSharedDatabase = new NpgSqlSharedDatabase(_connectionFactory.Object);

        CleanupData();
    }

    private void CleanupData()
    {
        using var connection = new NpgsqlConnection(_testSettings.ConnectionString);
        connection.Open();
        using var command = new NpgsqlCommand(
            """
            TRUNCATE TABLE example_app.audit_log;
            """, connection, null);
        command.ExecuteNonQuery();
    }

    [TestCleanup]
    public void TestCleanup()
    {
        _postgreSqlSharedDatabase.Dispose();
    }

    [TestMethod]
    public void Then_ContextIsDisposable()
    {
        var context = new TestableContext(_postgreSqlSharedDatabase);
        context.Dispose();
        Assert.ThrowsExactly<ObjectDisposedException>(() => _ = context.ChangeTracker);
    }

    [TestMethod]
    public void When_UseContext_Then_ConnectionIsOpened()
    {
        Assert.AreNotEqual(ConnectionState.Open, _postgreSqlSharedDatabase.Connection.State);
        var context = new TestableContext(_postgreSqlSharedDatabase);
        Assert.IsFalse(context.AuditLogs.Any());
        Assert.AreEqual(ConnectionState.Open, _postgreSqlSharedDatabase.Connection.State);
    }

    [TestMethod]
    public void Give_SharedDbHasTransaction_When_CreateContext_Then_TransactionIsEnlisted()
    {
        // start with  transaction before the context is created
        _postgreSqlSharedDatabase.BeginTransaction();
        using (var context = new TestableContext(_postgreSqlSharedDatabase))
        {
            context.AuditLogs.Add(new() { Occured = DateTime.UtcNow, Message = "TestMessage" });
            context.SaveChanges();
            _postgreSqlSharedDatabase.RollbackTransaction();
        }

        using (var context = new TestableContext(_postgreSqlSharedDatabase))
        {
            Assert.IsFalse(context.AuditLogs.Any());
        }
    }

    [TestMethod]
    public void Given_Context_When_SharedDbBeginsTransaction_Then_TransactionIsEnlisted()
    {
        // create the context, then start the transaction.
        using (var context = new TestableContext(_postgreSqlSharedDatabase))
        {
            _postgreSqlSharedDatabase.BeginTransaction();
            context.AuditLogs.Add(new() { Occured = DateTime.UtcNow, Message = "TestMessage" });
            context.SaveChanges();
            _postgreSqlSharedDatabase.RollbackTransaction();
        }

        using (var context = new TestableContext(_postgreSqlSharedDatabase))
        {
            Assert.IsFalse(context.AuditLogs.Any());
        }
    }
}
