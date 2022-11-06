using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Threading.Tasks;

namespace PeachtreeBus.DataAccessTests
{
    /// <summary>
    /// Proves the behavior or DapperDataAccess.AddMessage
    /// </summary>
    [TestClass]
    public class QueueMessageAddFixture : FixtureBase
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
        /// Proves the message is inserted into the table.
        /// </summary>
        /// <returns></returns>
        [TestMethod]
        public async Task AddMessage_StoresTheMessage()
        {
            var newMessage = CreateQueueMessage();

            Assert.AreEqual(0, CountRowsInTable(QueuePendingTable));

            newMessage.Id = await dataAccess.AddMessage(newMessage, DefaultQueue);

            Assert.IsTrue(newMessage.Id > 0);

            var data = GetTableContent(QueuePendingTable);
            Assert.IsNotNull(data);

            var messages = data.ToMessages();
            Assert.AreEqual(1, messages.Count);

            AssertMessageEquals(newMessage, messages[0]);
        }

        /// <summary>
        /// Proves that NotBefore must specify DateTimeKind.
        /// </summary>
        [TestMethod]
        public async Task AddMessage_ThrowsIfDateTimeKindUnspecified()
        {
            var action = new Func<Model.QueueMessage, Task>(async (m) => await dataAccess.AddMessage(m, DefaultQueue));

            // we check the not-before because not-before is the only time
            // parameter used by Enqueue message.
            var poisonNotBefore = CreateQueueMessage();
            poisonNotBefore.NotBefore = DateTime.SpecifyKind(poisonNotBefore.NotBefore, DateTimeKind.Unspecified);
            await ActionThrowsFor(action, poisonNotBefore);
        }

        /// <summary>
        /// Proves that statements do not execute if Schema contains 
        /// characters that are an SQL injection risk.
        /// </summary>
        [TestMethod]
        public async Task AddMessage_ThrowsIfSchemaContainsUnsafe()
        {
            var action = new Func<Task>(async () => await dataAccess.AddMessage(new Model.QueueMessage(), DefaultQueue));
            await ActionThrowsIfSchemaContainsPoisonChars(action);
        }

        /// <summary>
        /// Proves that statements do not execute if queue name contains
        /// character that are an SQL injection risk
        /// </summary>
        [TestMethod]
        public async Task AddMessage_ThrowsIfQueueNameContainsUnsafe()
        {
            var action = new Func<string, Task>(async (s) => await dataAccess.AddMessage(new Model.QueueMessage(), s));
            await ActionThrowsIfParameterContainsPoisonChars(action);
        }
    }
}
