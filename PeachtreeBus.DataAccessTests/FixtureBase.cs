using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using PeachtreeBus.Data;
using PeachtreeBus.DatabaseSharing;
using System;
using System.Data;
using System.Data.SqlClient;
using System.Threading.Tasks;

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
        protected SqlConnection PrimaryConnection;

        /// <summary>
        /// A secondary connection used by the tests to act on the DB
        /// without using the DapperDataAccess.
        /// </summary>
        protected SqlConnection SecondaryConnection;

        protected SharedDatabase SharedDB;

        private Mock<ISqlConnectionFactory> _connectionFactory = new();

        /// <summary>
        /// The data acess being tested.
        /// </summary>
        protected TAccess dataAccess;

        /// <summary>
        /// Provides a schema to the data access.
        /// </summary>
        protected Mock<IDbSchemaConfiguration> MockSchema;

        /// <summary>
        /// Provides a log to the data access.
        /// </summary>
        protected Mock<ILogger<TAccess>> MockLog;

        protected const string DefaultSchema = "PeachtreeBus";
        protected const string DefaultQueue = "QueueName";
        protected const string QueuePendingTable = DefaultQueue + "_Pending";
        protected const string QueueCompletedTable = DefaultQueue + "_Completed";
        protected const string QueueFailedTable = DefaultQueue + "_Failed";
        protected const string DefaultSagaName = "SagaName";
        protected const string DefaultSagaTable = DefaultSagaName + "_SagaData";
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
            PrimaryConnection = new SqlConnection(AssemblyInitialize.dbConnectionString);

            SecondaryConnection = new SqlConnection(AssemblyInitialize.dbConnectionString);
            SecondaryConnection.Open();

            // create the data access object.
            _connectionFactory.Setup(f => f.GetConnection()).Returns(PrimaryConnection);
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
        protected SqlTransaction transaction = null;

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
            using var cmd = new SqlCommand(statment, SecondaryConnection, transaction);
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
            using var cmd = new SqlCommand(statement, SecondaryConnection, transaction);
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
            using (var cmd = new SqlCommand(statement, SecondaryConnection, transaction))
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
            using (var cmd = new SqlCommand(statement, SecondaryConnection, transaction))
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
        protected void AssertSagaEquals(Model.SagaData expected, Model.SagaData actual)
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
        protected void AssertMessageEquals(Model.QueueMessage expected, Model.QueueMessage actual)
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
        protected void AssertSubscribedEquals(Model.SubscribedMessage expected, Model.SubscribedMessage actual)
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
        /// Helper to test that an action throws an exception when the schema is unsafe.
        /// </summary>
        /// <param name="action"></param>
        protected async Task ActionThrowsIfSchemaContainsPoisonChars(Func<Task> action)
        {
            var poison = new char[] { '\'', ';', '@', '-', '/', '*' };
            foreach (var p in poison)
            {
                await ActionThrowsIfShcemaContains(action, p.ToString());
            }
        }

        /// <summary>
        /// Helper to test that an action throws an exception when the schema contains
        /// a specific character.
        /// </summary>
        /// <param name="action"></param>
        /// <param name="poison"></param>
        protected async Task ActionThrowsIfShcemaContains(Func<Task> action, string poison)
        {
            var exceptionThrown = false;
            MockSchema.Setup(s => s.Schema).Returns(poison);
            try
            {
                await action();
            }
            catch (ArgumentException)
            {
                exceptionThrown = true;
            }
            Assert.IsTrue(exceptionThrown, "Action did not throw an argument exception for an unsafe schema name.");
        }

        /// <summary>
        /// Helper function to test that an action throws an exception when a
        /// parameter contains unsafe chacters
        /// </summary>
        /// <param name="action"></param>
        protected async Task ActionThrowsIfParameterContainsPoisonChars(Func<string, Task> action)
        {
            var poison = new char[] { '\'', ';', '@', '-', '/', '*' };
            foreach (var p in poison)
            {
                await ActionThrowsIfParameterContains(action, p.ToString());
            }
        }

        /// <summary>
        /// Helper function test that an action throws an exception when a parameter
        /// contains a specific character.
        /// </summary>
        /// <param name="action"></param>
        /// <param name="poison"></param>
        protected async Task ActionThrowsIfParameterContains(Func<string, Task> action, string poison)
        {
            var exceptionThrown = false;
            try
            {
                await action.Invoke(poison);
            }
            catch (ArgumentException)
            {
                exceptionThrown = true;
            }
            Assert.IsTrue(exceptionThrown, "Action did not throw an argument exception for an unsafe Parameter.");
        }

        /// <summary>
        /// Creates a new QueueMessage
        /// </summary>
        /// <returns></returns>
        protected Model.QueueMessage CreateQueueMessage()
        {
            return new Model.QueueMessage
            {
                Body = "Body",
                Completed = null,
                Failed = null,
                Enqueued = DateTime.UtcNow,
                Headers = "Headers",
                MessageId = Guid.NewGuid(),
                NotBefore = DateTime.UtcNow,
                Retries = 0
            };
        }

        /// <summary>
        /// Creates a new SubscribedMessage
        /// </summary>
        /// <returns></returns>
        protected Model.SubscribedMessage CreateSubscribed()
        {
            return new Model.SubscribedMessage
            {
                Body = "Body",
                Completed = null,
                Failed = null,
                Enqueued = DateTime.UtcNow,
                Headers = "Headers",
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
        protected Model.SagaData CreateTestSagaData()
        {
            return new Model.SagaData
            {
                Blocked = false,
                Data = "Data",
                SagaId = Guid.NewGuid(),
                Key = "Key"
            };
        }

        /// <summary>
        /// Helper method that tests that an action using 
        /// </summary>
        /// <param name="action"></param>
        /// <param name="message"></param>
        protected async Task ActionThrowsFor<T>(Func<T, Task> action, T message)
        {
            var exceptionThrown = false;
            try
            {
                await action.Invoke(message);
            }
            catch (ArgumentException)
            {
                exceptionThrown = true;
            }
            Assert.IsTrue(exceptionThrown, "Action did not throw an argument exception for poison message.");
        }
    }
}
