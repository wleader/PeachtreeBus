using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using PeachtreeBus.Data;
using PeachtreeBus.DatabaseSharing;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Text;
using System.Threading;

namespace PeachtreeBus.DataAccessTests
{

    public class FixtureBase
    {
        protected SqlConnection PrimaryConnection;
        protected SqlConnection SecondaryConnection;
        protected DapperDataAccess dataAccess;
        protected Mock<IDbSchemaConfiguration> MockSchema;

        protected const string DefaultSchema = "PeachtreeBus";
        protected const string DefaultQueue = "QueueName";
        protected const string PendingMessagesTable = DefaultQueue + "_PendingMessages";
        protected const string CompletedMessagesTable = DefaultQueue + "_CompletedMessages";
        protected const string ErrorMessagesTable = DefaultQueue + "_ErrorMessages";
        protected const string DefaultSagaName = "SagaName";
        protected const string DefaultSagaTable = DefaultSagaName + "_SagaData";

        public virtual void TestInitialize()
        {
            PrimaryConnection = new SqlConnection(AssemblyInitialize.dbConnectionString);
            PrimaryConnection.Open();

            SecondaryConnection = new SqlConnection(AssemblyInitialize.dbConnectionString);
            SecondaryConnection.Open();

            var sharedDB = new SharedDatabase(PrimaryConnection);

            MockSchema = new Mock<IDbSchemaConfiguration>();
            MockSchema.Setup(s => s.Schema).Returns(DefaultSchema);

            dataAccess = new DapperDataAccess(sharedDB, MockSchema.Object);

            BeginSecondaryTransaction();
            TruncateAll();
            CommitSecondaryTransaction();
        }

        public virtual void TestCleanup()
        {
            if (transaction != null) transaction.Rollback();

            BeginSecondaryTransaction();
            TruncateAll();
            CommitSecondaryTransaction();

            PrimaryConnection.Close();
            SecondaryConnection.Close();
        }

        protected SqlTransaction transaction = null;
        protected void BeginSecondaryTransaction()
        {
            transaction = SecondaryConnection.BeginTransaction();
        }

        protected void RollbackSecondaryTransaction()
        {
            if (transaction != null)
            {
                transaction.Rollback();
                transaction = null;
            }
        }

        protected void CommitSecondaryTransaction()
        {
            if (transaction != null)
            {
                transaction.Commit();
                transaction = null;
            }
        }

        protected int CountRowsInTable(string tablename)
        {
            string statment = $"SELECT COUNT(*) FROM [{DefaultSchema}].[{tablename}]";
            using (var cmd = new SqlCommand(statment, SecondaryConnection,transaction))
            {
                return (int)cmd.ExecuteScalar();
            }
        }

        protected void TruncateAll()
        {
            string statment =
                $"TRUNCATE TABLE [{DefaultSchema}].[{CompletedMessagesTable}]; " +
                $"TRUNCATE TABLE [{DefaultSchema}].[{ErrorMessagesTable}]; " +
                $"TRUNCATE TABLE [{DefaultSchema}].[{PendingMessagesTable}]; " +
                $"TRUNCATE TABLE [{DefaultSchema}].[{DefaultSagaTable}] ";

            using (var cmd = new SqlCommand(statment, SecondaryConnection, transaction))
            {
                cmd.ExecuteNonQuery();
            }
        }

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

        protected void AssertSqlDbDateTime(DateTime? expected, DateTime? actual)
        {
            if (expected.HasValue && actual.HasValue)
                AssertSqlDbDateTime(expected.Value, actual.Value);

            if (expected.HasValue != actual.HasValue)
                Assert.Fail($"Expected {expected}, Actual {actual}");

            // else both are null and match.
        }

        protected void AssertSqlDbDateTime(DateTime expected, DateTime actual)
        {
            Assert.AreEqual(expected.Year, actual.Year);
            Assert.AreEqual(expected.Month, actual.Month);
            Assert.AreEqual(expected.Day, actual.Day);
            Assert.AreEqual(expected.Hour, actual.Hour);
            Assert.AreEqual(expected.Minute, actual.Minute);
            Assert.AreEqual(expected.Second, actual.Second);

            // date times the get stored in SQL because of the way things are stored can
            // be off by a few ms, so just make sure its close
            Assert.IsTrue(actual.Millisecond < expected.Millisecond + 3, $"Millisecond Mismatch {expected.Millisecond} {actual.Millisecond}");
            Assert.IsTrue(actual.Millisecond > expected.Millisecond - 3, $"Millisecond Mismatch {expected.Millisecond} {actual.Millisecond}");

            Assert.AreEqual(expected.Kind, actual.Kind);
        }

        protected void ActionThrowsIfSchemaContainsPoisonChars(Action action)
        {
            var poison = new char[] { '\'', ';', '@', '-', '/', '*' };
            foreach (var p in poison)
            {
                ActionThrowsIfShcemaContains(action, p.ToString());
            }
        }

        protected void ActionThrowsIfShcemaContains(Action action, string poison)
        {
            var exceptionThrown = false;
            MockSchema.Setup(s => s.Schema).Returns(poison);
            try
            {
                action.Invoke();
            }
            catch (ArgumentException)
            {
                exceptionThrown = true;
            }
            Assert.IsTrue(exceptionThrown, "Action did not throw an argument exception for an unsafe schema name.");
        }

        protected void ActionThrowsIfParameterContainsPoisonChars(Action<string> action)
        {
            var poison = new char[] { '\'', ';', '@', '-', '/', '*' };
            foreach (var p in poison)
            {
                ActionThrowsIfParameterContains(action, p.ToString());
            }
        }

        protected void ActionThrowsIfParameterContains(Action<string> action, string poison)
        {
            var exceptionThrown = false;
            try
            {
                action.Invoke(poison);
            }
            catch (ArgumentException)
            {
                exceptionThrown = true;
            }
            Assert.IsTrue(exceptionThrown, "Action did not throw an argument exception for an unsafe Parameter.");
        }

        protected Model.QueueMessage CreateTestMessage()
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
        protected void ActionThrowsForMessagesWithUnspecifiedDateTimeKinds(Action<Model.QueueMessage> action)
        {
            var poisonEnqueued = CreateTestMessage();
            poisonEnqueued.Enqueued = DateTime.SpecifyKind(poisonEnqueued.Enqueued, DateTimeKind.Unspecified);
            ActionThrowsForMessage(action, poisonEnqueued);
            
            var poisonNotBefore = CreateTestMessage();
            poisonNotBefore.NotBefore = DateTime.SpecifyKind(poisonNotBefore.NotBefore, DateTimeKind.Unspecified);
            ActionThrowsForMessage(action, poisonNotBefore);
            
            var poisonCompleted = CreateTestMessage();
            poisonCompleted.Completed = DateTime.SpecifyKind(DateTime.Now, DateTimeKind.Unspecified);
            ActionThrowsForMessage(action, poisonCompleted);
            
            var poisonFailed = CreateTestMessage();
            poisonFailed.Failed = DateTime.SpecifyKind(DateTime.Now, DateTimeKind.Unspecified);
            ActionThrowsForMessage(action, poisonFailed);
        }

        protected void ActionThrowsForMessage(Action<Model.QueueMessage> action, Model.QueueMessage message)
        {
            var exceptionThrown = false;
            try
            {
                action.Invoke(message);
            }
            catch (ArgumentException)
            {
                exceptionThrown = true;
            }
            Assert.IsTrue(exceptionThrown, "Action did not throw an argument exception for poison message.");
        }
    }
}
