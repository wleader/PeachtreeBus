using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace PeachtreeBus.DataAccessTests
{
    [TestClass]
    public class ManagementGetPendingSubscribedMessagesFixture : ManagementDataAccessFixtureBase
    {
        [TestMethod]
        public async Task GetsTheExpectedData()
        {
            var s1 = await CreatePendingSubscribed();
            var s2 = await CreatePendingSubscribed();
            var s3 = await CreatePendingSubscribed();
            var s4 = await CreatePendingSubscribed();

            var actual = await dataAccess.GetPendingSubscribedMessages(1, 2);

            Assert.AreEqual(2, actual.Count);
            Assert.IsFalse(actual.Any(s => s.Id == s1.Id), "Oldest should not be taken");
            Assert.IsFalse(actual.Any(s => s.Id == s4.Id), "Newest should be skipped");
            Assert.AreEqual(actual[0].Id, s3.Id, "Newer Expected is not correct.");
            Assert.AreEqual(actual[1].Id, s2.Id, "Older Expected is not correct.");
        }

        [TestMethod]
        public async Task ThrowsIfSchemaNameContainsUnsafe()
        {
            var action = new Func<Task>(async () => await dataAccess.GetPendingSubscribedMessages(1, 2));
            await ActionThrowsIfSchemaContainsPoisonChars(action);
        }

    }
}
