using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using PeachtreeBus.Core.Tests.Fakes;
using PeachtreeBus.Data;
using PeachtreeBus.DatabaseSharing;

namespace PeachtreeBus.DataAccessTests;

[TestClass]
public class TransactionsFixture
{
    private readonly Mock<ILogger<MsSqlBusDataAccess>> _mockLog = new();
    private readonly Mock<ISqlSharedDatabase> _mockSharedDatabase = new();
    private readonly Mock<IDapperMethods> _mockDapperMethods = new();
    private readonly FakeBreakerProvider _fakeBreakerProvider = new();
    private MsSqlBusDataAccess _dataAccess = null!;

    [TestInitialize]
    public void TestInitialize()
    {
        _mockLog.Reset();
        _mockSharedDatabase.Reset();
        _mockDapperMethods.Reset();
        _dataAccess = new(
            _mockSharedDatabase.Object,
            null!,
            _mockLog.Object,
            _mockDapperMethods.Object,
            _fakeBreakerProvider);
    }

    [TestMethod]
    public void StartTransaction_InvokesSharedDB()
    {
        _mockSharedDatabase.Setup(db => db.BeginTransaction()).Verifiable();
        _dataAccess.BeginTransaction();
        _mockSharedDatabase.Verify();
    }

    [TestMethod]
    public void CommitTransaction_InvokesSharedDB()
    {
        _mockSharedDatabase.Setup(db => db.CommitTransaction()).Verifiable();
        _dataAccess.CommitTransaction();
        _mockSharedDatabase.Verify();
    }

    [TestMethod]
    public void RollbackTransaction_InvokesSharedDB()
    {
        _mockSharedDatabase.Setup(db => db.RollbackTransaction()).Verifiable();
        _dataAccess.RollbackTransaction();
        _mockSharedDatabase.Verify();
    }

    [TestMethod]
    public void CreateSavepoint_InvokesSharedDB()
    {
        _mockSharedDatabase.Setup(db => db.CreateSavepoint("Savepoint")).Verifiable();
        _dataAccess.CreateSavepoint("Savepoint");
        _mockSharedDatabase.Verify();
    }

    [TestMethod]
    public void RollbackToSavepoint_InvokesSharedDB()
    {
        _mockSharedDatabase.Setup(db => db.RollbackToSavepoint("Savepoint")).Verifiable();
        _dataAccess.RollbackToSavepoint("Savepoint");
        _mockSharedDatabase.Verify();
    }
}