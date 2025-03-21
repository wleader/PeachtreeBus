using Microsoft.VisualStudio.TestTools.UnitTesting;
using PeachtreeBus.Abstractions.Tests;
using PeachtreeBus.Subscriptions;
using PeachtreeBus.Tests;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace PeachtreeBus.DataAccessTests
{
    /// <summary>
    /// Proves the behavior of DapperDataAccess.GetPendingSubscribed
    /// </summary>
    [TestClass]
    public class SubscriptionMessageGetPendingFixture : DapperDataAccessFixtureBase
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
        /// Proves that a message is not returned before its NotBefore value.
        /// </summary>
        /// <returns></returns>
        [TestMethod]
        public async Task GetPendingSubscriptionMessage_DoesNotReturnDelayedMessage()
        {
            // Add one message;
            var testMessage = TestData.CreateSubscribedData(
                notBefore: DateTime.UtcNow.AddHours(1));
            await InsertSubscribedMessage(testMessage);
            await Task.Delay(10); // wait for the rows to be ready
            var actual = await dataAccess.GetPendingSubscribed(testMessage.SubscriberId);
            Assert.IsNull(actual);
        }

        /// <summary>
        /// Provess that a row locked by another connection cannot be returned.
        /// </summary>
        /// <returns></returns>
        [TestMethod]
        public async Task GetPendingSubscriptionMessage_DoesNotReturnLocked()
        {
            // Add one message;
            var testMessage = TestData.CreateSubscribedData();
            await InsertSubscribedMessage(testMessage);
            await Task.Delay(10); // wait for the rows to be ready

            // lock the row
            BeginSecondaryTransaction();
            try
            {
                var pending = GetTableContentAndLock(SubscribedPendingTable);

                // check that the locked row can not be fetched.
                var actual = await dataAccess.GetPendingSubscribed(testMessage.SubscriberId);
                Assert.IsNull(actual);
            }
            finally
            {
                RollbackSecondaryTransaction();
            }
        }

        /// <summary>
        /// Proves that a row can be returned after its NotBefore value.
        /// </summary>
        /// <returns></returns>
        [TestMethod]
        public async Task GetPendingSubscriptionMessage_DoesReturnDelayedAfterWait()
        {
            // Add one message;
            var testMessage = TestData.CreateSubscribedData(
                notBefore: DateTime.UtcNow.AddMilliseconds(200));
            await InsertSubscribedMessage(testMessage);
            await Task.Delay(10); // wait for the rows to be ready
            var actual = await dataAccess.GetPendingSubscribed(testMessage.SubscriberId);
            Assert.IsNull(actual);
            await Task.Delay(400);
            actual = await dataAccess.GetPendingSubscribed(testMessage.SubscriberId);
            Assert.IsNotNull(actual);
            AssertSubscribedEquals(testMessage, actual);
        }

        /// <summary>
        /// Proves that a pending row is returned.
        /// </summary>
        /// <returns></returns>
        [TestMethod]
        public async Task GetPendingSubscriptionMessage_GetsMessage()
        {
            // Add one message;
            var testMessage = TestData.CreateSubscribedData();
            await InsertSubscribedMessage(testMessage);

            await Task.Delay(10); // wait for the rows to be ready

            var actual = await dataAccess.GetPendingSubscribed(testMessage.SubscriberId);
            Assert.IsNotNull(actual);
            AssertSubscribedEquals(testMessage, actual);
        }

        /// <summary>
        /// Proves that the returned row is locked.
        /// </summary>
        /// <returns></returns>
        [TestMethod]
        public async Task GetPendingSubscriptionMessage_LocksTheMessage()
        {
            // Add two messages;
            var testMessage1 = TestData.CreateSubscribedData();
            await InsertSubscribedMessage(testMessage1);
            var testMessage2 = TestData.CreateSubscribedData();
            await InsertSubscribedMessage(testMessage2);

            await Task.Delay(10); // wait for the rows to be ready

            // get a message and leave the transaction open.
            dataAccess.BeginTransaction();
            try
            {
                var actual = await dataAccess.GetPendingSubscribed(testMessage1.SubscriberId);
                Assert.IsNotNull(actual, "Did not read a message back.");

                BeginSecondaryTransaction();
                try
                {
                    var unlockedMessages = GetTableContentAndLock(SubscribedPendingTable).ToSubscribed();

                    Assert.AreEqual(1, unlockedMessages.Count, "Wrong number of unlocked messages.");
                    Assert.AreNotEqual(testMessage1.Id, testMessage2.Id, "Test Messages have the same ID.");
                    Assert.IsFalse(unlockedMessages.Any(m => m.Id == actual.Id), $"Locked message {actual.Id} found in unlocked messages {unlockedMessages[0].Id}");
                }
                finally
                {
                    RollbackSecondaryTransaction();
                }
            }
            finally
            {
                dataAccess.RollbackTransaction();
            }
        }



        [TestMethod]
        public async Task GetPendingSubscriptionMessage_ReturnsHigherPriorityMessage()
        {
            var subscriber = SubscriberId.New();

            var lowMessage = TestData.CreateSubscribedData(
                priority: 1,
                notBefore: DateTime.UtcNow.AddMinutes(-2),
                subscriberId: subscriber);
            await InsertSubscribedMessage(lowMessage);

            var highMessage = TestData.CreateSubscribedData(
                priority: 2,
                notBefore: DateTime.UtcNow.AddMinutes(-1),
                subscriberId: subscriber);
            await InsertSubscribedMessage(highMessage);

            await Task.Delay(10); // wait for the rows to be ready

            var actual = await dataAccess.GetPendingSubscribed(subscriber);
            Assert.IsNotNull(actual);
            AssertSubscribedEquals(highMessage, actual);
        }

        [TestMethod]
        public async Task GetPendingSubscriptionMessage_ThrowsIfSubscriberIsGuidEmpty()
        {
            await Assert.ThrowsExceptionAsync<SubscriberIdException>(() =>
                dataAccess.GetPendingSubscribed(TestData.UnintializedSubscriberId));
        }
    }
}
