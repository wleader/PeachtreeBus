using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PeachtreeBus.DataAccessTests
{
    [TestClass]
    public class UpdateSagaFixture : FixtureBase
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

        [TestMethod]
        public async Task UpdateSaga_Updates()
        {
            var newSaga1 = CreateTestSagaData();
            newSaga1.Key = "1";
            var newSaga2 = CreateTestSagaData();
            newSaga2.Key = "2";

            newSaga1.Id = await dataAccess.Insert(newSaga1, DefaultSagaName);
            newSaga2.Id = await dataAccess.Insert(newSaga2, DefaultSagaName);

            await Task.Delay(10);
            Assert.AreEqual(2, CountRowsInTable(DefaultSagaTable));

            var updatedSaga = new Model.SagaData
            {
                Id = newSaga1.Id,
                Blocked = true, // doesn't actually get stored.
                Data = "NewData", // check this gets updated
                Key = "NewKey", // check this doesn't update
                SagaId = Guid.NewGuid() // check this doesn't update
            };

            await dataAccess.Update(updatedSaga, DefaultSagaName );
            await Task.Delay(10);

            var sagas = GetTableContent(DefaultSagaTable).ToSagas();
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

        [TestMethod]
        public void UpdateSaga_ThrowsIfSchemaContainsUnsafe()
        {
            var action = new Action(() => dataAccess.Update(new Model.SagaData(), DefaultSagaName));
            ActionThrowsIfSchemaContainsPoisonChars(action);
        }

        [TestMethod]
        public void UpdateSaga_ThrowsIfSagaNameContainsUnsafe()
        {
            var action = new Action<string>((s) => dataAccess.Update(new Model.SagaData(), s));
            ActionThrowsIfParameterContainsPoisonChars(action);
        }
    }
}
