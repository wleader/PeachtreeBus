using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Threading.Tasks;

namespace PeachtreeBus.DataAccessTests
{
    /// <summary>
    /// Proves the behavior of DapperDataAccess.DeleteSagaData
    /// </summary>
    [TestClass]
    public class SagaDeleteFixture : MsSqlBusDataAccessFixtureBase
    {
        [TestInitialize]
        public override void Initialize() => base.Initialize();

        [TestCleanup]
        public override void Cleanup() => base.Cleanup();


        /// <summary>
        /// Proves that the correct row is deleted.
        /// </summary>
        /// <returns></returns>
        [TestMethod]
        public async Task DeleteSaga_DeletesTheCorrectSaga()
        {
            var newSaga1 = CreateTestSagaData();
            newSaga1.Key = new("1");
            var newSaga2 = CreateTestSagaData();
            newSaga2.Key = new("2");

            newSaga1.Id = await BusDataAccess.InsertSagaData(newSaga1, TestConfig.DefaultSagaName);
            newSaga2.Id = await BusDataAccess.InsertSagaData(newSaga2,  TestConfig.DefaultSagaName);

            await Task.Delay(10);
            Assert.AreEqual(2, CountRowsInTable( TestConfig.SagaData));

            await BusDataAccess.DeleteSagaData( TestConfig.DefaultSagaName, newSaga1.Key);
            await Task.Delay(10);

            var sagas = GetTableContent( TestConfig.SagaData).ToSagas();
            Assert.AreEqual(1, sagas.Count);

            AssertSagaEquals(newSaga2, sagas[0]);
        }
    }
}
