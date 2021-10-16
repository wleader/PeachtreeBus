using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace PeachtreeBus.DataAccessTests
{
    [TestClass]
    public class UpdateMessageFixture : FixtureBase
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
        public async Task UpdateMessage_UpdatesPendingTable()
        {
            // Add two messages;
            var testMessage1 = CreateTestMessage();
            testMessage1.Id = await dataAccess.EnqueueMessage(testMessage1, DefaultQueue);
            var testMessage2 = CreateTestMessage();
            testMessage2.Id = await dataAccess.EnqueueMessage(testMessage2, DefaultQueue);
            await Task.Delay(10); // wait for the rows to be ready

            // get and update a message.
            var toUpdate = await dataAccess.GetOnePendingMessage(DefaultQueue);
            
            // set changed values
            toUpdate.MessageId = Guid.NewGuid(); // this should never persist a change.
            toUpdate.Enqueued = toUpdate.Enqueued.AddMinutes(-1); // this should never change.
            toUpdate.Body = "Changed Body"; // should never change.
            toUpdate.Headers = "Changed Headers";
            toUpdate.NotBefore = toUpdate.NotBefore.AddMinutes(1);
            toUpdate.Completed = DateTime.UtcNow;
            toUpdate.Failed = DateTime.UtcNow;
            toUpdate.Retries = 10;

            await dataAccess.Update(toUpdate, DefaultQueue);
            await Task.Delay(10); // wait for the rows to be ready

            // Check that it ended up in the error table.
            var pending = GetTableContent(PendingMessagesTable).ToMessages();
            Assert.AreEqual(2, pending.Count);

            var expectUnchanged = toUpdate.Id == testMessage1.Id ? testMessage2 : testMessage1;
            var changedOriginal = toUpdate.Id != testMessage1.Id ? testMessage2 : testMessage1;

            var actualUnchanged = pending.Single(m => m.Id != toUpdate.Id);
            AssertMessageEquals(expectUnchanged, actualUnchanged);

            var actualChanged = pending.Single(m => m.Id == toUpdate.Id);
            // compare the unchangable fields.
            Assert.AreEqual(changedOriginal.Id, actualChanged.Id);
            Assert.AreEqual(changedOriginal.MessageId, actualChanged.MessageId);
            AssertSqlDbDateTime(changedOriginal.Enqueued, actualChanged.Enqueued);
            Assert.AreEqual(changedOriginal.Body, actualChanged.Body);
            // compare the changeable fields.
            Assert.AreEqual(toUpdate.Headers, actualChanged.Headers);
            AssertSqlDbDateTime(toUpdate.NotBefore, actualChanged.NotBefore);
            AssertSqlDbDateTime(toUpdate.Completed, actualChanged.Completed);
            AssertSqlDbDateTime(toUpdate.Failed, actualChanged.Failed);
            Assert.AreEqual(toUpdate.Retries, actualChanged.Retries);

        }
    }
}
