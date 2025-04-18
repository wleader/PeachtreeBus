﻿using Microsoft.VisualStudio.TestTools.UnitTesting;
using PeachtreeBus.Abstractions.Tests;
using PeachtreeBus.Core.Tests;
using PeachtreeBus.Data;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace PeachtreeBus.DataAccessTests
{
    /// <summary>
    /// Proves the behavior of DapperDataAccess.CompleteMessage
    /// </summary>
    [TestClass]
    public class QueueMessageCompleteFixture : DapperDataAccessFixtureBase
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
        /// Proves the message is copied from Pending table to Complete Table.
        /// </summary>
        /// <returns></returns>
        [TestMethod]
        public async Task CompleteMessage_InsertsIntoCompleteTable()
        {
            // Add two messages;
            var testMessage1 = TestData.CreateQueueData();
            testMessage1.Id = await dataAccess.AddMessage(testMessage1, DefaultQueue);
            var testMessage2 = TestData.CreateQueueData();
            testMessage2.Id = await dataAccess.AddMessage(testMessage2, DefaultQueue);
            await Task.Delay(10); // wait for the rows to be ready

            // get and complete a message.
            var messageToComplete = await dataAccess.GetPendingQueued(DefaultQueue);
            Assert.IsNotNull(messageToComplete);
            messageToComplete.Completed = DateTime.UtcNow;
            await dataAccess.CompleteMessage(messageToComplete, DefaultQueue);
            await Task.Delay(10); // wait for the rows to be ready

            // Check that it ended up in the completed table.
            var completed = GetTableContent(QueueCompleted).ToMessages();
            Assert.AreEqual(1, completed.Count);
            AssertQueueDataAreEqual(messageToComplete, completed[0]);
        }

        /// <summary>
        /// Proves the message is deleted from the pending table.
        /// </summary>
        /// <returns></returns>
        [TestMethod]
        public async Task CompleteMessage_DeletesFromPendingTable()
        {
            // Add two messages;
            var testMessage1 = TestData.CreateQueueData();
            testMessage1.Id = await dataAccess.AddMessage(testMessage1, DefaultQueue);
            var testMessage2 = TestData.CreateQueueData();
            testMessage2.Id = await dataAccess.AddMessage(testMessage2, DefaultQueue);
            await Task.Delay(10); // wait for the rows to be ready

            var messageToComplete = await dataAccess.GetPendingQueued(DefaultQueue);
            Assert.IsNotNull(messageToComplete);
            messageToComplete.Completed = DateTime.UtcNow;
            await dataAccess.CompleteMessage(messageToComplete, DefaultQueue);
            await Task.Delay(10); // wait for the rows to be ready

            var pending = GetTableContent(QueuePending).ToMessages();
            Assert.AreEqual(1, pending.Count);
            Assert.IsFalse(pending.Any(m => m.Id == messageToComplete.Id), "Completed message is still in the pending table.");
        }

        /// <summary>
        /// Proves that fields that should not change do not change.
        /// </summary>
        /// <returns></returns>
        [TestMethod]
        public async Task CompleteMessage_CantMutateFields()
        {
            // Add two messages;
            var testMessage1 = TestData.CreateQueueData();
            testMessage1.Id = await dataAccess.AddMessage(testMessage1, DefaultQueue);
            await Task.Delay(10); // wait for the rows to be ready

            // get and complete a message.
            var messageToComplete = await dataAccess.GetPendingQueued(DefaultQueue);
            Assert.IsNotNull(messageToComplete);
            messageToComplete.Completed = DateTime.UtcNow;
            // screw with the fields that shouldn't change.
            messageToComplete.Body = new("NewBody");
            messageToComplete.Enqueued = messageToComplete.Enqueued.AddMinutes(1);
            messageToComplete.MessageId = UniqueIdentity.New();

            await dataAccess.CompleteMessage(messageToComplete, DefaultQueue);
            await Task.Delay(10); // wait for the rows to be ready

            // Check that it ended up in the completed table.
            var completed = GetTableContent(QueueCompleted).ToMessages();
            Assert.AreEqual(1, completed.Count);
            var actual = completed.Single(m => m.Id == testMessage1.Id);

            // check the immutable fields are the oringal valules.
            Assert.AreEqual(testMessage1.MessageId, actual.MessageId, "MessageId should not change.");
            AssertSqlDbDateTime(testMessage1.Enqueued, actual.Enqueued);
            Assert.AreEqual(testMessage1.Body, actual.Body, "Body should not change.");
        }
    }
}
