using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Threading.Tasks;

namespace PeachtreeBus.DataAccessTests
{
    [TestClass]
    public class ManagementCancelQueueMessageFixture : ManagementDataAccessFixtureBase
    {
        [TestMethod]
        public async Task InsertsIntoTargetTable()
        {
            var s1 = await CreatePendingQueued();
            await CreatePendingQueued();

            await dataAccess.CancelPendingQueueMessage(DefaultQueue, s1.Id);

            var failed = await dataAccess.GetFailedQueueMessages(DefaultQueue, 0, 1);
            Assert.AreEqual(1, failed.Count);
            Assert.AreEqual(s1.MessageId, failed[0].MessageId);
            Assert.AreEqual(s1.Headers, failed[0].Headers);
            Assert.AreEqual(s1.Body, failed[0].Body);
            AssertSqlDbDateTime(s1.Enqueued, failed[0].Enqueued);
            Assert.AreEqual(0, failed[0].Retries);
            Assert.AreEqual(null, failed[0].Completed);
            AssertSqlDbDateTime(DateTime.UtcNow, failed[0].Failed, 5000);
            AssertSqlDbDateTime(s1.NotBefore, failed[0].NotBefore);
        }

        [TestMethod]
        public async Task DeletesFromSourceTable()
        {
            var s1 = await CreatePendingQueued();
            var s2 = await CreatePendingQueued();

            await dataAccess.CancelPendingQueueMessage(DefaultQueue, s1.Id);

            var pending = await dataAccess.GetPendingQueueMessages(DefaultQueue, 0, int.MaxValue);
            Assert.AreEqual(1, pending.Count);
            Assert.AreEqual(s2.Id, pending[0].Id);
        }
    }
}
