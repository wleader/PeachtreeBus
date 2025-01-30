using Microsoft.VisualStudio.TestTools.UnitTesting;
using PeachtreeBus.Subscriptions;
using PeachtreeBus.Tests;
using System.Threading.Tasks;

namespace PeachtreeBus.DataAccessTests
{
    /// <summary>
    /// Proves the behavior of DapperDataAccess.AddMessage (subscribed)
    /// </summary>
    [TestClass]
    public class SubscriptionAddMessageFixture : DapperDataAccessFixtureBase
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
        /// proves the row is inserted.
        /// </summary>
        /// <returns></returns>
        [TestMethod]
        public async Task AddMessage_StoresTheMessage()
        {
            Assert.AreEqual(0, CountRowsInTable(SubscribedPendingTable));

            var newMessage = TestData.CreateSubscribedMessage();
            newMessage.SubscriberId = SubscriberId.New();

            newMessage.Id = await dataAccess.AddMessage(newMessage);

            Assert.IsTrue(newMessage.Id.Value > 0);

            var data = GetTableContent(SubscribedPendingTable);
            Assert.IsNotNull(data);

            var messages = data.ToSubscribed();
            Assert.AreEqual(1, messages.Count);

            AssertSubscribedEquals(newMessage, messages[0]);
        }

        [TestMethod]
        public async Task Given_UninitializedSubscriberId_When_AddMessage_Then_Throws()
        {
            var newMessage = TestData.CreateSubscribedMessage(
                subscriberId: TestData.UnintializedSubscriberId);
            await Assert.ThrowsExceptionAsync<SubscriberIdException>(() =>
                dataAccess.AddMessage(newMessage));
        }
    }
}
