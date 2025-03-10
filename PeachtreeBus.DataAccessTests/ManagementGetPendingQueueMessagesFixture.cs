﻿using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Linq;
using System.Threading.Tasks;

namespace PeachtreeBus.DataAccessTests
{
    [TestClass]
    public class ManagementGetPendingQueueMessagesFixture : ManagementDataAccessFixtureBase
    {
        [TestMethod]
        public async Task GetsTheExpectedData()
        {
            var s1 = await CreatePendingQueued();
            var s2 = await CreatePendingQueued();
            var s3 = await CreatePendingQueued();
            var s4 = await CreatePendingQueued();

            var actual = await dataAccess.GetPendingQueueMessages(DefaultQueue, 1, 2);

            Assert.AreEqual(2, actual.Count);
            Assert.IsFalse(actual.Any(s => s.Id == s1.Id), "Oldest should not be taken");
            Assert.IsFalse(actual.Any(s => s.Id == s4.Id), "Newest should be skipped");
            Assert.AreEqual(actual[0].Id, s3.Id, "Newer Expected is not correct.");
            Assert.AreEqual(actual[1].Id, s2.Id, "Older Expected is not correct.");
        }
    }
}
