using System;
using Dapper;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using PeachtreeBus.DatabaseSharing;
using PeachtreeBus.DatabaseTesting;

namespace PeachtreeBus.MsSql.DataAccessTests.DatabaseSharing;

[TestClass]
public sealed class MsSqlTransactionProxyFixture
{
    private IServiceScope _scope = null!;

    [TestInitialize]
    public void Initialize()
    {
        _scope = TestServices.ServiceProvider.CreateScope();
    }

    [TestMethod]
    public void Verify_ProxyForwardsTransactionEventsToServer()
    {
        var db = _scope.ServiceProvider.GetRequiredService<MsSqlSharedDatabase>();
        db.Reconnect();

        db.BeginTransaction();
        AssertTransactionCount(1);

        db.CommitTransaction();
        AssertTransactionCount(0);

        db.BeginTransaction();
        AssertTransactionCount(1);

        db.RollbackTransaction();
        AssertTransactionCount(0);
        return;

        void AssertTransactionCount(int expected)
        {
            // ReSharper disable once StringLiteralTypo
            var actual = db.Connection.ExecuteScalar<int>("SELECT @@TRANCOUNT", transaction: db.Transaction);
            Assert.AreEqual(expected, actual);
        }
    }

    [TestMethod]
    public void Verify_ProxyUsesSavePoints()
    {
        var db = _scope.ServiceProvider.GetRequiredService<MsSqlSharedDatabase>();
        db.Reconnect();

        db.Connection.Execute("TRUNCATE TABLE [PeachtreeBus].[Subscriptions]");

        db.BeginTransaction();
        db.CreateSavepoint("Savepoint1");

        Insert();
        db.RollbackToSavepoint("Savepoint1");
        Insert();
        db.CommitTransaction();

        var count = db.Connection.ExecuteScalar<int>("SELECT COUNT(*) FROM [PeachtreeBus].[Subscriptions]");
        Assert.AreEqual(1, count);
        return;

        void Insert()
        {
            // ReSharper disable once StringLiteralTypo
            const string statement =
                """
                INSERT INTO [PeachtreeBus].[Subscriptions]
                ([SubscriberId],[Topic],[ValidUntil])
                VALUES
                (@SubscriberId, @Topic, SYSUTCDATETIME());
                """;
            var p = new DynamicParameters();
            p.Add("@SubscriberId", Guid.NewGuid());
            p.Add("@Topic", "Topic");

            db.Connection.Execute(statement, p, transaction: db.Transaction);
        }
    }

    [TestMethod]
    public void Verify_DisposeTransaction()
    {
        var factory = TestServices.GetService<ISqlConnectionFactory>();
        var connection = factory.GetConnection();
        connection.Open();
        var t = connection.BeginTransaction();
        t.Dispose();
        Assert.IsTrue(t.Disposed);
    }
}