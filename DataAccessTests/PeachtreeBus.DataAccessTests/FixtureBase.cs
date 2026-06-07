using Dapper;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using PeachtreeBus.Core.Tests;
using PeachtreeBus.Core.Tests.Fakes;
using PeachtreeBus.Data;
using PeachtreeBus.DatabaseSharing;
using PeachtreeBus.Sagas;
using PeachtreeBus.Serialization;
using PeachtreeBus.Subscriptions;
using System;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;
using PeachtreeBus.DatabaseTesting;
using PeachtreeBus.DatabaseTesting.MsSql;

namespace PeachtreeBus.DataAccessTests;

/// <summary>
/// A base class that contains code useful in multiple tests.
/// </summary>
public abstract class FixtureBase<TAccess>
{
    protected TestConfig TestConfig { get; } = new();

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

    protected FakeBreakerProvider FakeBreakerProvider = new();

    /// <summary>
    /// Performs tasks that happen before each test.
    /// </summary>
    public virtual void Initialize()
    {
        var factory = TestServices.GetService<ISqlConnectionFactory>();
        
        // Create connections.
        PrimaryConnection = factory.GetConnection();

        SecondaryConnection = factory.GetConnection();
        SecondaryConnection.Open();

        // create the data access object.
        _connectionFactory.Setup(f => f.GetConnection()).Returns(() =>
        {
            if (PrimaryConnection.Disposed)
                PrimaryConnection = factory.GetConnection();
            return PrimaryConnection;
        });
        SharedDB = new SharedDatabase(_connectionFactory.Object);
        DapperMethods = new(new DapperTypesHandler(new DefaultSerializer()), SharedDB);

        Configuration = new();
        Configuration.SetupGet(s => s.Schema).Returns(TestConfig.DefaultSchema);

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
    public virtual void Cleanup()
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
            $"TRUNCATE TABLE [{TestConfig.DefaultSchema}].[{TestConfig.QueueCompleted}]; " +
            $"TRUNCATE TABLE [{TestConfig.DefaultSchema}].[{TestConfig.QueueFailed}]; " +
            $"TRUNCATE TABLE [{TestConfig.DefaultSchema}].[{TestConfig.QueuePending}]; " +
            $"TRUNCATE TABLE [{TestConfig.DefaultSchema}].[{TestConfig.SagaData}]; " +
            $"TRUNCATE TABLE [{TestConfig.DefaultSchema}].[{TestConfig.Subscriptions}]; " +
            $"TRUNCATE TABLE [{TestConfig.DefaultSchema}].[{TestConfig.SubscribedPending}]; " +
            $"TRUNCATE TABLE [{TestConfig.DefaultSchema}].[{TestConfig.SubscribedFailed}]; " +
            $"TRUNCATE TABLE [{TestConfig.DefaultSchema}].[{TestConfig.SubscribedCompleted}]; ";
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
        string statment = $"SELECT COUNT(*) FROM [{TestConfig.DefaultSchema}].[{table}]";
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
        string statement = $"SELECT * FROM [{TestConfig.DefaultSchema}].[{tablename}]";
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
    /// Tests that two SubscribedMessage are equal.
    /// </summary>
    /// <param name="expected"></param>
    /// <param name="actual"></param>
    protected void AssertSubscribedEquals(SubscribedData expected, SubscribedData actual)
    {
        Assert.IsFalse(expected == null && actual == null, "Do not assert Null is Null.");
        Assert.IsNotNull(actual, "Actual is null, expected is not.");
        Assert.IsNotNull(expected, "Expected is null, actual is not.");
        DataAssert.AreEqual(expected.Headers, actual.Headers);
        Assert.AreEqual(expected.MessageId, actual.MessageId);
        DataAssert.AreEqual(expected.NotBefore, actual.NotBefore);
        Assert.AreEqual(expected.Id, actual.Id);
        Assert.AreEqual(expected.Body, actual.Body);
        DataAssert.AreEqual(expected.Completed, actual.Completed);
        DataAssert.AreEqual(expected.Enqueued, actual.Enqueued);
        DataAssert.AreEqual(expected.Failed, actual.Failed);
        Assert.AreEqual(expected.Retries, actual.Retries);
        Assert.AreEqual(expected.SubscriberId, actual.SubscriberId);
        DataAssert.AreEqual(expected.ValidUntil, actual.ValidUntil);
        Assert.AreEqual(expected.Topic, actual.Topic);
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

        string statement = string.Format(EnqueueMessageStatement, TestConfig.DefaultSchema);

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

    protected List<SubscriptionsRow> GetSubscriptions() => GetTableContent(TestConfig.Subscriptions).ToSubscriptions();
    protected List<SubscribedData> GetSubscribedPending() => GetTableContent(TestConfig.SubscribedPending).ToSubscribed();
    protected List<SubscribedData> GetSubscribedFailed() => GetTableContent(TestConfig.SubscribedFailed).ToSubscribed();
    protected List<SubscribedData> GetSubscribedCompleted() => GetTableContent(TestConfig.SubscribedCompleted).ToSubscribed();
}
