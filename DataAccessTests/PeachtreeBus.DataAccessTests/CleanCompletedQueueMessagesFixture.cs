using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Threading.Tasks;
using PeachtreeBus.Data;

namespace PeachtreeBus.DataAccessTests
{
    public abstract class CleanCompletedQueueMessagesFixture : BusDataAccessFixtureBase
    {
        private long _lastId = 1000;

        [TestInitialize]
        public override void Initialize() => base.Initialize();

        [TestCleanup]
        public override void Cleanup() => base.Cleanup();

        private void Given_CompletedMessage(DateTime completed)
        {
            TestDataAccess.InsertQueueCompleted(new()
            {
                Id = new(_lastId++),
                MessageId = UniqueIdentity.New(),
                Priority = 0,
                NotBefore = DateTime.UtcNow.AddDays(-1),
                Enqueued = DateTime.UtcNow.AddDays(-1),
                Completed = completed,
                Failed = null,
                Retries = 0,
                Headers = new(),
                Body = new("{}"),
            });
        }

        private void Given_CountCompletedMessage(int count, DateTime completed)
        {
            for (var i = 0; i < count; i++)
            {
                Given_CompletedMessage(completed);
            }
        }
        
        [TestMethod]
        [DataRow(10, 10, 0, DisplayName = "Cleans All")]
        [DataRow(10, 5, 5, DisplayName = "Cleans Top N")]
        public async Task Given_MessagesCompletedYesterday_When_CleanCompletedQueueMessages_Then_TableHasCount(
            int givenCount, int cleanupCount, int remainingCount)
        {
            var yesterday = DateTime.UtcNow.AddDays(-1);
            Given_CountCompletedMessage(givenCount, yesterday);

            var olderThan = DateTime.UtcNow;
            var deletedCount = await BusDataAccess.CleanQueueCompleted(TestConfig.DefaultQueue, olderThan, cleanupCount);
            Assert.AreEqual(cleanupCount, deletedCount);

            TestDataAccess.Then_TableHasCount(TestConfig.QueueCompleted, remainingCount);
        }

        [TestMethod]
        public async Task Given_MessagesCompletedToday_When_CleanCompletedQueueMessages_Then_MessagesNotDeleted()
        {
            Given_CountCompletedMessage(10, DateTime.UtcNow);

            var olderThan = DateTime.UtcNow.AddMinutes(-5);
            var deletedCount = await BusDataAccess.CleanQueueCompleted(TestConfig.DefaultQueue, olderThan, 10);

            Assert.AreEqual(0, deletedCount);
            TestDataAccess.Then_TableHasCount(TestConfig.QueueCompleted, 10);
        }

        [TestMethod]
        public async Task Given_MixOfCompleted_When_CleanCompletedQueueMessages_Then_OldMessagesAreDeleted()
        {
            Given_CountCompletedMessage(3,  DateTime.UtcNow.AddDays(-1));
            Given_CountCompletedMessage(7, DateTime.UtcNow);
            
            var olderThan = DateTime.UtcNow.AddMinutes(-5);
            var deletedCount = await BusDataAccess.CleanQueueCompleted(TestConfig.DefaultQueue, olderThan, 10);
            Assert.AreEqual(3, deletedCount);

            TestDataAccess.Then_TableHasCount(TestConfig.QueueCompleted, 7);
        }
    }
}
