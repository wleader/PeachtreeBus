using Dapper;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using PeachtreeBus.Core.Tests;
using PeachtreeBus.Data;
using PeachtreeBus.DatabaseSharing;
using PeachtreeBus.Queues;
using PeachtreeBus.Sagas;
using PeachtreeBus.Serialization;
using PeachtreeBus.Subscriptions;
using System;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;

namespace PeachtreeBus.DataAccessTests;

/// <summary>
/// A base class that contains code useful in multiple tests.
/// </summary>
public abstract class FixtureBase<TAccess> : TestConfig
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
    protected DapperMethods DapperMethods = default!;

    private readonly Mock<ISqlConnectionFactory> _connectionFactory = new();

    /// <summary>
    /// The data acess being tested.
    /// </summary>
    protected TAccess dataAccess = default!;

    protected Mock<IBusConfiguration> Configuration = default!;

    /// <summary>
    /// Provides a log to the data access.
    /// </summary>
    protected Mock<ILogger<TAccess>> MockLog = default!;



    /// <summary>
    /// Performs tasks that happen before each test.
    /// </summary>
    public virtual void TestInitialize()
    {
        // Create connections.
        PrimaryConnection = new SqlConnectionProxy(DbConnectionString);

        SecondaryConnection = new SqlConnectionProxy(DbConnectionString);
        SecondaryConnection.Open();

        // create the data access object.
        _connectionFactory.Setup(f => f.GetConnection()).Returns(() =>
        {
            if (PrimaryConnection.Disposed)
                PrimaryConnection = new SqlConnectionProxy(DbConnectionString);
            return PrimaryConnection;
        });
        SharedDB = new SharedDatabase(_connectionFactory.Object);
        DapperMethods = new(new DapperTypesHandler(new DefaultSerializer()), SharedDB);

        Configuration = new();
        Configuration.SetupGet(s => s.Schema).Returns(DefaultSchema);

        MockLog = new Mock<ILogger<TAccess>>();

        dataAccess = CreateDataAccess();

        // start all tests with an Empty DB.
        // each test will setup rows it needs.

        CleanupEverything();
    }

    protected abstract TAccess CreateDataAccess();

    /// <summary>
    /// Performs tasks that happen after each test.
    /// </summary>
    public virtual void TestCleanup()
    {
        // Cleanup up any data left behind by the test.
        CleanupEverything();

        // close the connections.
        PrimaryConnection.Close();
        SecondaryConnection.Close();
    }

    /// <summary>
    /// Holds any transaction started by test code.
    /// </summary>
    protected ISqlTransaction? transaction = null;

    private void CleanupEverything()
    {
        using var transaction = SecondaryConnection.BeginTransaction();
        string statement =
            $"TRUNCATE TABLE [{DefaultSchema}].[{QueueCompleted}]; " +
            $"TRUNCATE TABLE [{DefaultSchema}].[{QueueFailed}]; " +
            $"TRUNCATE TABLE [{DefaultSchema}].[{QueuePending}]; " +
            $"TRUNCATE TABLE [{DefaultSchema}].[{SagaData}]; " +
            $"TRUNCATE TABLE [{DefaultSchema}].[{Subscriptions}]; " +
            $"TRUNCATE TABLE [{DefaultSchema}].[{SubscribedPending}]; " +
            $"TRUNCATE TABLE [{DefaultSchema}].[{SubscribedFailed}]; " +
            $"TRUNCATE TABLE [{DefaultSchema}].[{SubscribedCompleted}]; ";
        using var cmd = new SqlCommand(statement, SecondaryConnection.Connection, transaction.Transaction);
        cmd.ExecuteNonQuery();
        transaction.Commit();
    }

    /// <summary>
    /// Uses the secondary connection to count the rows in a table.
    /// </summary>
    /// <param name="table"></param>
    /// <returns></returns>
    protected int CountRowsInTable(TableName table)
    {
        string statment = $"SELECT COUNT(*) FROM [{DefaultSchema}].[{table}]";
        using var cmd = new SqlCommand(statment, SecondaryConnection.Connection, null);
        return (int)cmd.ExecuteScalar();
    }

    /// <summary>
    /// Gets everything in a table as a dataset using the secondary connection.
    /// </summary>
    /// <param name="tablename"></param>
    /// <returns></returns>
    protected DataSet GetTableContent(TableName tablename)
    {
        var result = new DataSet();
        string statement = $"SELECT * FROM [{DefaultSchema}].[{tablename}]";
        using (var cmd = new SqlCommand(statement, SecondaryConnection.Connection, null))
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
        Assert.IsFalse(expected == null && actual == null, "Do not assert Null is Null");
        Assert.IsNotNull(actual, "Actual is null, expected is not.");
        Assert.IsNotNull(expected, "Expected is null, actual is not.");
        Assert.AreEqual(expected.Id, actual.Id);
        Assert.AreEqual(expected.Data, actual.Data);
        Assert.AreEqual(expected.SagaId, actual.SagaId);
        Assert.AreEqual(expected.Key, actual.Key);
        // don't check the blocked because its not really part of the
        // entity. Test that as needed in tests.
        //Assert.AreEqual(expected.Blocked, actual.Blocked);
        Assert.AreEqual(expected.MetaData, actual.MetaData);
    }

    /// <summary>
    /// Tests that two QueueMessage are equal.
    /// </summary>
    /// <param name="expected"></param>
    /// <param name="actual"></param>
    protected void AssertQueueDataAreEqual(QueueData expected, QueueData actual)
    {
        Assert.IsFalse(expected is null && actual is null, "Do not assert Null is Null.");
        Assert.IsNotNull(actual, "Actual is null, expected is not.");
        Assert.IsNotNull(expected, "Expected is null, actual is not.");
        AssertHeadersEquals(expected.Headers, actual.Headers);
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
    protected void AssertSubscribedEquals(SubscribedData expected, SubscribedData actual)
    {
        Assert.IsFalse(expected == null && actual == null, "Do not assert Null is Null.");
        Assert.IsNotNull(actual, "Actual is null, expected is not.");
        Assert.IsNotNull(expected, "Expected is null, actual is not.");
        AssertHeadersEquals(expected.Headers, actual.Headers);
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
        Assert.AreEqual(expected.Topic, actual.Topic);
    }

    protected void AssertPublishedEquals(SubscribedData expected, SubscribedData actual)
    {
        Assert.IsFalse(expected == null && actual == null, "Do not assert Null is Null.");
        Assert.IsNotNull(actual, "Actual is null, expected is not.");
        Assert.IsNotNull(expected, "Expected is null, actual is not.");
        AssertHeadersEquals(expected.Headers, actual.Headers);
        AssertSqlDbDateTime(expected.NotBefore, actual.NotBefore);
        // Do not assert the actual.Id as it is database generated.
        Assert.AreEqual(expected.Body, actual.Body);
        AssertSqlDbDateTime(expected.Completed, actual.Completed);
        AssertSqlDbDateTime(expected.Enqueued, actual.Enqueued);
        AssertSqlDbDateTime(expected.Failed, actual.Failed);
        Assert.AreEqual(expected.Retries, actual.Retries);
        AssertSqlDbDateTime(expected.ValidUntil, actual.ValidUntil);
        Assert.AreEqual(expected.Topic, actual.Topic);
    }

    protected void AssertHeadersEquals(Headers? expected, Headers? actual)
    {
        Assert.IsFalse(expected == null && actual == null, "Do not assert Null is Null.");
        Assert.IsNotNull(actual, "Actual is null, expected is not.");
        Assert.IsNotNull(expected, "Expected is null, actual is not.");
        Assert.AreEqual(expected.MessageClass, actual.MessageClass);
        Assert.AreEqual(expected.ExceptionDetails, actual.ExceptionDetails);
        CollectionAssert.AreEqual(expected.UserHeaders, actual.UserHeaders);
        Assert.AreEqual(expected.Diagnostics, actual.Diagnostics);
    }

    /// <summary>
    /// Tests that two nullable DateTime values are equal.
    /// </summary>
    /// <param name="expected"></param>
    /// <param name="actual"></param>
    /// <param name="allowDriftMs">Allows a minor difference in times.</param>
    protected void AssertSqlDbDateTime(DateTime? expected, DateTime? actual, int allowDriftMs = 100)
    {
        // if they are both null, its ok.
        if (!expected.HasValue && !actual.HasValue) return;

        // if one is null and the other is not its a failure.
        Assert.AreEqual(expected.HasValue, actual.HasValue, $"Expected {expected}, Actual {actual}");

        // both are not null, so compare deeper.
        AssertSqlDbDateTime(expected!.Value, actual!.Value, allowDriftMs);
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
    /// Creates a new SagaData
    /// </summary>
    /// <returns></returns>
    protected SagaData CreateTestSagaData()
    {
        return new SagaData
        {
            Blocked = false,
            Data = new("Data"),
            SagaId = UniqueIdentity.New(),
            Key = new("Key"),
            MetaData = TestData.CreateSagaMetaData(),
        };
    }

    protected async Task InsertSubscribedMessage(SubscribedData message)
    {
        const string EnqueueMessageStatement =
            """
            INSERT INTO [{0}].[Subscribed_Pending] WITH (ROWLOCK)
            ([SubscriberId], [Topic], [ValidUntil], [MessageId], [Priority], [NotBefore], [Enqueued], [Completed], [Failed], [Retries], [Headers], [Body])
            OUTPUT INSERTED.[Id]
            VALUES
            (@SubscriberId, @Topic, @ValidUntil, @MessageId, @Priority, @NotBefore, SYSUTCDATETIME(), NULL, NULL, 0, @Headers, @Body)
            """;

        ArgumentNullException.ThrowIfNull(message);

        string statement = string.Format(EnqueueMessageStatement, DefaultSchema);

        var p = new DynamicParameters();
        p.Add("@MessageId", message.MessageId);
        p.Add("@Priority", message.Priority);
        p.Add("@SubscriberId", message.SubscriberId);
        p.Add("@ValidUntil", message.ValidUntil);
        p.Add("@NotBefore", message.NotBefore);
        p.Add("@Headers", message.Headers);
        p.Add("@Body", message.Body);
        p.Add("@Topic", message.Topic);

        message.Id = await SecondaryConnection.Connection.QueryFirstAsync<Identity>(statement, p);
    }

    protected List<SubscriptionsRow> GetSubscriptions() => GetTableContent(Subscriptions).ToSubscriptions();
    protected List<SubscribedData> GetSubscribedPending() => GetTableContent(SubscribedPending).ToSubscribed();
    protected List<SubscribedData> GetSubscribedFailed() => GetTableContent(SubscribedFailed).ToSubscribed();
    protected List<SubscribedData> GetSubscribedCompleted() => GetTableContent(SubscribedCompleted).ToSubscribed();
}
