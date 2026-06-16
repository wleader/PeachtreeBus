using System;
using System.Threading.Tasks;
using Dapper;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using PeachtreeBus.DatabaseTesting;

namespace PeachtreeBus.DatabaseSharing.PostgreSql.Tests;

[TestClass]
public sealed class PostgreSqlTransactionProxyFixture
{
    private IServiceScope _scope = null!;

    [TestInitialize]
    public void Initialize()
    {
        _scope = TestServices.ServiceProvider.CreateScope();
    }
    
    [TestMethod]
    public async Task Verify_ProxyForwardsTransactionEventsToServer()
    {
        var db = _scope.ServiceProvider.GetRequiredService<NpgSqlSharedDatabase>();
        db.Reconnect();
        
        db.BeginTransaction();
        Insert(db); // postgres doesn't actually start a transaction unless you have modified something.
        AssertHasTransaction(db, true);
        
        db.CommitTransaction();
        AssertHasTransaction(db, false);
        
        db.BeginTransaction();
        Insert(db); // postgres doesn't actually start a transaction unless you have modified something.
        AssertHasTransaction(db, true);
        
        db.RollbackTransaction();
        AssertHasTransaction(db, false);
    }

    [TestMethod]
    public async Task Verify_ProxyUsesSavePoints()
    {
        var db = _scope.ServiceProvider.GetRequiredService<NpgSqlSharedDatabase>();
        db.Reconnect();

        db.Connection.Execute("DELETE FROM peachtreebus.subscriptions");

        db.BeginTransaction();
        db.CreateSavepoint("Savepoint1");

        Insert(db);
        db.RollbackToSavepoint("Savepoint1");
        Insert(db);
        db.CommitTransaction();

        var count = db.Connection.ExecuteScalar<int>("SELECT COUNT(*) FROM peachtreebus.subscriptions");
        Assert.AreEqual(1, count);
    }

    private static void AssertHasTransaction(INpgSqlSharedDatabase db, bool expected)
    {
        var id = db.Connection.QueryFirstOrDefault<long?>("SELECT pg_current_xact_id_if_assigned();");
        Assert.AreEqual(expected, id.HasValue);
    }
    
    private static void Insert(INpgSqlSharedDatabase db)
    {
        const string statement =
            """
            INSERT INTO peachtreebus.subscriptions
            (subscriber_id,topic,valid_until)
            VALUES
            ( @SubscriberId, @Topic, NOW());
            """;
        var p = new DynamicParameters();
        p.Add("@SubscriberId", Guid.NewGuid());
        p.Add("@Topic", "Topic");

        db.Connection.Execute(statement, p, transaction: db.Transaction);
    }

    [TestMethod]
    public void Verify_DisposeTransaction()
    {
        var factory = TestServices.GetService<INpgSqlConnectionFactory>();
        using var connection = factory.GetConnection();
        connection.Open();
        var t = connection.BeginTransaction();
        t.Dispose();
        Assert.IsTrue(t.Disposed);
    }
}