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
        public new void TestInitialize()
        {
            base.TestInitialize();
        }

        [TestMethod]
        public async Task EnqueueMessage_StoresTheMessage()
        {
            TruncateAll();

            var newMessage = new Model.QueueMessage
            {
                Body = "Body",
                Completed = null,
                Failed = null,
                Enqueued = DateTime.UtcNow,
                Headers = "Headers",
                MessageId = Guid.NewGuid(),
                NotBefore = DateTime.UtcNow.AddMinutes(1),
                Retries = 0
            };

            Assert.AreEqual(0, CountRowsInTable("QueueName_PendingMessages"));

            newMessage.Id = await dataAccess.EnqueueMessage(newMessage, "QueueName");

            Assert.IsTrue(newMessage.Id > 0);
            
            var data = GetTableContent("QueueName_PendingMessages");
            Assert.IsNotNull(data); 
            
            var messages = data.ToMessages();
            Assert.AreEqual(1, messages.Count);

            AssertMessageEquals(newMessage, messages[0]);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public async Task EnqueueMessage_ThrowsIfEnqueuedKindUnspecified()
        {
            var newMessage = new Model.QueueMessage
            {
                Body = "Body",
                Completed = null,
                Failed = null,
                Enqueued = DateTime.SpecifyKind(DateTime.Now, DateTimeKind.Unspecified),
                Headers = "Headers",
                MessageId = Guid.NewGuid(),
                NotBefore = DateTime.UtcNow.AddMinutes(1),
                Retries = 0
            };

            await dataAccess.EnqueueMessage(newMessage, "QueueName");
        }
    }
}
