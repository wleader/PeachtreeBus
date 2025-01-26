using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Threading.Tasks;

namespace PeachtreeBus.DataAccessTests
{
    /// <summary>
    /// Proves the behavior of DapperDataAccess.DeleteSagaData
    /// </summary>
    [TestClass]
    public class SagaDeleteFixture : DapperDataAccessFixtureBase
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

            newSaga1.Id = await dataAccess.Insert(newSaga1, DefaultSagaName);
            newSaga2.Id = await dataAccess.Insert(newSaga2, DefaultSagaName);

            await Task.Delay(10);
            Assert.AreEqual(2, CountRowsInTable(DefaultSagaTable));

            await dataAccess.DeleteSagaData(DefaultSagaName, newSaga1.Key);
            await Task.Delay(10);

            var sagas = GetTableContent(DefaultSagaTable).ToSagas();
            Assert.AreEqual(1, sagas.Count);

            AssertSagaEquals(newSaga2, sagas[0]);
        }
    }
}
