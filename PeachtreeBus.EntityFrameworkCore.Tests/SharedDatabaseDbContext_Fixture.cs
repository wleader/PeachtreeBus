using PeachtreeBus.DatabaseSharing;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using Microsoft.Extensions.Configuration;
using Microsoft.Data.SqlClient;
using System.Data;

namespace PeachtreeBus.EntityFrameworkCore.Tests;

[TestClass]
public class SharedDatabaseDbContext_Fixture
{


    private SharedDatabase _sharedDatabase = default!;
    private Mock<ISqlConnectionFactory> _connectionFactory = default!;
    private SqlConnectionProxy _connection = default!;


    private string? _connectionString;
    private string ConnectionString
    {
        get => _connectionString ??= ReadConnectionString();
    }

    private static string ReadConnectionString()
    {
        var configurationBuilder = new ConfigurationBuilder();
        configurationBuilder.AddJsonFile("appSettings.json");
        var config = configurationBuilder.Build();
        return config.GetConnectionString("PeachtreeBus")
            ?? throw new InvalidOperationException("Could not read connection string from appsettings.json");
    }

    [TestInitialize]
    public void TestInitialize()
    {
        _connectionFactory = new();
        _connectionFactory.Setup(c => c.GetConnection()).Returns(() => _connection = new(ConnectionString));

        _sharedDatabase = new SharedDatabase(_connectionFactory.Object);

        CleanupData();
    }

    private void CleanupData()
    {
        using var connection = new SqlConnection(ConnectionString);
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
        _connection.Dispose();
    }

    [TestMethod]
    public void Then_ContextIsDisposable()
    {
        var context = new TestableContext(_sharedDatabase);
        context.Dispose();
        Assert.IsFalse(_connection.Disposed);
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
