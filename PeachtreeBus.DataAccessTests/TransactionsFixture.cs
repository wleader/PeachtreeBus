using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using PeachtreeBus.Data;
using PeachtreeBus.DatabaseSharing;

namespace PeachtreeBus.DataAccessTests
{
    /// <summary>
    /// Proves the behavior of DapperDataAccess CreateSavePoint, RollbackToSavepoint,
    /// BeginTransaction, RollbackTransaction, and CommitTransaction
    /// </summary>
    [TestClass]
    public class TransactionsFixture
    {
        private Mock<ILogger<DapperDataAccess>> MockLog = default!;
        private Mock<ISharedDatabase> MockSharedDatabase = default!;
        private DapperDataAccess dataAccess = default!;

        [TestInitialize]
        public void TestInitialize()
        {
            MockLog = new Mock<ILogger<DapperDataAccess>>();
            MockSharedDatabase = new Mock<ISharedDatabase>();
            dataAccess = new DapperDataAccess(MockSharedDatabase.Object, null!, MockLog.Object);
        }

        /// <summary>
        /// Proves that BeginTransaction is passed through to the Shared Database object.
        /// </summary>
        [TestMethod]
        public void StartTransaction_InvokesSharedDB()
        {
            MockSharedDatabase.Setup(db => db.BeginTransaction()).Verifiable();
            dataAccess.BeginTransaction();
            MockSharedDatabase.Verify();
        }

        /// <summary>
        /// Proves that CommitTransaction is passed through to the Shared Database object.
        /// </summary>
        [TestMethod]
        public void CommitTransaction_InvokesSharedDB()
        {
            MockSharedDatabase.Setup(db => db.CommitTransaction()).Verifiable();
            dataAccess.CommitTransaction();
            MockSharedDatabase.Verify();
        }

        /// <summary>
        /// Proves that RollbackTransaction is passed through to the shared database object.
        /// </summary>
        [TestMethod]
        public void RollbackTransaction_InvokesSharedDB()
        {
            MockSharedDatabase.Setup(db => db.RollbackTransaction()).Verifiable();
            dataAccess.RollbackTransaction();
            MockSharedDatabase.Verify();
        }

        /// <summary>
        /// Proves that CreateSavepont is passed through to the shared database object.
        /// </summary>
        [TestMethod]
        public void CreateSavepoint_InvokesSharedDB()
        {
            MockSharedDatabase.Setup(db => db.CreateSavepoint("Savepoint")).Verifiable();
            dataAccess.CreateSavepoint("Savepoint");
            MockSharedDatabase.Verify();
        }

        /// <summary>
        /// Proves that RollbackToSavepoint is passed through to the shared database object.
        /// </summary>
        [TestMethod]
        public void RollbackToSavepoint_InvokesSharedDB()
        {
            MockSharedDatabase.Setup(db => db.RollbackToSavepoint("Savepoint")).Verifiable();
            dataAccess.RollbackToSavepoint("Savepoint");
            MockSharedDatabase.Verify();
        }
    }
}
