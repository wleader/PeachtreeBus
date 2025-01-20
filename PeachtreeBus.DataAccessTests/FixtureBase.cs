using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using PeachtreeBus.Data;
using PeachtreeBus.DatabaseSharing;
using PeachtreeBus.Queues;
using PeachtreeBus.Sagas;
using PeachtreeBus.Subscriptions;
using System;
using System.Data;

namespace PeachtreeBus.DataAccessTests
{
    /// <summary>
    /// A base class that contains code useful in multiple tests.
    /// </summary>
    public abstract class FixtureBase<TAccess>
    {
        /// <summary>
        /// A DB Connection provided to the DapperDataAccess.
        /// </summary>
        protected ISqlConnection PrimaryConnection = default!;

        /// <summary>
        /// A secondary connection used by the tests to act on the DB
        /// without using the DapperDataAccess.
        /// </summary>
        protected ISqlConnection SecondaryConnection = default!;

        protected SharedDatabase SharedDB = default!;

        private readonly Mock<ISqlConnectionFactory> _connectionFactory = new();

        /// <summary>
        /// The data acess being tested.
        /// </summary>
        protected TAccess dataAccess = default!;

        /// <summary>
        /// Provides a schema to the data access.
        /// </summary>
        protected Mock<IDbSchemaConfiguration> MockSchema = default!;

        /// <summary>
        /// Provides a log to the data access.
        /// </summary>
        protected Mock<ILogger<TAccess>> MockLog = default!;

        protected readonly SchemaName DefaultSchema = new("PeachtreeBus");
        protected const string DefaultQueueStr = "QueueName";
        protected readonly QueueName DefaultQueue = new(DefaultQueueStr);
        protected const string QueuePendingTable = DefaultQueueStr + "_Pending";
        protected const string QueueCompletedTable = DefaultQueueStr + "_Completed";
        protected const string QueueFailedTable = DefaultQueueStr + "_Failed";
        protected const string DefaultSagaNameStr = "SagaName";
        protected readonly SagaName DefaultSagaName = new(DefaultSagaNameStr);
        protected const string DefaultSagaTable = DefaultSagaNameStr + "_SagaData";
        protected const string SubscriptionsTable = "Subscriptions";
        protected const string SubscribedPendingTable = "Subscribed_Pending";
        protected const string SubscribedFailedTable = "Subscribed_Failed";
        protected const string SubscribedCompletedTable = "Subscribed_Completed";

        /// <summary>
        /// Performs tasks that happen before each test.
        /// </summary>
        public virtual void TestInitialize()
        {
            // Create connections.
            PrimaryConnection = new SqlConnectionProxy(AssemblyInitialize.DbConnectionString);

            SecondaryConnection = new SqlConnectionProxy(AssemblyInitialize.DbConnectionString);
            SecondaryConnection.Open();

            // create the data access object.
            _connectionFactory.Setup(f => f.GetConnection()).Returns(() =>
            {
                if (PrimaryConnection.Disposed)
                    PrimaryConnection = new SqlConnectionProxy(AssemblyInitialize.DbConnectionString);
                return PrimaryConnection;
            });
            SharedDB = new SharedDatabase(_connectionFactory.Object);

            MockSchema = new Mock<IDbSchemaConfiguration>();
            MockSchema.Setup(s => s.Schema).Returns(DefaultSchema);

            MockLog = new Mock<ILogger<TAccess>>();

            dataAccess = CreateDataAccess();

            // start all tests with an Empty DB.
            // each test will setup rows it needs.
            BeginSecondaryTransaction();
            TruncateAll();
            CommitSecondaryTransaction();
        }

        protected abstract TAccess CreateDataAccess();

        /// <summary>
        /// Performs tasks that happen after each test.
        /// </summary>
        public virtual void TestCleanup()
        {
            // rollback any uncommitted transaction.
            transaction?.Rollback();

            // Cleanup up any data left behind by the test.
            BeginSecondaryTransaction();
            TruncateAll();
            CommitSecondaryTransaction();

            // close the connections.
            PrimaryConnection.Close();
            SecondaryConnection.Close();
        }

        /// <summary>
        /// Holds any transaction started by test code.
        /// </summary>
        protected ISqlTransaction? transaction = null;

        /// <summary>
        /// starts a transaction on the secondary connection.
        /// </summary>
        protected void BeginSecondaryTransaction()
        {
            transaction = SecondaryConnection.BeginTransaction();
        }

        /// <summary>
        /// Rolls back the transaction on the secondary connection.
        /// </summary>
        protected void RollbackSecondaryTransaction()
        {
            if (transaction != null)
            {
                transaction.Rollback();
                transaction = null;
            }
        }

