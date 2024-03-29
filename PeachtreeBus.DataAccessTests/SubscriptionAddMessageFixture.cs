﻿using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Threading.Tasks;

namespace PeachtreeBus.DataAccessTests
{
    /// <summary>
    /// Proves the behavior of DapperDataAccess.AddMessage (subscribed)
    /// </summary>
    [TestClass]
    public class SubscriptionAddMessageFixture : DapperDataAccessFixtureBase
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
        /// proves the row is inserted.
        /// </summary>
        /// <returns></returns>
        [TestMethod]
        public async Task AddMessage_StoresTheMessage()
        {
            Assert.AreEqual(0, CountRowsInTable(SubscribedPendingTable));

            var newMessage = CreateSubscribed();
            newMessage.SubscriberId = Guid.NewGuid();

            newMessage.Id = await dataAccess.AddMessage(newMessage);

            Assert.IsTrue(newMessage.Id > 0);

            var data = GetTableContent(SubscribedPendingTable);
            Assert.IsNotNull(data);

            var messages = data.ToSubscribed();
            Assert.AreEqual(1, messages.Count);

            AssertSubscribedEquals(newMessage, messages[0]);
        }

        /// <summary>
        /// Proves that unsafe schema is not allowed.
        /// </summary>
        [TestMethod]
        public async Task AddMessage_ThrowsIfSchemaContainsUnsafe()
        {
            var newMessage = CreateSubscribed();
            newMessage.SubscriberId = Guid.NewGuid();
            var action = new Func<Task>(() => dataAccess.AddMessage(newMessage));
            await ActionThrowsIfSchemaContainsPoisonChars(action);
        }

        /// <summary>
        /// proves that NotBefore must specify DateTimeKind
        /// </summary>
        [TestMethod]
        public async Task AddMessage_ThowsIfNotBeforeDateTimeKindUnspecified()
        {
            var newMessage = CreateSubscribed();
            newMessage.SubscriberId = Guid.NewGuid();
            newMessage.NotBefore = DateTime.SpecifyKind(newMessage.NotBefore, DateTimeKind.Unspecified);
            var action = new Func<Model.SubscribedMessage, Task>(async (m) => await dataAccess.AddMessage(m));
            await ActionThrowsFor(action, newMessage);
        }

        /// <summary>
        /// Proves that ValidUntil must specify DateTimeKind
        /// </summary>
        [TestMethod]
        public async Task AddMessage_ThrowsIfValidUntilDateTimeKindUnspecified()
        {
            var newMessage = CreateSubscribed();
            newMessage.SubscriberId = Guid.NewGuid();
            newMessage.ValidUntil = DateTime.SpecifyKind(newMessage.ValidUntil, DateTimeKind.Unspecified);
            var action = new Func<Model.SubscribedMessage, Task>(async (m) => await dataAccess.AddMessage(m));
            await ActionThrowsFor(action, newMessage);
        }

        /// <summary>
        /// Proves that subscriber ID cannot be empty.
        /// </summary>
        [TestMethod]
        public async Task AddMessage_ThrowsIfSubscriberIdIsGuidEmpty()
        {
            var newMessage = CreateSubscribed();
            newMessage.SubscriberId = Guid.Empty;
            var action = new Func<Model.SubscribedMessage, Task>(async (m) => await dataAccess.AddMessage(m));
            await ActionThrowsFor(action, newMessage);
        }
    }
}
