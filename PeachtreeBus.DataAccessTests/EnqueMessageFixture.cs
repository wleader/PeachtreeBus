using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using PeachtreeBus.Data;
using PeachtreeBus.DatabaseSharing;
using System;
using System.Data.SqlClient;
using System.Threading.Tasks;

namespace PeachtreeBus.DataAccessTests
{
    [TestClass]
    public class EnqueMessageFixture : FixtureBase
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
        public async Task EnqueueMessage_StoresTheMessage()
        {
            TruncateAll();

            var newMessage = CreateTestMessage();

            Assert.AreEqual(0, CountRowsInTable(PendingMessagesTable));

            newMessage.Id = await dataAccess.EnqueueMessage(newMessage, DefaultQueue);

            Assert.IsTrue(newMessage.Id > 0);

            var data = GetTableContent(PendingMessagesTable);
            Assert.IsNotNull(data);

            var messages = data.ToMessages();
            Assert.AreEqual(1, messages.Count);

            AssertMessageEquals(newMessage, messages[0]);
        }

        [TestMethod]
        public void EnqueueMessage_ThrowsIfDateTimeKindUnspecified()
        {
            var action = new Action<Model.QueueMessage>((m) => dataAccess.EnqueueMessage(m, DefaultQueue));
            ActionThrowsForMessagesWithUnspecifiedDateTimeKinds(action);
        }

        [TestMethod]
        public void EnqueueMessage_ThrowsIfSchemaContainsUnsafe()
        {
            var action = new Action(() => dataAccess.EnqueueMessage(new Model.QueueMessage(), DefaultQueue));
            ActionThrowsIfSchemaContainsPoisonChars(action);
        }

        [TestMethod]
        public void EnqueueMessage_ThrowsIfQueueNameContainsUnsafe()
        {
            var action = new Action<string>((s) => dataAccess.EnqueueMessage(new Model.QueueMessage(), s));
            ActionThrowsIfParameterContainsPoisonChars(action);
        }
    }
}
