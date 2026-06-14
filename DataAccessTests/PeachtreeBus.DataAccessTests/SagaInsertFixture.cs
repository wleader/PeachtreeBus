using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Threading.Tasks;
using PeachtreeBus.Core.Tests;
using PeachtreeBus.Sagas;

namespace PeachtreeBus.DataAccessTests;

public abstract class SagaInsertFixture : BusDataAccessFixtureBase
{
    [TestInitialize]
    public override Task Initialize() => base.Initialize();

    [TestCleanup]
    public override Task Cleanup() => base.Cleanup();

    /// <summary>
    /// Proves the data is inserted correctly.
    /// </summary>
    /// <returns></returns>
    [TestMethod]
    public async Task InsertSaga_StoresTheData()
    {
        var newSaga = TestData.CreateSagaData();

        await TestDataAccess.Then_TableIsEmpty(TestConfig.SagaData);

        newSaga.Id = await BusDataAccess.InsertSagaData(newSaga, TestConfig.DefaultSagaName);

        Assert.IsTrue(newSaga.Id.Value > 0);

        var sagas = await TestDataAccess.GetTableContent<SagaData>(TestConfig.SagaData);
        Assert.AreEqual(1, sagas.Count);

        DataAssert.AreEqual(newSaga, sagas[0]);
    }
}