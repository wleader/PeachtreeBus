using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Threading.Tasks;

namespace PeachtreeBus.DataAccessTests
{
    /// <summary>
    /// Proves the behavior of DapperDataAccess.GetSagaData
    /// </summary>
    [TestClass]
    public class SagaGetFixture : DapperDataAccessFixtureBase
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
        /// Proves that that Blocked is set correctly when the row is not locked.
        /// </summary>
        /// <returns></returns>
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

        /// <summary>
        /// Proves that null is returned when matching row is not found.
        /// </summary>
        /// <returns></returns>
        [TestMethod]
        public async Task GetSagaData_ReturnsNullWhenDoesntExist()
        {
            Assert.AreEqual(0, CountRowsInTable(DefaultSagaTable));
            var sagadata = await dataAccess.GetSagaData(DefaultSagaName, "1");
            Assert.IsNull(sagadata);
        }

        /// <summary>
        /// proves that blocked is true, when the row is locked.
        /// </summary>
        /// <returns></returns>
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

        /// <summary>
        /// Proves that unsafe schema is not allowed
        /// </summary>
        [TestMethod]
        public async Task GetSagaData_ThrowsIfSchemaContainsUnsafe()
        {
            var action = new Func<Task>(async () => await dataAccess.GetSagaData(DefaultSagaName, string.Empty));
            await ActionThrowsIfSchemaContainsPoisonChars(action);
        }

        /// <summary>
        /// Proves tha unsafe saga names are not allowed.
        /// </summary>
        [TestMethod]
        public async Task GetSagaData_ThrowsIfSagaNameContainsUnsafe()
        {
            var action = new Func<string, Task>(async (s) => await dataAccess.GetSagaData(s, string.Empty));
            await ActionThrowsIfParameterContainsPoisonChars(action);
        }
    }
}
