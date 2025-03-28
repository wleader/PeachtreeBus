using Dapper;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using PeachtreeBus.Data;
using PeachtreeBus.DatabaseSharing;
using PeachtreeBus.Queues;
using PeachtreeBus.Tests.Fakes;
using System;
using System.Threading.Tasks;

namespace PeachtreeBus.DataAccessTests;

[TestClass]
public class TransactionProxyFixture : FixtureBase<DapperDataAccess>
{
    [TestInitialize]
    public override void TestInitialize()
    {
        base.TestInitialize();
    }

    [TestCleanup]
    public override void TestCleanup()
    {
        base.TestCleanup();
    }

    [TestMethod]
    public void Verify_ProxyForwardsTransactionEventsToServer()
    {
        SharedDB.BeginTransaction();
        Assert.AreEqual(1, CountTransactions());

        SharedDB.CommitTransaction();
        Assert.AreEqual(0, CountTransactions());

        SharedDB.BeginTransaction();
        Assert.AreEqual(1, CountTransactions());

        SharedDB.RollbackTransaction();
        Assert.AreEqual(0, CountTransactions());
    }

    private int CountTransactions()
    {
        return SharedDB.Connection.ExecuteScalar<int>("SELECT @@TRANCOUNT", transaction: SharedDB.Transaction);
    }

    [TestMethod]
    public async Task Verify_ProxyUsesSavepoints()
    {
        var message = new QueueData
        {
            MessageId = UniqueIdentity.New(),
            Body = new SerializedData("Foo"),
            Headers = new SerializedData("Baz"),
            Priority = 0,
            NotBefore = DateTime.UtcNow,
            Retries = 0,
            Completed = null,
            Failed = null,
            Enqueued = DateTime.UtcNow,
        };


        SharedDB.BeginTransaction();
        SharedDB.CreateSavepoint("Savepoint1");

        await dataAccess.AddMessage(message, DefaultQueue);

        SharedDB.RollbackToSavepoint("Savepoint1");
        SharedDB.CommitTransaction();

        var pending = await dataAccess.GetPendingQueued(DefaultQueue);
        Assert.IsNull(pending);
    }

    [TestMethod]
    public void Verify_DisposeTransaction()
    {
        using var connection = new SqlConnectionProxy(TestConfig.DbConnectionString);
        connection.Open();
        var transaction = connection.BeginTransaction();
        transaction.Dispose();
        Assert.IsTrue(transaction.Disposed);
    }

    protected override DapperDataAccess CreateDataAccess()
    {
        return new DapperDataAccess(SharedDB, Configuration.Object, MockLog.Object, FakeClock.Instance);
    }
}
