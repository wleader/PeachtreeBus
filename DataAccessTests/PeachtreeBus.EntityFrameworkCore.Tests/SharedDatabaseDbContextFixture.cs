using Microsoft.Data.SqlClient;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using PeachtreeBus.DatabaseSharing;
using System;
using System.Data;
using System.Linq;
using PeachtreeBus.DatabaseTesting;
using PeachtreeBus.DatabaseTesting.MsSql;

namespace PeachtreeBus.EntityFrameworkCore.Tests;

[TestClass]
public class SharedDatabaseDbContextFixture
{
    private MsSqlSharedDatabase _msSqlSharedDatabase = null!;
    private Mock<ISqlConnectionFactory> _connectionFactory = null!;

    private IMsSqlTestSettings _testSettings = null!;
    
    [TestInitialize]
    public void TestInitialize()
    {
        _testSettings = TestServices.GetService<IMsSqlTestSettings>();
        
        _connectionFactory = new();
        _connectionFactory.Setup(c => c.GetConnection())
            .Returns(() => new SqlConnectionProxy(new(_testSettings.ConnectionString)));

        _msSqlSharedDatabase = new MsSqlSharedDatabase(_connectionFactory.Object);

        CleanupData();
    }

    private void CleanupData()
    {
        using var connection = new SqlConnection(_testSettings.ConnectionString);
        connection.Open();
        using var command = new SqlCommand(
            """
            DELETE FROM [ExampleApp].[AuditLog]
            """, connection, null);
        command.ExecuteNonQuery();
    }

    [TestCleanup]
    public void TestCleanup()
    {
        _msSqlSharedDatabase.Dispose();
    }

    [TestMethod]
    public void Then_ContextIsDisposable()
    {
        var context = new TestableContext(_msSqlSharedDatabase);
        context.Dispose();
        Assert.IsTrue(context.Disposed);
    }

    [TestMethod]
    public void When_UseContext_Then_ConnectionIsOpened()
    {
        Assert.AreNotEqual(ConnectionState.Open, _msSqlSharedDatabase.Connection.State);
        var context = new TestableContext(_msSqlSharedDatabase);
        Assert.IsFalse(context.AuditLogs.Any());
        Assert.AreEqual(ConnectionState.Open, _msSqlSharedDatabase.Connection.State);
    }

    [TestMethod]
    public void Give_SharedDbHasTransaction_When_CreateContext_Then_TransactionIsEnlisted()
    {
        // start with  transaction before the context is created
        _msSqlSharedDatabase.BeginTransaction();
        using (var context = new TestableContext(_msSqlSharedDatabase))
        {
            context.AuditLogs.Add(new() { Occured = DateTime.UtcNow, Message = "TestMessage" });
            context.SaveChanges();
            _msSqlSharedDatabase.RollbackTransaction();
        }

        using (var context = new TestableContext(_msSqlSharedDatabase))
        {
            Assert.IsFalse(context.AuditLogs.Any());
        }
    }

    [TestMethod]
    public void Given_Context_When_SharedDbBeginsTransaction_Then_TransactionIsEnlisted()
    {
        // create the context, then start the transaction.
        using (var context = new TestableContext(_msSqlSharedDatabase))
        {
            _msSqlSharedDatabase.BeginTransaction();
            context.AuditLogs.Add(new() { Occured = DateTime.UtcNow, Message = "TestMessage" });
            context.SaveChanges();
            _msSqlSharedDatabase.RollbackTransaction();
        }

        using (var context = new TestableContext(_msSqlSharedDatabase))
        {
            Assert.IsFalse(context.AuditLogs.Any());
        }
    }
}
