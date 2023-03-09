using Dapper;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Threading.Tasks;

namespace PeachtreeBus.DataAccessTests
{
    /// <summary>
    /// Proves the behavior of DapperDataAcess.CleanQueueCompleted
    /// </summary>
    [TestClass]
    public class CleanCompletedQueueMessagesFixture : DapperDataAccessFixtureBase
    {
        private long lastId = 1000;

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
        /// Inserts a row to setup the test.
        /// </summary>
        /// <param name="completed">The time that the message was completed.</param>
        /// <returns></returns>
        private async Task CreateTestRow(DateTime completed)
        {
            // puts a row in the completed table.
            var statement =
            "INSERT INTO [{0}].[{1}_Completed] " +
            "([Id],[MessageId],[NotBefore],[Enqueued],[Completed],[Failed],[Retries],[Headers],[Body]) " +
            "VALUES " +
            "(@Id, @MessageId, @NotBefore, @Enqueued, @Completed, @Failed, @Retries, @Headers, @Body)";

            statement = string.Format(statement, DefaultSchema, DefaultQueue);

            var p = new DynamicParameters();
            p.Add("@Id", lastId++);
            p.Add("@MessageId", Guid.NewGuid());
            p.Add("@NotBefore", DateTime.UtcNow.AddDays(-1));
            p.Add("@Enqueued", DateTime.UtcNow.AddDays(-1));
            p.Add("@Completed", completed);
            p.Add("@Failed", null);
            p.Add("@Retries", 0);
            p.Add("@Headers", "");
            p.Add("@Body", "");

            await SecondaryConnection.ExecuteAsync(statement, p);
        }

        /// <summary>
        /// Proves that rows get deleted.
        /// </summary>
        /// <returns></returns>
        [TestMethod]
        public async Task CleanCompletedQueueMessages_Cleans()
        {
            var completed = DateTime.UtcNow.AddDays(-1);
            for (var i = 0; i < 10; i++)
            {
                await CreateTestRow(completed);
            }

            Assert.AreEqual(10, CountRowsInTable(QueueCompletedTable));
            var olderthan = DateTime.UtcNow;

            var count = await dataAccess.CleanQueueCompleted(DefaultQueue, olderthan, 10);
            Assert.AreEqual(10, count);

            Assert.AreEqual(0, CountRowsInTable(QueueCompletedTable));
        }

        /// <summary>
        /// Proves that the cleanup is limited to the supplied max count.
        /// </summary>
        /// <returns></returns>
        [TestMethod]
        public async Task CleanCompletedQueueMessages_CleansTopN()
        {
            var completed = DateTime.UtcNow.AddDays(-1);
            for (var i = 0; i < 10; i++)
            {
                await CreateTestRow(completed);
            }

            Assert.AreEqual(10, CountRowsInTable(QueueCompletedTable));
            var olderthan = DateTime.UtcNow;

            var count = await dataAccess.CleanQueueCompleted(DefaultQueue, olderthan, 5);
            Assert.AreEqual(5, count);

            Assert.AreEqual(5, CountRowsInTable(QueueCompletedTable));
        }

        /// <summary>
        /// Proves that messages that completed after the olderthan time are not cleaned.
        /// </summary>
        /// <returns></returns>
        [TestMethod]
        public async Task CleanCompletedQueueMessages_RespectsOlderThan()
        {
            var completed = DateTime.UtcNow;
            for (var i = 0; i < 10; i++)
            {
                await CreateTestRow(completed);
            }

            Assert.AreEqual(10, CountRowsInTable(QueueCompletedTable));
            var olderthan = DateTime.UtcNow.AddMinutes(-5);

            var count = await dataAccess.CleanQueueCompleted(DefaultQueue, olderthan, 10);
            Assert.AreEqual(0, count);

            Assert.AreEqual(10, CountRowsInTable(QueueCompletedTable));
        }

        /// <summary>
        /// Proves that older messages are cleaned and younger messages are not.
        /// </summary>
        /// <returns></returns>
        [TestMethod]
        public async Task CleanCompletedQueueMessages_HandlesMix()
        {
            var completed = DateTime.UtcNow.AddDays(-1);
            for (var i = 0; i < 3; i++)
            {
                await CreateTestRow(completed);
            }

            completed = DateTime.UtcNow;
            for (var i = 0; i < 7; i++)
            {
                await CreateTestRow(completed);
            }

            Assert.AreEqual(10, CountRowsInTable(QueueCompletedTable));
            var olderthan = DateTime.UtcNow.AddMinutes(-5);

            var count = await dataAccess.CleanQueueCompleted(DefaultQueue, olderthan, 10);
            Assert.AreEqual(3, count);

            Assert.AreEqual(7, CountRowsInTable(QueueCompletedTable));
        }

        /// <summary>
        /// Proves that statements will not execute if the schema name contains
        /// characters that risk SQL injection.
        /// </summary>
        [TestMethod]
        public async Task CleanCompletedQueueMessages_ThrowsIfSchemaUnsafe()
        {
            var action = new Func<Task>(async () => await dataAccess.CleanQueueCompleted(DefaultQueue, DateTime.MinValue, 1));
            await ActionThrowsIfSchemaContainsPoisonChars(action);
        }

        /// <summary>
        /// proves that statements will not execute if the queeu name contains
        /// characters that risk SQL injection.
        /// </summary>
        [TestMethod]
        public async Task CleanCompletedQueueMessages_ThrowsIfQueueNameUnsafe()
        {
            var action = new Func<string, Task>(async (s) => await dataAccess.CleanQueueCompleted(s, DateTime.MinValue, 1));
            await ActionThrowsIfParameterContainsPoisonChars(action);
        }
    }
}
