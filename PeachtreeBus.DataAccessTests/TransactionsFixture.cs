using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using PeachtreeBus.Data;
using PeachtreeBus.DatabaseSharing;
using System;
using System.Collections.Generic;
using System.Text;

namespace PeachtreeBus.DataAccessTests
{
    [TestClass]
    public class TransactionsFixture
    {
        private Mock<ISharedDatabase> MockSharedDatabase;
        private DapperDataAccess dataAccess;

        [TestInitialize]
        public void TestInitialize()
        {
            MockSharedDatabase = new Mock<ISharedDatabase>();
            dataAccess = new DapperDataAccess(MockSharedDatabase.Object, null);
        }

        [TestMethod]
        public void StartTransaction_InvokesSharedDB()
        {
            MockSharedDatabase.Setup(db => db.BeginTransaction()).Verifiable();
            dataAccess.BeginTransaction();
            MockSharedDatabase.Verify();
        }

        [TestMethod]
        public void CommitTransaction_InvokesSharedDB()
        {
            MockSharedDatabase.Setup(db => db.CommitTransaction()).Verifiable();
            dataAccess.CommitTransaction();
            MockSharedDatabase.Verify();
        }

        [TestMethod]
        public void RollbackTransaction_InvokesSharedDB()
        {
            MockSharedDatabase.Setup(db => db.RollbackTransaction()).Verifiable();
            dataAccess.RollbackTransaction();
            MockSharedDatabase.Verify();
        }

        [TestMethod]
        public void CreateSavepoint_InvokesSharedDB()
        {
            MockSharedDatabase.Setup(db => db.CreateSavepoint("Savepoint")).Verifiable();
            dataAccess.CreateSavepoint("Savepoint");
            MockSharedDatabase.Verify();
        }

        [TestMethod]
        public void RollbackToSavepoint_InvokesSharedDB()
        {
            MockSharedDatabase.Setup(db => db.RollbackToSavepoint("Savepoint")).Verifiable();
            dataAccess.RollbackToSavepoint("Savepoint");
            MockSharedDatabase.Verify();
        }
    }
}
