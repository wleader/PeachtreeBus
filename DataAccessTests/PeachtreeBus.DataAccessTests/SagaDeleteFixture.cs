using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Threading.Tasks;
using PeachtreeBus.Core.Tests;
using PeachtreeBus.Sagas;

namespace PeachtreeBus.DataAccessTests;

public abstract class SagaDeleteFixture : BusDataAccessFixtureBase
{
    [TestInitialize]
    public override void Initialize() => base.Initialize();

    [TestCleanup]
    public override void Cleanup() => base.Cleanup();

    [TestMethod]
    public async Task DeleteSaga_DeletesTheCorrectSaga()
    {
        var newSaga1 = TestData.CreateSagaData(sagaKey: new("1"));
        var newSaga2 = TestData.CreateSagaData(sagaKey: new("2"));

        newSaga1.Id = await BusDataAccess.InsertSagaData(newSaga1, TestConfig.DefaultSagaName);
        newSaga2.Id = await BusDataAccess.InsertSagaData(newSaga2,  TestConfig.DefaultSagaName);

        await Task.Delay(10);
        TestDataAccess.Then_TableHasCount(TestConfig.SagaData, 2);

        await BusDataAccess.DeleteSagaData( TestConfig.DefaultSagaName, newSaga1.Key);
        await Task.Delay(10);

        var sagas = TestDataAccess.GetTableContent<SagaData>(TestConfig.SagaData);
        Assert.AreEqual(1, sagas.Count);
        DataAssert.AreEqual(newSaga2, sagas[0]);
    }
}