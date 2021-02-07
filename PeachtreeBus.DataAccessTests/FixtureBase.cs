using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using PeachtreeBus.Data;
using PeachtreeBus.DatabaseSharing;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Text;

namespace PeachtreeBus.DataAccessTests
{

    public class FixtureBase
    {
        protected SqlConnection PrimaryConnection;
        protected SqlConnection SecondaryConnection;
        protected DapperDataAccess dataAccess;
        protected Mock<IDbSchema> MockSchema;

        const string schema = "PeachtreeBus";

        protected void TestInitialize()
        {
            PrimaryConnection = new SqlConnection(AssemblyInitialize.dbConnectionString);
            PrimaryConnection.Open();

            SecondaryConnection = new SqlConnection(AssemblyInitialize.dbConnectionString);
            SecondaryConnection.Open();

            var sharedDB = new SharedDatabase(PrimaryConnection);

            MockSchema = new Mock<IDbSchema>();
            MockSchema.Setup(s => s.Schema).Returns(schema);

            dataAccess = new DapperDataAccess(sharedDB, MockSchema.Object);
        }

        protected int CountRowsInTable(string tablename)
        {
            string statment = $"SELECT COUNT(*) FROM [{schema}].[{tablename}]";
            using (var cmd = new SqlCommand(statment, SecondaryConnection))
            {
                return (int)cmd.ExecuteScalar();
            }
        }

        protected void TruncateAll()
        {
            string statment =
                $"TRUNCATE TABLE [{schema}].[QueueName_CompletedMessages]; " +
                $"TRUNCATE TABLE [{schema}].[QueueName_ErrorMessages]; " +
                $"TRUNCATE TABLE [{schema}].[QueueName_PendingMessages]; " +
                $"TRUNCATE TABLE [{schema}].[SagaName_SagaData] ";

            using (var cmd = new SqlCommand(statment, SecondaryConnection))
            {
                cmd.ExecuteNonQuery();
            }
        }

        protected DataSet GetTableContent(string tablename)
        {
            var result = new DataSet();
            string statement = $"SELECT * FROM [{schema}].[{tablename}]";
            using (var cmd = new SqlCommand(statement, SecondaryConnection))
            using (var adpater = new SqlDataAdapter(cmd))
            {
                adpater.Fill(result);
            }
            return result;
        }

        protected void AssertMessageEquals(Model.QueueMessage expected, Model.QueueMessage actual)
        {
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
                NotBefore = DateTime.UtcNow.AddMinutes(1),
                Retries = 0
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
