using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Threading.Tasks;

namespace PeachtreeBus.DataAccessTests
{
    [TestClass]
    public class ManagementRetryQueueMessageFixture : ManagementDataAccessFixtureBase
    {
        [TestMethod]
        public async Task ThrowsIfQueueNameContainsUnsafe()
        {
            var action = new Func<string, Task>(async (s) => await dataAccess.RetryFailedQueueMessage(s, 1));
            await ActionThrowsIfParameterContainsPoisonChars(action);
        }

        [TestMethod]
        public async Task ThrowsIfSchemaNameContainsUnsafe()
        {
            var action = new Func<Task>(async () => await dataAccess.RetryFailedQueueMessage(DefaultQueue, 1));
            await ActionThrowsIfSchemaContainsPoisonChars(action);
        }

        [TestMethod]
        public async Task InsertsIntoTargetTable()
        {
            var s1 = await CreateFailedQueued();
            await CreateFailedQueued();

            await dataAccess.RetryFailedQueueMessage(DefaultQueue, s1.Id);

            var pending = await dataAccess.GetPendingQueueMessages(DefaultQueue, 0, 1);
            Assert.AreEqual(1, pending.Count);
            Assert.AreEqual(s1.MessageId, pending[0].MessageId);
            Assert.AreEqual(s1.Headers, pending[0].Headers);
            Assert.AreEqual(s1.Body, pending[0].Body);
            AssertSqlDbDateTime(s1.Enqueued, pending[0].Enqueued);
            Assert.AreEqual(0, pending[0].Retries);
            Assert.AreEqual(null, pending[0].Completed);
            Assert.AreEqual(null, pending[0].Failed);
            AssertSqlDbDateTime(s1.NotBefore, pending[0].NotBefore);
        }

        [TestMethod]
        public async Task DeletesFromSourceTable()
        {
            var s1 = await CreateFailedQueued();
            var s2 = await CreateFailedQueued();

            await dataAccess.RetryFailedQueueMessage(DefaultQueue, s1.Id);

            var failed = await dataAccess.GetFailedQueueMessages(DefaultQueue, 0, int.MaxValue);
            Assert.AreEqual(1, failed.Count);
            Assert.AreEqual(s2.Id, failed[0].Id);
        }
    }
}
