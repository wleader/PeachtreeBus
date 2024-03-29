﻿using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Threading.Tasks;

namespace PeachtreeBus.DataAccessTests
{
    /// <summary>
    /// Proves the behavior of DapperDataAccess.Insert saga
    /// </summary>
    [TestClass]
    public class SagaInsertFixture : DapperDataAccessFixtureBase
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
        /// Proves the data is inserted correctly.
        /// </summary>
        /// <returns></returns>
        [TestMethod]
        public async Task InsertSaga_StoresTheData()
        {
            var newSaga = CreateTestSagaData();

            Assert.AreEqual(0, CountRowsInTable(DefaultSagaTable));

            newSaga.Id = await dataAccess.Insert(newSaga, DefaultSagaName);

            Assert.IsTrue(newSaga.Id > 0);

            var data = GetTableContent(DefaultSagaTable);
            Assert.IsNotNull(data);

            var sagas = data.ToSagas();
            Assert.AreEqual(1, sagas.Count);

            AssertSagaEquals(newSaga, sagas[0]);
        }

        /// <summary>
        /// proves that unsafe schema are not allowed.
        /// </summary>
        [TestMethod]
        public async Task InsertSaga_ThrowsIfSchemaContainsUnsafe()
        {
            var action = new Func<Task>(() => dataAccess.Insert(new Model.SagaData(), DefaultSagaName));
            await ActionThrowsIfSchemaContainsPoisonChars(action);
        }

        /// <summary>
        /// proves that unsafe schema names are not allowed.
        /// </summary>
        [TestMethod]
        public async Task InsertSaga_ThrowsIfSagaNameContainsUnsafe()
        {
            var action = new Func<string, Task>((s) => dataAccess.Insert(new Model.SagaData(), s));
            await ActionThrowsIfParameterContainsPoisonChars(action);
        }
    }
}
