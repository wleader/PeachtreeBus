using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Threading.Tasks;
using PeachtreeBus.Core.Tests;
using PeachtreeBus.Sagas;

namespace PeachtreeBus.DataAccessTests;

public abstract class SagaGetFixture : BusDataAccessFixtureBase
{
    [TestInitialize]
    public override void Initialize() => base.Initialize();

    [TestCleanup]
    public override void Cleanup() => base.Cleanup();

    [TestMethod]
    public async Task Given_NotLockedSagaDataRow_When_GetSagaData_Then_ResultIsNotBlocked()
    {
        var newSaga1 = TestData.CreateSagaData(sagaKey: new("1"));
        newSaga1.Id = await BusDataAccess.InsertSagaData(newSaga1, TestConfig.DefaultSagaName);

        await Task.Delay(10);
        TestDataAccess.Then_TableHasCount(TestConfig.SagaData, 1);

        BusDataAccess.BeginTransaction();
        try
        {
            var actual = await BusDataAccess.GetSagaData(TestConfig.DefaultSagaName, newSaga1.Key);
            Assert.IsNotNull(actual);
            DataAssert.AreEqual(newSaga1, actual);
            Assert.IsFalse(actual.Blocked);
            
            // check that getting the saga data locked it.
            using var notLockedRows = TestDataAccess.LockRows<SagaData>(TestConfig.SagaData);
            Assert.IsEmpty(notLockedRows.Data);
        }
        finally
        {
            BusDataAccess.RollbackTransaction();
        } 
    }

    [TestMethod]
    public async Task Given_RowNotInTable_When_GetSagaData_Then_ResultIsNull()
    {
        TestDataAccess.Then_TableIsEmpty(TestConfig.SagaData);
        var actual = await BusDataAccess.GetSagaData(TestConfig.DefaultSagaName, new("1"));
        Assert.IsNull(actual);
    }

    [TestMethod]
    public async Task Given_RowInTable_And_RowIsLocked_When_GetSagaData_Then_ResultIsBlocked()
    {
        var newSaga1 = TestData.CreateSagaData(sagaKey: new("1"));
        newSaga1.Id = await BusDataAccess.InsertSagaData(newSaga1, TestConfig.DefaultSagaName);

        await Task.Delay(10);
        
        // lock the saga data row
        using var locked = TestDataAccess.LockRows<SagaData>(TestConfig.SagaData);

        var actual = await BusDataAccess.GetSagaData(TestConfig.DefaultSagaName, newSaga1.Key);
        Assert.IsNotNull(actual);
        Assert.IsTrue(actual.Blocked);
    }
}