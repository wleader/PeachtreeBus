using Microsoft.VisualStudio.TestTools.UnitTesting;
using PeachtreeBus.Core.Tests;
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
            var newMessage = TestData.CreateQueueData();

            Assert.AreEqual(0, CountRowsInTable(QueuePending));

            newMessage.Id = await dataAccess.AddMessage(newMessage, DefaultQueue);

            Assert.IsTrue(newMessage.Id.Value > 0);

            var data = GetTableContent(QueuePending);
            Assert.IsNotNull(data);

            var messages = data.ToMessages();
            Assert.AreEqual(1, messages.Count);

            AssertQueueDataAreEqual(newMessage, messages[0]);
        }
    }
}
