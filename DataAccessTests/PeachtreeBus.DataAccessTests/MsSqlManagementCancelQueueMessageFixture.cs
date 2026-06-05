using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Threading.Tasks;

namespace PeachtreeBus.DataAccessTests
{
    [TestClass]
    public class MsSqlManagementCancelQueueMessageFixture : MsSqlManagementDataAccessFixtureBase
    {
        [TestMethod]
        public async Task InsertsIntoTargetTable()
        {
            var s1 = await CreatePendingQueued();
            await CreatePendingQueued();

            await dataAccess.CancelPendingQueueMessage(TestConfig.DefaultQueue, s1.Id);

            var failed = await dataAccess.GetFailedQueueMessages(TestConfig.DefaultQueue, 0, 1);
            Assert.AreEqual(1, failed.Count);
            Assert.AreEqual(s1.MessageId, failed[0].MessageId);
            DataAssert.AreEqual(s1.Headers, failed[0].Headers);
            Assert.AreEqual(s1.Body, failed[0].Body);
            DataAssert.AreEqual(s1.Enqueued, failed[0].Enqueued);
            Assert.AreEqual(0, failed[0].Retries);
            Assert.AreEqual(null, failed[0].Completed);
            DataAssert.AreEqual(DateTime.UtcNow, failed[0].Failed, 5000);
            DataAssert.AreEqual(s1.NotBefore, failed[0].NotBefore);
        }

        [TestMethod]
        public async Task DeletesFromSourceTable()
        {
            var s1 = await CreatePendingQueued();
            var s2 = await CreatePendingQueued();

            await dataAccess.CancelPendingQueueMessage(TestConfig.DefaultQueue, s1.Id);

            var pending = await dataAccess.GetPendingQueueMessages(TestConfig.DefaultQueue, 0, int.MaxValue);
            Assert.AreEqual(1, pending.Count);
            Assert.AreEqual(s2.Id, pending[0].Id);
        }
    }
}
