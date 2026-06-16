using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using PeachtreeBus.Data;

namespace PeachtreeBus.Core.Tests.Data;

public abstract class BusDataAccessPassthroughFixtureBase<TBusDataAccess>
    : BusDataAccessUnitTestBase<TBusDataAccess>
    where TBusDataAccess : IBusDataAccess
{
    [TestMethod]
    public void Reset_CallsReconnect()
    {
        DataAccess.Reconnect();
        SharedDatabase.Verify(x => x.Reconnect(), Times.Once());
    }
    
    [TestMethod]
    public void StartTransaction_InvokesSharedDB()
    {
        DataAccess.BeginTransaction();
        SharedDatabase.Verify(x => x.BeginTransaction(), Times.Once());
    }

    [TestMethod]
    public void CommitTransaction_InvokesSharedDB()
    {
        DataAccess.CommitTransaction();
        SharedDatabase.Verify(x => x.CommitTransaction(), Times.Once());
    }

    [TestMethod]
    public void RollbackTransaction_InvokesSharedDB()
    {
        DataAccess.RollbackTransaction();
        SharedDatabase.Verify(x => x.RollbackTransaction(), Times.Once());
    }

    [TestMethod]
    public void CreateSavepoint_InvokesSharedDB()
    {
        DataAccess.CreateSavepoint("Savepoint");
        SharedDatabase.Verify(x => x.CreateSavepoint("Savepoint"), Times.Once());
    }

    [TestMethod]
    public void RollbackToSavepoint_InvokesSharedDB()
    {
        DataAccess.RollbackToSavepoint("Savepoint");
        SharedDatabase.Verify(x => x.RollbackToSavepoint("Savepoint"), Times.Once());
    }
}