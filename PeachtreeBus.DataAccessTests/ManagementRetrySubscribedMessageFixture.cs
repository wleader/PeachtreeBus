using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Threading.Tasks;

namespace PeachtreeBus.DataAccessTests
{
    [TestClass]
    public class ManagementRetrySubscribedMessageFixture : ManagementDataAccessFixtureBase
    {
        [TestMethod]
        public async Task InsertsIntoTargetTable()
        {
            var s1 = await CreateFailedSubscribed();
            await CreateFailedSubscribed();

            await dataAccess.RetryFailedSubscribedMessage(s1.Id);

            var pending = await dataAccess.GetPendingSubscribedMessages(0, 1);
            Assert.AreEqual(1, pending.Count);
            Assert.AreEqual(s1.MessageId, pending[0].MessageId);
            Assert.AreEqual(s1.Headers, pending[0].Headers);
            AssertSqlDbDateTime(s1.ValidUntil, pending[0].ValidUntil);
            Assert.AreEqual(s1.Body, pending[0].Body);
            AssertSqlDbDateTime(s1.Enqueued, pending[0].Enqueued);
            Assert.AreEqual(0, pending[0].Retries);
            Assert.AreEqual(null, pending[0].Completed);
            Assert.AreEqual(null, pending[0].Failed);
            Assert.AreEqual(s1.SubscriberId, pending[0].SubscriberId);
            AssertSqlDbDateTime(s1.NotBefore, pending[0].NotBefore);
        }

        [TestMethod]
        public async Task DeletesFromSourceTable()
        {
            var s1 = await CreateFailedSubscribed();
            var s2 = await CreateFailedSubscribed();

            await dataAccess.RetryFailedSubscribedMessage(s1.Id);

            var failed = await dataAccess.GetFailedSubscribedMessages(0, int.MaxValue);
            Assert.AreEqual(1, failed.Count);
            Assert.AreEqual(s2.Id, failed[0].Id);
        }
    }
}
