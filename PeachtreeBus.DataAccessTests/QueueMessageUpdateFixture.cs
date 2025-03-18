using Microsoft.VisualStudio.TestTools.UnitTesting;
using PeachtreeBus.Data;
using PeachtreeBus.Tests;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace PeachtreeBus.DataAccessTests
{
    /// <summary>
    /// Proves the behavior of DapperDataAccess.Update
    /// </summary>
    [TestClass]
    public class QueueMessageUpdateFixture : DapperDataAccessFixtureBase
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
        /// Proves that the pending message is updated.
        /// Proves that only the columns that are allowed to change are changed.
        /// </summary>
        /// <returns></returns>
        [TestMethod]
        public async Task Update_UpdatesPendingTable()
        {
            // Add two messages;
            var testMessage1 = TestData.CreateQueueMessage();
            testMessage1.Id = await dataAccess.AddMessage(testMessage1, DefaultQueue);
            var testMessage2 = TestData.CreateQueueMessage();
            testMessage2.Id = await dataAccess.AddMessage(testMessage2, DefaultQueue);
            await Task.Delay(10); // wait for the rows to be ready

            // get and update a message.
            var toUpdate = await dataAccess.GetPendingQueued(DefaultQueue);
            Assert.IsNotNull(toUpdate);
            // set changed values
            toUpdate.MessageId = UniqueIdentity.New(); // this should never persist a change.
            toUpdate.Enqueued = toUpdate.Enqueued.AddMinutes(-1); // this should never change.
            toUpdate.Body = new("Changed Body"); // should never change.
            toUpdate.Headers = new("Changed Headers");
            toUpdate.NotBefore = toUpdate.NotBefore.AddMinutes(1);
            toUpdate.Completed = DateTime.UtcNow;
            toUpdate.Failed = DateTime.UtcNow;
            toUpdate.Retries = 10;

            await dataAccess.UpdateMessage(toUpdate, DefaultQueue);
            await Task.Delay(10); // wait for the rows to be ready

            // Check that it ended up in the error table.
            var pending = GetTableContent(QueuePendingTable).ToMessages();
            Assert.AreEqual(2, pending.Count);

            var expectUnchanged = toUpdate.Id == testMessage1.Id ? testMessage2 : testMessage1;
            var changedOriginal = toUpdate.Id != testMessage1.Id ? testMessage2 : testMessage1;

            var actualUnchanged = pending.Single(m => m.Id != toUpdate.Id);
            AssertQueueDataAreEqual(expectUnchanged, actualUnchanged);

            var actualChanged = pending.Single(m => m.Id == toUpdate.Id);
            // compare the unchangable fields.
            Assert.AreEqual(changedOriginal.Id, actualChanged.Id);
            Assert.AreEqual(changedOriginal.MessageId, actualChanged.MessageId);
            AssertSqlDbDateTime(changedOriginal.Enqueued, actualChanged.Enqueued);
            Assert.AreEqual(changedOriginal.Body, actualChanged.Body);
            // compare the changeable fields.
            Assert.AreEqual(toUpdate.Headers, actualChanged.Headers);
            AssertSqlDbDateTime(toUpdate.NotBefore, actualChanged.NotBefore);
            Assert.AreEqual(toUpdate.Retries, actualChanged.Retries);

            // completed and failed will be null for pending messages.            
            AssertSqlDbDateTime(null, actualChanged.Completed);
            AssertSqlDbDateTime(null, actualChanged.Failed);
        }
    }
}