        /// <summary>
        /// Commits the transaction on the secondary connection.
        /// </summary>
        protected void CommitSecondaryTransaction()
        {
            if (transaction != null)
            {
                transaction.Commit();
                transaction = null;
            }
        }

        /// <summary>
        /// Uses the secondary connection to count the rows in a table.
        /// </summary>
        /// <param name="tablename"></param>
        /// <returns></returns>
        protected int CountRowsInTable(string tablename)
        {
            string statment = $"SELECT COUNT(*) FROM [{DefaultSchema}].[{tablename}]";
            using var cmd = new SqlCommand(statment, SecondaryConnection.Connection, transaction?.Transaction);
            return (int)cmd.ExecuteScalar();
        }

        /// <summary>
        /// Truncates all the tables for the test using the secondary connection.
        /// </summary>
        protected void TruncateAll()
        {
            string statement =
                $"TRUNCATE TABLE [{DefaultSchema}].[{QueueCompletedTable}]; " +
                $"TRUNCATE TABLE [{DefaultSchema}].[{QueueFailedTable}]; " +
                $"TRUNCATE TABLE [{DefaultSchema}].[{QueuePendingTable}]; " +
                $"TRUNCATE TABLE [{DefaultSchema}].[{DefaultSagaTable}]; " +
                $"TRUNCATE TABLE [{DefaultSchema}].[{SubscriptionsTable}]; " +
                $"TRUNCATE TABLE [{DefaultSchema}].[{SubscribedPendingTable}]; " +
                $"TRUNCATE TABLE [{DefaultSchema}].[{SubscribedFailedTable}]; " +
                $"TRUNCATE TABLE [{DefaultSchema}].[{SubscribedCompletedTable}]; ";
            ExecuteNonQuery(statement);
        }

        /// <summary>
        /// Executes a statement on the secondary connection.
        /// </summary>
        /// <param name="statement"></param>
        protected void ExecuteNonQuery(string statement)
        {
            using var cmd = new SqlCommand(statement, SecondaryConnection.Connection, transaction?.Transaction);
            cmd.ExecuteNonQuery();
        }

        /// <summary>
        /// Gets everything in a table as a dataset using the secondary connection.
        /// </summary>
        /// <param name="tablename"></param>
        /// <returns></returns>
        protected DataSet GetTableContent(string tablename)
        {
            var result = new DataSet();
            string statement = $"SELECT * FROM [{DefaultSchema}].[{tablename}]";
            using (var cmd = new SqlCommand(statement, SecondaryConnection.Connection, transaction?.Transaction))
            using (var adpater = new SqlDataAdapter(cmd))
            {
                adpater.Fill(result);
            }
            return result;
        }

        /// <summary>
        /// Gets everything in a table as a dataset, locks the selected rows, and reads past locked rows.
        /// Uses the secondary connection.
        /// </summary>
        /// <param name="tablename"></param>
        /// <returns></returns>
        protected DataSet GetTableContentAndLock(string tablename)
        {
            var result = new DataSet();
            string statement = $"SELECT * FROM [{DefaultSchema}].[{tablename}] WITH (UPDLOCK, READPAST)";
            using (var cmd = new SqlCommand(statement, SecondaryConnection.Connection, transaction?.Transaction))
            using (var adpater = new SqlDataAdapter(cmd))
            {
                adpater.Fill(result);
            }
            return result;
        }

        /// <summary>
        /// Tests two SagaDatas are equal.
        /// </summary>
        /// <param name="expected"></param>
        /// <param name="actual"></param>
        protected void AssertSagaEquals(SagaData expected, SagaData actual)
        {
            if (expected == null && actual == null) return;
            Assert.IsNotNull(actual, "Actual is null, expected is not.");
            Assert.IsNotNull(expected, "Expected is null, actual is not.");
            Assert.AreEqual(expected.Id, actual.Id);
            Assert.AreEqual(expected.Data, actual.Data);
            Assert.AreEqual(expected.SagaId, actual.SagaId);
            Assert.AreEqual(expected.Key, actual.Key);
            // don't check the blocked because its not really part of the
            // entity. Test that as needed in tests.
            //Assert.AreEqual(expected.Blocked, actual.Blocked);
        }

