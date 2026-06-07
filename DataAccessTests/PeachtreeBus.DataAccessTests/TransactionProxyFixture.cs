using Dapper;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using PeachtreeBus.ClassNames;
using PeachtreeBus.Data;
using PeachtreeBus.Queues;
using System;
using System.Threading.Tasks;
using PeachtreeBus.DatabaseSharing;
using PeachtreeBus.DatabaseTesting;
using PeachtreeBus.DatabaseTesting.MsSql;

namespace PeachtreeBus.DataAccessTests;

[TestClass]
public class TransactionProxyFixture : FixtureBase<MsSqlBusDataAccess>
{
    [TestInitialize]
    public override void Initialize() => base.Initialize();

    [TestCleanup]
    public override void Cleanup() => base.Cleanup();

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
            Headers = new() { MessageClass = ClassName.Default },
            Priority = 0,
            NotBefore = DateTime.UtcNow,
            Retries = 0,
            Completed = null,
            Failed = null,
            Enqueued = DateTime.UtcNow,
        };


        SharedDB.BeginTransaction();
        SharedDB.CreateSavepoint("Savepoint1");

        await BusDataAccess.AddMessage(message, TestConfig.DefaultQueue);

        SharedDB.RollbackToSavepoint("Savepoint1");
        SharedDB.CommitTransaction();

        var pending = await BusDataAccess.GetPendingQueued(TestConfig.DefaultQueue);
        Assert.IsNull(pending);
    }

    [TestMethod]
    public void Verify_DisposeTransaction()
    {
        var factory = TestServices.GetService<ISqlConnectionFactory>();
        using var connection = factory.GetConnection();
        connection.Open();
        var transaction = connection.BeginTransaction();
        transaction.Dispose();
        Assert.IsTrue(transaction.Disposed);
    }

    protected override MsSqlBusDataAccess CreateDataAccess()
    {
        return new (
            SharedDB,
            Configuration.Object,
            MockLog.Object,
            DapperMethods,
            FakeBreakerProvider);
    }
}