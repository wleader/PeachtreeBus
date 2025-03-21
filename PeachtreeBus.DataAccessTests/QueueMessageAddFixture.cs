using Microsoft.VisualStudio.TestTools.UnitTesting;
using PeachtreeBus.Abstractions.Tests;
using PeachtreeBus.Tests;
using System.Threading.Tasks;

namespace PeachtreeBus.DataAccessTests
{
    /// <summary>
    /// Proves the behavior or DapperDataAccess.AddMessage
    /// </summary>
    [TestClass]
    public class QueueMessageAddFixture : DapperDataAccessFixtureBase
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
            var newMessage = TestData.CreateQueueMessage();

            Assert.AreEqual(0, CountRowsInTable(QueuePendingTable));

            newMessage.Id = await dataAccess.AddMessage(newMessage, DefaultQueue);

            Assert.IsTrue(newMessage.Id.Value > 0);

            var data = GetTableContent(QueuePendingTable);
            Assert.IsNotNull(data);

            var messages = data.ToMessages();
            Assert.AreEqual(1, messages.Count);

            AssertQueueDataAreEqual(newMessage, messages[0]);
        }
    }
}
