using Dapper;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Threading.Tasks;

namespace PeachtreeBus.DataAccessTests
{
    /// <summary>
    /// Proves the behavior of DapperDataAcess.CleanQueueFailed
    /// </summary>
    [TestClass]
    public class CleanQueueFailedFixture : FixtureBase
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
        /// Inserrts a failed queue message for the test.
        /// </summary>
        /// <param name="failed"></param>
        /// <returns></returns>
        private async Task CreateTestRow(DateTime failed)
        {
            // puts a row in the completed table.
            var statement =
            "INSERT INTO [{0}].[{1}_Failed] " +
            "([Id],[MessageId],[NotBefore],[Enqueued],[Completed],[Failed],[Retries],[Headers],[Body]) " +
            "VALUES " +
            "(@Id, @MessageId, @NotBefore, @Enqueued, @Completed, @Failed, @Retries, @Headers, @Body)";

            statement = string.Format(statement, DefaultSchema, DefaultQueue);

            var p = new DynamicParameters();
            p.Add("@Id", lastId++);
            p.Add("@MessageId", Guid.NewGuid());
            p.Add("@NotBefore", DateTime.UtcNow.AddDays(-1));
            p.Add("@Enqueued", DateTime.UtcNow.AddDays(-1));
            p.Add("@Completed", null);
            p.Add("@Failed", failed);
            p.Add("@Retries", 0);
            p.Add("@Headers", "");
            p.Add("@Body", "");

            await SecondaryConnection.ExecuteAsync(statement, p);
        }

        /// <summary>
        /// Proves basic cleanup deletes rows.
        /// </summary>
        /// <returns></returns>
        [TestMethod]
        public async Task CleanQueueFailed_Cleans()
        {
            var completed = DateTime.UtcNow.AddDays(-1);
            for (var i = 0; i < 10; i++)
            {
                await CreateTestRow(completed);
            }

            Assert.AreEqual(10, CountRowsInTable(QueueFailedTable));
            var olderthan = DateTime.UtcNow;

            var count = await dataAccess.CleanQueueFailed(DefaultQueue, olderthan, 10);
            Assert.AreEqual(10, count);

            Assert.AreEqual(0, CountRowsInTable(QueueFailedTable));
        }

        /// <summary>
        /// Proves that the number of rows deleted is limited.
        /// </summary>
        /// <returns></returns>
        [TestMethod]
        public async Task CleanQueueFailed_CleansTopN()
        {
            var completed = DateTime.UtcNow.AddDays(-1);
            for (var i = 0; i < 10; i++)
            {
                await CreateTestRow(completed);
            }

            Assert.AreEqual(10, CountRowsInTable(QueueFailedTable));
            var olderthan = DateTime.UtcNow;

            var count = await dataAccess.CleanQueueFailed(DefaultQueue, olderthan, 5);
            Assert.AreEqual(5, count);

            Assert.AreEqual(5, CountRowsInTable(QueueFailedTable));
        }

        /// <summary>
        /// Proves that the older than criteria is used.
        /// </summary>
        /// <returns></returns>
        [TestMethod]
        public async Task CleanQueueFailed_RespectsOlderThan()
        {
            var completed = DateTime.UtcNow;
            for (var i = 0; i < 10; i++)
            {
                await CreateTestRow(completed);
            }

            Assert.AreEqual(10, CountRowsInTable(QueueFailedTable));
            var olderthan = DateTime.UtcNow.AddMinutes(-5);

            var count = await dataAccess.CleanQueueFailed(DefaultQueue, olderthan, 10);
            Assert.AreEqual(0, count);

            Assert.AreEqual(10, CountRowsInTable(QueueFailedTable));
        }

        /// <summary>
        /// Proves that young rows are not deleted.
        /// </summary>
        /// <returns></returns>
        [TestMethod]
        public async Task CleanQueueFailed_HandlesMix()
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

            Assert.AreEqual(10, CountRowsInTable(QueueFailedTable));
            var olderthan = DateTime.UtcNow.AddMinutes(-5);

            var count = await dataAccess.CleanQueueFailed(DefaultQueue, olderthan, 10);
            Assert.AreEqual(3, count);

            Assert.AreEqual(7, CountRowsInTable(QueueFailedTable));
        }

        /// <summary>
        /// Proves that statements are not executed if the schema contains
        /// characters that are a SQL injection risk.
        /// </summary>
        [TestMethod]
        public void CleanQueueFailed_ThrowsIfSchemaUnsafe()
        {
            var action = new Action(() => dataAccess.CleanQueueFailed(DefaultQueue, DateTime.MinValue, 1));
            ActionThrowsIfSchemaContainsPoisonChars(action);
        }

        /// <summary>
        /// Proves that statements are not executed if the queue name contains
        /// characters that are a SQL injection risk.
        /// </summary>
        [TestMethod]
        public void CleanQueueFailed_ThrowsIfQueueNameUnsafe()
        {
            var action = new Action<string>((s) => dataAccess.CleanQueueFailed(s, DateTime.MinValue, 1));
            ActionThrowsIfParameterContainsPoisonChars(action);
        }
    }
}
