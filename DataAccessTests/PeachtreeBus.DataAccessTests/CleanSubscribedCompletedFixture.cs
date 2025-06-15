using Dapper;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Threading.Tasks;

namespace PeachtreeBus.DataAccessTests
{
    /// <summary>
    /// Proves the behavior of DapperDataAcess.CleanSubscribedCompleted
    /// </summary>
    [TestClass]
    public class CleanSubscribedCompletedFixture : DapperDataAccessFixtureBase
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
        /// Adds a row for the test.
        /// </summary>
        /// <param name="completed">The Compelted time for the subscribed message.</param>
        /// <returns></returns>
        private async Task CreateTestRow(DateTime completed)
        {
            // puts a row in the completed table.
            var statement =
            """
            INSERT INTO [{0}].[Subscribed_Completed]
            ([Id],[SubscriberId],[Topic],[ValidUntil],[MessageId],[Priority],[NotBefore],[Enqueued],[Completed],[Failed],[Retries],[Headers],[Body])
            VALUES
            (@Id, @SubscriberId, @Topic, @ValidUntil, @MessageId, @Priority, @NotBefore, @Enqueued, @Completed, @Failed, @Retries, @Headers, @Body)
            """;

            statement = string.Format(statement, DefaultSchema, DefaultQueue);

            var p = new DynamicParameters();
            p.Add("@Id", lastId++);
            p.Add("@SubscriberId", Guid.NewGuid());
            p.Add("@ValidUntil", DateTime.MaxValue);
            p.Add("@MessageId", Guid.NewGuid());
            p.Add("@Priority", 0);
            p.Add("@NotBefore", DateTime.UtcNow.AddDays(-1));
            p.Add("@Enqueued", DateTime.UtcNow.AddDays(-1));
            p.Add("@Completed", completed);
            p.Add("@Failed", null);
            p.Add("@Retries", 0);
            p.Add("@Headers", "");
            p.Add("@Body", "");
            p.Add("@Topic", "Topic");

            await SecondaryConnection.Connection.ExecuteAsync(statement, p);
        }

        /// <summary>
        /// Proves the basic cleaning functionality.
        /// </summary>
        /// <returns></returns>
        [TestMethod]
        public async Task CleanSubscribedCompleted_Cleans()
        {
            var completed = DateTime.UtcNow.AddDays(-1);
            for (var i = 0; i < 10; i++)
            {
                await CreateTestRow(completed);
            }

            Assert.AreEqual(10, CountRowsInTable(SubscribedCompleted));
            var olderthan = DateTime.UtcNow;

            var count = await dataAccess.CleanSubscribedCompleted(olderthan, 10);
            Assert.AreEqual(10, count);

            Assert.AreEqual(0, CountRowsInTable(SubscribedCompleted));
        }

        /// <summary>
        /// Proves that cleanup is limited to the specified number of rows.
        /// </summary>
        /// <returns></returns>
        [TestMethod]
        public async Task CleanSubscribedCompleted_CleansTopN()
        {
            var completed = DateTime.UtcNow.AddDays(-1);
            for (var i = 0; i < 10; i++)
            {
                await CreateTestRow(completed);
            }

            Assert.AreEqual(10, CountRowsInTable(SubscribedCompleted));
            var olderthan = DateTime.UtcNow;

            var count = await dataAccess.CleanSubscribedCompleted(olderthan, 5);
            Assert.AreEqual(5, count);

            Assert.AreEqual(5, CountRowsInTable(SubscribedCompleted));
        }

        /// <summary>
        /// Proves that cleanup will not delete rows that completed after the specified time.
        /// </summary>
        /// <returns></returns>
        [TestMethod]
        public async Task CleanSubscribedCompleted_RespectsOlderThan()
        {
            var completed = DateTime.UtcNow;
            for (var i = 0; i < 10; i++)
            {
                await CreateTestRow(completed);
            }

            Assert.AreEqual(10, CountRowsInTable(SubscribedCompleted));
            var olderthan = DateTime.UtcNow.AddMinutes(-5);

            var count = await dataAccess.CleanSubscribedCompleted(olderthan, 10);
            Assert.AreEqual(0, count);

            Assert.AreEqual(10, CountRowsInTable(SubscribedCompleted));
        }

        /// <summary>
        /// Proves that cleanup deletes old rows and not young ones.
        /// </summary>
        /// <returns></returns>
        [TestMethod]
        public async Task CleanSubscribedCompleted_HandlesMix()
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

            Assert.AreEqual(10, CountRowsInTable(SubscribedCompleted));
            var olderthan = DateTime.UtcNow.AddMinutes(-5);

            var count = await dataAccess.CleanSubscribedCompleted(olderthan, 10);
            Assert.AreEqual(3, count);

            Assert.AreEqual(7, CountRowsInTable(SubscribedCompleted));
        }
    }
}
