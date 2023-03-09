using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace PeachtreeBus.DataAccessTests
{
    [TestClass]
    public class ManagementGetCompletedQueueMessagesFixture : ManagementDataAccessFixtureBase
    {
        [TestMethod]
        public async Task GetsTheExpectedData()
        {
            var s1 = await CreateCompletedQueued();
            var s2 = await CreateCompletedQueued();
            var s3 = await CreateCompletedQueued();
            var s4 = await CreateCompletedQueued();

            var actual = await dataAccess.GetCompletedQueueMessages(DefaultQueue, 1, 2);

            Assert.AreEqual(2, actual.Count);
            Assert.IsFalse(actual.Any(s => s.Id == s1.Id), "Oldest should not be taken");
            Assert.IsFalse(actual.Any(s => s.Id == s4.Id), "Newest should be skipped");
            Assert.AreEqual(actual[0].Id, s3.Id, "Newer Expected is not correct.");
            Assert.AreEqual(actual[1].Id, s2.Id, "Older Expected is not correct.");
        }

        [TestMethod]
        public async Task ThrowsIfQueueNameContainsUnsafe()
        {
            var action = new Func<string, Task>(async (s) => await dataAccess.GetCompletedQueueMessages(s, 1, 2));
            await ActionThrowsIfParameterContainsPoisonChars(action);
        }

        [TestMethod]
        public async Task ThrowsIfSchemaNameContainsUnsafe()
        {
            var action = new Func<Task>(async () => await dataAccess.GetCompletedQueueMessages(DefaultQueue, 1, 2));
            await ActionThrowsIfSchemaContainsPoisonChars(action);
        }

    }
}
