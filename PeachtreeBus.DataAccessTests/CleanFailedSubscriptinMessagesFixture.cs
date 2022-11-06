using Dapper;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Threading.Tasks;

namespace PeachtreeBus.DataAccessTests
{
    /// <summary>
    /// Proves the behavior of DapperDataAcess.CleanSubscribedFailed
    /// </summary>
    [TestClass]
    public class CleanSubscribedFailedFixture : FixtureBase
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
        /// Adds a failed subscribed message for the tests.
        /// </summary>
        /// <param name="failed"></param>
        /// <returns></returns>
        private async Task CreateTestRow(DateTime failed)
        {
            // puts a row in the completed table.
            var statement =
            "INSERT INTO [{0}].[Subscribed_Failed] " +
            "([Id],[SubscriberId],[ValidUntil],[MessageId],[NotBefore],[Enqueued],[Completed],[Failed],[Retries],[Headers],[Body]) " +
            "VALUES " +
            "(@Id, @SubscriberId, @ValidUntil, @MessageId, @NotBefore, @Enqueued, @Completed, @Failed, @Retries, @Headers, @Body)";

            statement = string.Format(statement, DefaultSchema, DefaultQueue);

            var p = new DynamicParameters();
            p.Add("@Id", lastId++);
            p.Add("@SubscriberId", Guid.NewGuid());
            p.Add("@ValidUntil", DateTime.MaxValue);
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
        /// Proves that rows get deleted.
        /// </summary>
        /// <returns></returns>
        [TestMethod]
        public async Task CleanSubscribedFailed_Cleans()
        {
            var completed = DateTime.UtcNow.AddDays(-1);
            for (var i = 0; i < 10; i++)
            {
                await CreateTestRow(completed);
            }

            Assert.AreEqual(10, CountRowsInTable(SubscribedFailedTable));
            var olderthan = DateTime.UtcNow;

            var count = await dataAccess.CleanSubscribedFailed(olderthan, 10);
            Assert.AreEqual(10, count);

            Assert.AreEqual(0, CountRowsInTable(SubscribedFailedTable));
        }

        /// <summary>
        /// Proves that the number of rows deleted is limited.
        /// </summary>
        /// <returns></returns>
        [TestMethod]
        public async Task CleanSubscribedFailed_CleansTopN()
        {
            var completed = DateTime.UtcNow.AddDays(-1);
            for (var i = 0; i < 10; i++)
            {
                await CreateTestRow(completed);
            }

            Assert.AreEqual(10, CountRowsInTable(SubscribedFailedTable));
            var olderthan = DateTime.UtcNow;

            var count = await dataAccess.CleanSubscribedFailed(olderthan, 5);
            Assert.AreEqual(5, count);

            Assert.AreEqual(5, CountRowsInTable(SubscribedFailedTable));
        }

        /// <summary>
        /// Proves that only older rows are deleted.
        /// </summary>
        /// <returns></returns>
        [TestMethod]
        public async Task CleanSubscribedFailed_RespectsOlderThan()
        {
            var completed = DateTime.UtcNow;
            for (var i = 0; i < 10; i++)
            {
                await CreateTestRow(completed);
            }

            Assert.AreEqual(10, CountRowsInTable(SubscribedFailedTable));
            var olderthan = DateTime.UtcNow.AddMinutes(-5);

            var count = await dataAccess.CleanSubscribedFailed(olderthan, 10);
            Assert.AreEqual(0, count);

            Assert.AreEqual(10, CountRowsInTable(SubscribedFailedTable));
        }

        /// <summary>
        /// Proves that younger rows are not deleted.
        /// </summary>
        /// <returns></returns>
        [TestMethod]
        public async Task CleanSubscribedFailed_HandlesMix()
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

            Assert.AreEqual(10, CountRowsInTable(SubscribedFailedTable));
            var olderthan = DateTime.UtcNow.AddMinutes(-5);

            var count = await dataAccess.CleanSubscribedFailed(olderthan, 10);
            Assert.AreEqual(3, count);

            Assert.AreEqual(7, CountRowsInTable(SubscribedFailedTable));
        }

        /// <summary>
        /// Proves that statements do not run if the schema contains
        /// characters that are a SQL injection risk.
        /// </summary>
        [TestMethod]
        public async Task CleanSubscribedFailed_ThrowsIfSchemaUnsafe()
        {
            var action = new Func<Task>(async () => await dataAccess.CleanSubscribedFailed(DateTime.MinValue, 1));
            await ActionThrowsIfSchemaContainsPoisonChars(action);
        }
    }
}
