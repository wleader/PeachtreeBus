using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace PeachtreeBus.DataAccessTests
{
    [TestClass]
    public class DeleteSagaFixture :FixtureBase
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
        public async Task DeleteSaga_DeletesTheCorrectSaga()
        {
            var newSaga1 = CreateTestSagaData();
            newSaga1.Key = "1";
            var newSaga2 = CreateTestSagaData();
            newSaga2.Key = "2";

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

        [TestMethod]
        public void DeleteSaga_ThrowsIfSchemaContainsUnsafe()
        {
            var action = new Action(() => dataAccess.DeleteSagaData(DefaultSagaName, string.Empty));
            ActionThrowsIfSchemaContainsPoisonChars(action);
        }

        [TestMethod]
        public void DeleteSaga_ThrowsIfSagaNameContainsUnsafe()
        {
            var action = new Action<string>((s) => dataAccess.DeleteSagaData(s, string.Empty));
            ActionThrowsIfParameterContainsPoisonChars(action);
        }
    }
}
