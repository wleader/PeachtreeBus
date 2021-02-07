using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace PeachtreeBus.DataAccessTests
{
    [TestClass]
    public class GetOnePendingMessageFixture : FixtureBase
    {
        [TestInitialize]
        public new void TestInitialize()
        {
            base.TestInitialize();
        }

        [TestMethod]
        public async Task GetOnePendingMessage_GetsMessage()
        {
            TruncateAll();

            // Add one message;
            var testMessage = CreateTestMessage();
            testMessage.Id = await dataAccess.EnqueueMessage(testMessage, DefaultQueue);
            
            var actual = await dataAccess.GetOnePendingMessage(DefaultQueue);

            AssertMessageEquals(testMessage, actual);
        }

        [TestMethod]
        public async Task GetOnePendingMessage_LocksTheMessage()
        {
            TruncateAll();

            // Add two messages;
            var testMessage1 = CreateTestMessage();
            testMessage1.Id = await dataAccess.EnqueueMessage(testMessage1, DefaultQueue);
            var testMessage2 = CreateTestMessage();
            testMessage2.Id = await dataAccess.EnqueueMessage(testMessage2, DefaultQueue);

            // get a message and leave the transaction open.
            dataAccess.BeginTransaction();
            var actual = await dataAccess.GetOnePendingMessage(DefaultQueue);

            BeginSecondaryTransaction();
            var unlockedMessages = GetTableContentAndLock(PendingMessagesTable).ToMessages();

            Assert.AreEqual(1, unlockedMessages.Count);
            Assert.AreNotEqual(testMessage1.Id, testMessage2.Id);
            Assert.AreNotEqual(actual.Id, unlockedMessages[0].Id);

            RollbackSecondaryTransaction();
            dataAccess.RollbackTransaction();
        }
    }
}
