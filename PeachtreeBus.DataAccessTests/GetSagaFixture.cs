using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace PeachtreeBus.DataAccessTests
{
    [TestClass]
    public class GetSagaFixture : FixtureBase
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
        public async Task GetSagaData_ReturnsUnblockedWhenRowIsNotLocked()
        {
            var newSaga1 = CreateTestSagaData();
            newSaga1.Key = "1";
            
            newSaga1.Id = await dataAccess.Insert(newSaga1, DefaultSagaName);
            
            await Task.Delay(10);
            Assert.AreEqual(1, CountRowsInTable(DefaultSagaTable));

            var actual = await dataAccess.GetSagaData(DefaultSagaName, newSaga1.Key);
            
            AssertSagaEquals(newSaga1, actual);
            Assert.IsFalse(actual.Blocked);
        }

        [TestMethod]
        public async Task GetSagaData_ReturnsNullWhenDoesntExist()
        {
            Assert.AreEqual(0, CountRowsInTable(DefaultSagaTable));
            var sagadata = await dataAccess.GetSagaData(DefaultSagaName, "1");
            Assert.IsNull(sagadata);
        }

        [TestMethod]
        public async Task GetSagaData_ReturnBlockeWhenRowIsLocked()
        {
            var newSaga1 = CreateTestSagaData();
            newSaga1.Key = "1";

            newSaga1.Id = await dataAccess.Insert(newSaga1, DefaultSagaName);

            await Task.Delay(10);
            Assert.AreEqual(1, CountRowsInTable(DefaultSagaTable));

            BeginSecondaryTransaction();
            try
            {
                var data = GetTableContentAndLock(DefaultSagaTable);

                var actual = await dataAccess.GetSagaData(DefaultSagaName, newSaga1.Key);

                Assert.IsTrue(actual.Blocked);
            }
            finally
            {
                RollbackSecondaryTransaction();
            }
        }


        [TestMethod]
        public void GetSagaData_ThrowsIfSchemaContainsUnsafe()
        {
            var action = new Action(() =>  dataAccess.GetSagaData(DefaultSagaName, string.Empty).Wait());
            ActionThrowsIfSchemaContainsPoisonChars(action);
        }

        [TestMethod]
        public void GetSagaData_ThrowsIfSagaNameContainsUnsafe()
        {
            var action = new Action<string>((s) => dataAccess.GetSagaData(s, string.Empty).Wait());
            ActionThrowsIfParameterContainsPoisonChars(action);
        }
    }
}
