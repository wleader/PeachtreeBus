using Microsoft.VisualStudio.TestTools.UnitTesting;
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
            newSaga1.Key = new("1");

            newSaga1.Id = await dataAccess.InsertSagaData(newSaga1, DefaultSagaName);

            await Task.Delay(10);
            Assert.AreEqual(1, CountRowsInTable(SagaData));

            var actual = await dataAccess.GetSagaData(DefaultSagaName, newSaga1.Key);
            Assert.IsNotNull(actual);
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
            Assert.AreEqual(0, CountRowsInTable(SagaData));
            var sagadata = await dataAccess.GetSagaData(DefaultSagaName, new("1"));
            Assert.IsNull(sagadata);
        }

        /// <summary>
        /// proves that blocked is true, when the row is locked.
        /// </summary>
        /// <returns></returns>
        [TestMethod]
        public async Task GetSagaData_ReturnsBlockedWhenRowIsLocked()
        {
            var newSaga1 = CreateTestSagaData();
            newSaga1.Key = new("1");

            newSaga1.Id = await dataAccess.InsertSagaData(newSaga1, DefaultSagaName);

            await Task.Delay(10);
            Assert.AreEqual(1, CountRowsInTable(SagaData));

            // lock the saga data row
            using var data = new RowLock(SagaData);

            var actual = await dataAccess.GetSagaData(DefaultSagaName, newSaga1.Key);
            Assert.IsNotNull(actual);
            Assert.IsTrue(actual.Blocked);
        }
    }
}
