﻿using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace PeachtreeBus.DataAccessTests
{
    [TestClass]
    public class InsertSagaFixture : FixtureBase
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

        [TestMethod]
        public void InsertSaga_ThrowsIfSchemaContainsUnsafe()
        {
            var action = new Action(() => dataAccess.Insert(new Model.SagaData(), DefaultSagaName));
            ActionThrowsIfSchemaContainsPoisonChars(action);
        }

        [TestMethod]
        public void InsertSaga_ThrowsIfSagaNameContainsUnsafe()
        {
            var action = new Action<string>((s) => dataAccess.Insert(new Model.SagaData(), s));
            ActionThrowsIfParameterContainsPoisonChars(action);
        }
    }
}
