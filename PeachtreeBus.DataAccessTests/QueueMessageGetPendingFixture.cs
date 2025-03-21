using Microsoft.VisualStudio.TestTools.UnitTesting;
using PeachtreeBus.Abstractions.Tests;
using PeachtreeBus.Data;
using PeachtreeBus.Tests;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace PeachtreeBus.DataAccessTests
{
    /// <summary>
    /// Proves the behavior of DapperDataAccess.GetPendingQueued
    /// </summary>
    [TestClass]
    public class QueueMessageGetPendingFixture : DapperDataAccessFixtureBase
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
        /// proves that a message is returned.
        /// </summary>
        /// <returns></returns>
        [TestMethod]
        public async Task GetPendingQueued_GetsMessage()
        {
            // Add one message;
            var testMessage = TestData.CreateQueueMessage();
            testMessage.Id = await dataAccess.AddMessage(testMessage, DefaultQueue);

            await Task.Delay(10); // wait for the rows to be ready

            var actual = await dataAccess.GetPendingQueued(DefaultQueue);
            Assert.IsNotNull(actual);
            AssertQueueDataAreEqual(testMessage, actual);
        }

        /// <summary>
        /// Proves that the returned message is locked so that other
        /// connections to the DB cannot get the same message.
        /// </summary>
        /// <returns></returns>
        [TestMethod]
        public async Task GetPendingQueued_LocksTheMessage()
        {
            // Add two messages;
            var testMessage1 = TestData.CreateQueueMessage();
            testMessage1.Id = await dataAccess.AddMessage(testMessage1, DefaultQueue);
            var testMessage2 = TestData.CreateQueueMessage();
            testMessage2.Id = await dataAccess.AddMessage(testMessage2, DefaultQueue);

            await Task.Delay(10); // wait for the rows to be ready

            // get a message and leave the transaction open.
            dataAccess.BeginTransaction();
            try
            {
                var actual = await dataAccess.GetPendingQueued(DefaultQueue);
                Assert.IsNotNull(actual, "Did not read a message back.");

                BeginSecondaryTransaction();
                try
                {
                    var unlockedMessages = GetTableContentAndLock(QueuePendingTable).ToMessages();

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
        /// Proves that locked messages are not returned.
        /// </summary>
        /// <returns></returns>
        [TestMethod]
        public async Task GetPendingQueued_DoesNotReturnLocked()
        {
            // Add one message;
            var testMessage = TestData.CreateQueueMessage();
            testMessage.Id = await dataAccess.AddMessage(testMessage, DefaultQueue);
            await Task.Delay(10); // wait for the rows to be ready

            // lock the row
            BeginSecondaryTransaction();
            try
            {
                var pending = GetTableContentAndLock(QueuePendingTable);

                // check that the locked row can not be fetched.
                var actual = await dataAccess.GetPendingQueued(DefaultQueue);
                Assert.IsNull(actual);
            }
            finally
            {
                RollbackSecondaryTransaction();
            }
        }

        /// <summary>
        /// Proves that a message is not returned before its NotBefore time.
        /// </summary>
        /// <returns></returns>
        [TestMethod]
        public async Task GetPendingQueued_DoesNotReturnDelayedMessage()
        {
            // Add one message;
            var testMessage = TestData.CreateQueueMessage();
            testMessage.NotBefore = testMessage.NotBefore.AddHours(1);
            testMessage.Id = await dataAccess.AddMessage(testMessage, DefaultQueue);
            await Task.Delay(10); // wait for the rows to be ready
            var actual = await dataAccess.GetPendingQueued(DefaultQueue);
            Assert.IsNull(actual);
        }

        /// <summary>
        /// Proves that a message can be returned after its NotBefore time.
        /// </summary>
        /// <returns></returns>
        [TestMethod]
        public async Task GetPendingQueued_DoesReturnDelayedAfterWait()
        {
            // Add one message;
            var testMessage = TestData.CreateQueueMessage();
            testMessage.NotBefore = testMessage.NotBefore.AddMilliseconds(200);
            testMessage.Id = await dataAccess.AddMessage(testMessage, DefaultQueue);
            await Task.Delay(10); // wait for the rows to be ready
            var actual = await dataAccess.GetPendingQueued(DefaultQueue);
            Assert.IsNull(actual);
            await Task.Delay(400);
            actual = await dataAccess.GetPendingQueued(DefaultQueue);
            Assert.IsNotNull(actual);
            AssertQueueDataAreEqual(testMessage, actual);
        }

        [TestMethod]
        public async Task GetPendingQueued_ReturnsHigherPriorityMessage()
        {
            var lowMessage = TestData.CreateQueueMessage();
            lowMessage.Priority = 1;
            lowMessage.NotBefore = DateTime.UtcNow.AddMinutes(-2);
            lowMessage.Id = await dataAccess.AddMessage(lowMessage, DefaultQueue);

            var highMessage = TestData.CreateQueueMessage();
            highMessage.Priority = 2;
            highMessage.NotBefore = DateTime.UtcNow.AddMinutes(-1);
            highMessage.Id = await dataAccess.AddMessage(highMessage, DefaultQueue);

            await Task.Delay(10); // wait for the rows to be ready

            var actual = await dataAccess.GetPendingQueued(DefaultQueue);
            Assert.IsNotNull(actual);
            AssertQueueDataAreEqual(highMessage, actual);
        }
    }
}
