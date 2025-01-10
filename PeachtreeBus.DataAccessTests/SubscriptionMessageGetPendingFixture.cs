using Microsoft.VisualStudio.TestTools.UnitTesting;
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
            var testMessage = CreateSubscribed();
            testMessage.SubscriberId = Guid.NewGuid();
            testMessage.NotBefore = testMessage.NotBefore.AddHours(1);
            testMessage.Id = await dataAccess.AddMessage(testMessage);
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
            var testMessage = CreateSubscribed();
            testMessage.SubscriberId = Guid.NewGuid();
            testMessage.Id = await dataAccess.AddMessage(testMessage);
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
            var testMessage = CreateSubscribed();
            testMessage.SubscriberId = Guid.NewGuid();
            testMessage.NotBefore = testMessage.NotBefore.AddMilliseconds(200);
            testMessage.Id = await dataAccess.AddMessage(testMessage);
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
            var testMessage = CreateSubscribed();
            testMessage.SubscriberId = Guid.NewGuid();
            testMessage.Id = await dataAccess.AddMessage(testMessage);

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
            var testMessage1 = CreateSubscribed();
            testMessage1.SubscriberId = Guid.NewGuid();
            testMessage1.Id = await dataAccess.AddMessage(testMessage1);
            var testMessage2 = CreateSubscribed();
            testMessage2.SubscriberId = Guid.NewGuid();
            testMessage2.Id = await dataAccess.AddMessage(testMessage2);

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

        /// <summary>
        /// Proves that the subscribed id cannot be empty.
        /// </summary>
        /// <returns></returns>
        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public async Task GetPendingSubscriptionMessage_ThrowsIfSubscriberIsGuidEmpty()
        {
            await dataAccess.GetPendingSubscribed(Guid.Empty);
        }

        /// <summary>
        /// Proves that unsafe schema is not allowed.
        /// </summary>
        [TestMethod]
        public async Task GetPendingSubscriptionMessage_ThrowsIfSchemaContainsUnsafe()
        {
            var action = new Func<Task>(() => dataAccess.GetPendingSubscribed(Guid.NewGuid()));
            await ActionThrowsIfSchemaContainsPoisonChars(action);
        }

        [TestMethod]
        public async Task GetPendingSubscriptionMessage_ReturnsHigherPriorityMessage()
        {
            var subscriber = Guid.NewGuid();

            var lowMessage = CreateSubscribed();
            lowMessage.Priority = 1;
            lowMessage.NotBefore = DateTime.UtcNow.AddMinutes(-2);
            lowMessage.SubscriberId = subscriber;
            lowMessage.Id = await dataAccess.AddMessage(lowMessage);

            var highMessage = CreateSubscribed();
            highMessage.Priority = 2;
            highMessage.NotBefore = DateTime.UtcNow.AddMinutes(-1);
            highMessage.SubscriberId = subscriber;
            highMessage.Id = await dataAccess.AddMessage(highMessage);

            await Task.Delay(10); // wait for the rows to be ready

            var actual = await dataAccess.GetPendingSubscribed(subscriber);
            Assert.IsNotNull(actual);
            AssertSubscribedEquals(highMessage, actual);
        }
    }
}
