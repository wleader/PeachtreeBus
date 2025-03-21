﻿using Microsoft.VisualStudio.TestTools.UnitTesting;
using PeachtreeBus.Subscriptions;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace PeachtreeBus.DataAccessTests
{
    /// <summary>
    /// Proves the behavior of DapperDataAccess.ExpireSubscriptions
    /// </summary>
    [TestClass]
    public class ExpireSubscriptionsFixture : DapperDataAccessFixtureBase
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
        /// Proves that expired rows are deleted.
        /// </summary>
        /// <returns></returns>
        [TestMethod]
        public async Task ExpireSubscriptions_DeletesCorrectRows()
        {
            // assumes subscribe is working correctly

            var subscriber1 = SubscriberId.New();
            var subscriber2 = SubscriberId.New();
            var subscriber3 = SubscriberId.New();
            var subscriber4 = SubscriberId.New();
            var topic1 = new Topic("TestTopic1");
            var topic2 = new Topic("TestTopic2");
            var valid = DateTime.UtcNow.AddHours(1);
            var expired = DateTime.UtcNow.AddSeconds(-1);

            await dataAccess.Subscribe(subscriber1, topic1, valid);
            await dataAccess.Subscribe(subscriber2, topic2, valid);
            await dataAccess.Subscribe(subscriber3, topic1, expired);
            await dataAccess.Subscribe(subscriber4, topic2, expired);

            await dataAccess.ExpireSubscriptions(100);

            var rows = GetSubscriptions();

            Assert.AreEqual(2, rows.Count);
            Assert.IsNotNull(rows.Single(r => r.SubscriberId == subscriber1 && r.Topic == topic1));
            Assert.IsNotNull(rows.Single(r => r.SubscriberId == subscriber2 && r.Topic == topic2));
        }
    }
}
