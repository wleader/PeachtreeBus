using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using PeachtreeBus.DatabaseSharing;
using System;
using System.Data;
using System.Linq;
using PeachtreeBus.DatabaseTestingShared;

namespace PeachtreeBus.EntityFrameworkCore.Tests;

[TestClass]
public class SharedDatabaseDbContext_Fixture
{
    private SharedDatabase _sharedDatabase = default!;
    private Mock<ISqlConnectionFactory> _connectionFactory = default!;

    [TestInitialize]
    public void TestInitialize()
    {
        _connectionFactory = new();
        _connectionFactory.Setup(c => c.GetConnection())
            .Returns(() => new SqlConnectionProxy(TestSettings.TestDatabase));

        _sharedDatabase = new SharedDatabase(_connectionFactory.Object);

        CleanupData();
    }

    private void CleanupData()
    {
        using var connection = new SqlConnection(TestSettings.TestDatabase);
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
        _sharedDatabase.Dispose();
    }

    [TestMethod]
    public void Then_ContextIsDisposable()
    {
        var context = new TestableContext(_sharedDatabase);
        context.Dispose();
        Assert.IsTrue(context.Disposed);
    }

    [TestMethod]
    public void When_UseContext_Then_ConnectionIsOpened()
    {
        Assert.AreNotEqual(ConnectionState.Open, _sharedDatabase.Connection.State);
        var context = new TestableContext(_sharedDatabase);
        Assert.IsFalse(context.AuditLogs.Any());
        Assert.AreEqual(ConnectionState.Open, _sharedDatabase.Connection.State);
    }

    [TestMethod]
    public void Give_SharedDbHasTransaction_When_CreateContext_Then_TransactionIsEnlisted()
    {
        // start with  transaction before the context is created
        _sharedDatabase.BeginTransaction();
        using (var context = new TestableContext(_sharedDatabase))
        {
            context.AuditLogs.Add(new() { Occured = DateTime.UtcNow, Message = "TestMessage" });
            context.SaveChanges();
            _sharedDatabase.RollbackTransaction();
        }

        using (var context = new TestableContext(_sharedDatabase))
        {
            Assert.IsFalse(context.AuditLogs.Any());
        }
    }

    [TestMethod]
    public void Given_Context_When_SharedDbBeginsTransaction_Then_TransactionIsEnlisted()
    {
        // create the context, then start the transaction.
        using (var context = new TestableContext(_sharedDatabase))
        {
            _sharedDatabase.BeginTransaction();
            context.AuditLogs.Add(new() { Occured = DateTime.UtcNow, Message = "TestMessage" });
            context.SaveChanges();
            _sharedDatabase.RollbackTransaction();
        }

        using (var context = new TestableContext(_sharedDatabase))
        {
            Assert.IsFalse(context.AuditLogs.Any());
        }
    }
}
