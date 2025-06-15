using Microsoft.VisualStudio.TestTools.UnitTesting;
using PeachtreeBus.Core.Tests;
using PeachtreeBus.Data;
using PeachtreeBus.Sagas;
using System.Linq;
using System.Threading.Tasks;

namespace PeachtreeBus.DataAccessTests
{
    /// <summary>
    /// Proves the behavior of DapperDataAccess.Update saga
    /// </summary>
    [TestClass]
    public class SagaUpdateFixture : DapperDataAccessFixtureBase
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
        /// Proves that the correct row is updated and that only changable fields can change.
        /// </summary>
        /// <returns></returns>
        [TestMethod]
        public async Task UpdateSaga_Updates()
        {
            var newSaga1 = CreateTestSagaData();
            newSaga1.Key = new("1");
            var newSaga2 = CreateTestSagaData();
            newSaga2.Key = new("2");

            newSaga1.Id = await dataAccess.InsertSagaData(newSaga1, DefaultSagaName);
            newSaga2.Id = await dataAccess.InsertSagaData(newSaga2, DefaultSagaName);

            await Task.Delay(10);
            Assert.AreEqual(2, CountRowsInTable(SagaData));

            var updatedSaga = new SagaData
            {
                Id = newSaga1.Id,
                Blocked = true, // doesn't actually get stored.
                Data = new("NewData"), // check this gets updated
                Key = new("NewKey"), // check this doesn't update
                SagaId = UniqueIdentity.New(), // check this doesn't update
                MetaData = TestData.CreateSagaMetaData(),
            };

            await dataAccess.UpdateSagaData(updatedSaga, DefaultSagaName);
            await Task.Delay(10);

            var sagas = GetTableContent(SagaData).ToSagas();
            Assert.AreEqual(2, sagas.Count);

            var actualSaga1 = sagas.Single(s => s.Id == newSaga1.Id);
            var actualSaga2 = sagas.Single(s => s.Id == newSaga2.Id);

            // saga 2 should be unchanged.
            AssertSagaEquals(newSaga2, actualSaga2);

            // Check that saga1 changes in the right ways.

            Assert.AreEqual(newSaga1.Key, actualSaga1.Key); // key shouldn't change.
            Assert.AreEqual(newSaga1.SagaId, actualSaga1.SagaId); // SagaId Shouldn't change
            Assert.AreEqual(updatedSaga.Data, actualSaga1.Data); // Data should change
        }
    }
}