        /// <summary>
        /// Tests that two QueueMessage are equal.
        /// </summary>
        /// <param name="expected"></param>
        /// <param name="actual"></param>
        protected void AssertMessageEquals(QueueMessage expected, QueueMessage actual)
        {
            if (expected == null && actual == null) return;
            Assert.IsNotNull(actual, "Actual is null, expected is not.");
            Assert.IsNotNull(expected, "Expected is null, actual is not.");
            Assert.AreEqual(expected.Headers, actual.Headers);
            Assert.AreEqual(expected.MessageId, actual.MessageId);
            AssertSqlDbDateTime(expected.NotBefore, actual.NotBefore);
            Assert.AreEqual(expected.Id, actual.Id);
            Assert.AreEqual(expected.Body, actual.Body);
            AssertSqlDbDateTime(expected.Completed, actual.Completed);
            AssertSqlDbDateTime(expected.Enqueued, actual.Enqueued);
            AssertSqlDbDateTime(expected.Failed, actual.Failed);
            Assert.AreEqual(expected.Retries, actual.Retries);
        }

        /// <summary>
        /// Tests that two SubscribedMessage are equal.
        /// </summary>
        /// <param name="expected"></param>
        /// <param name="actual"></param>
        protected void AssertSubscribedEquals(SubscribedMessage expected, SubscribedMessage actual)
        {
            if (expected == null && actual == null) return;
            Assert.IsNotNull(actual, "Actual is null, expected is not.");
            Assert.IsNotNull(expected, "Expected is null, actual is not.");
            Assert.AreEqual(expected.Headers, actual.Headers);
            Assert.AreEqual(expected.MessageId, actual.MessageId);
            AssertSqlDbDateTime(expected.NotBefore, actual.NotBefore);
            Assert.AreEqual(expected.Id, actual.Id);
            Assert.AreEqual(expected.Body, actual.Body);
            AssertSqlDbDateTime(expected.Completed, actual.Completed);
            AssertSqlDbDateTime(expected.Enqueued, actual.Enqueued);
            AssertSqlDbDateTime(expected.Failed, actual.Failed);
            Assert.AreEqual(expected.Retries, actual.Retries);
            Assert.AreEqual(expected.SubscriberId, actual.SubscriberId);
            AssertSqlDbDateTime(expected.ValidUntil, actual.ValidUntil);
        }

        /// <summary>
        /// Tests that two nullable DateTime values are equal.
        /// </summary>
        /// <param name="expected"></param>
        /// <param name="actual"></param>
        /// <param name="allowDriftMs">Allows a minor difference in times.</param>
        protected void AssertSqlDbDateTime(DateTime? expected, DateTime? actual, int allowDriftMs = 100)
        {
            if (expected.HasValue && actual.HasValue)
                AssertSqlDbDateTime(expected.Value, actual.Value, allowDriftMs);

            if (expected.HasValue != actual.HasValue)
                Assert.Fail($"Expected {expected}, Actual {actual}");

            // else both are null and match.
        }

        /// <summary>
        /// Tests that two DateTime values are equal.
        /// </summary>
        /// <param name="expected"></param>
        /// <param name="actual"></param>
        /// <param name="allowDriftMs">Allows for a minor difference in times.</param>
        protected void AssertSqlDbDateTime(DateTime expected, DateTime actual, int allowDriftMs = 100)
        {
            Assert.AreEqual(expected.Kind, actual.Kind);

            // date times the get stored in SQL, and because of the way things are stored
            // they can be off by a few ms, so just make sure its close
            var actualDrift = Math.Abs(expected.Subtract(actual).TotalMilliseconds);
            Assert.IsTrue(actualDrift < allowDriftMs);
        }

        /// <summary>
        /// Creates a new QueueMessage
        /// </summary>
        /// <returns></returns>
        protected QueueMessage CreateQueueMessage()
        {
            return new QueueMessage
            {
                Body = new("Body"),
                Completed = null,
                Failed = null,
                Enqueued = DateTime.UtcNow,
                Headers = new("Headers"),
                MessageId = Guid.NewGuid(),
                NotBefore = DateTime.UtcNow,
                Retries = 0
            };
        }

        /// <summary>
        /// Creates a new SubscribedMessage
        /// </summary>
        /// <returns></returns>
        protected SubscribedMessage CreateSubscribed()
        {
            return new SubscribedMessage
            {
                Body = new("Body"),
                Completed = null,
                Failed = null,
                Enqueued = DateTime.UtcNow,
                Headers = new("Headers"),
                MessageId = Guid.NewGuid(),
                NotBefore = DateTime.UtcNow,
                Retries = 0,
                SubscriberId = Guid.Empty,
                ValidUntil = DateTime.UtcNow.AddDays(1)
            };
        }

        /// <summary>
        /// Creates a new SagaData
        /// </summary>
        /// <returns></returns>
        protected SagaData CreateTestSagaData()
        {
            return new SagaData
            {
                Blocked = false,
                Data = new("Data"),
                SagaId = Guid.NewGuid(),
                Key = "Key"
            };
        }
    }
}
