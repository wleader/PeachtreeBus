using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using Dapper;
using Microsoft.Data.SqlClient;
using PeachtreeBus.Data;
using PeachtreeBus.DataAccessTests;
using PeachtreeBus.DatabaseSharing;
using PeachtreeBus.Queues;
using PeachtreeBus.Subscriptions;

namespace PeachtreeBus.MsSql.Tests;

public class MsSqlTestDataAccess(
    ISqlConnectionFactory connectionFactory,
    ITestConfig testConfig)
    : ITestDataAccess
{
    public ITestConfig TestConfig { get; } = testConfig;
    private ISqlConnection _connection = null!;
    
    public void Initialize()
    {
        _connection = connectionFactory.GetConnection();
        _connection.Open();
    }

    public void CleanEverything()
    {
        using var transaction = _connection.BeginTransaction();
        string statement =
            $"TRUNCATE TABLE [{TestConfig.DefaultSchema}].[{TestConfig.QueueCompleted}]; " +
            $"TRUNCATE TABLE [{TestConfig.DefaultSchema}].[{TestConfig.QueueFailed}]; " +
            $"TRUNCATE TABLE [{TestConfig.DefaultSchema}].[{TestConfig.QueuePending}]; " +
            $"TRUNCATE TABLE [{TestConfig.DefaultSchema}].[{TestConfig.SagaData}]; " +
            $"TRUNCATE TABLE [{TestConfig.DefaultSchema}].[{TestConfig.Subscriptions}]; " +
            $"TRUNCATE TABLE [{TestConfig.DefaultSchema}].[{TestConfig.SubscribedPending}]; " +
            $"TRUNCATE TABLE [{TestConfig.DefaultSchema}].[{TestConfig.SubscribedFailed}]; " +
            $"TRUNCATE TABLE [{TestConfig.DefaultSchema}].[{TestConfig.SubscribedCompleted}]; ";
        using var cmd = new SqlCommand(statement, _connection.Connection, transaction.Transaction);
        cmd.ExecuteNonQuery();
        transaction.Commit();
    }

    public void CloseConnections()
    {
        _connection.Close();
    }

    public long CountRowsInTable(TableName tableName)
    {
        string statement = $"SELECT COUNT(*) FROM [{TestConfig.DefaultSchema}].[{tableName}]";
        using var cmd = new SqlCommand(statement, _connection.Connection, null);
        return (int)cmd.ExecuteScalar();
    }

    public DataSet GetTableContent(TableName tableName)
    {
        var result = new DataSet();
        string statement = $"SELECT * FROM [{TestConfig.DefaultSchema}].[{tableName}]";
        using var cmd = new SqlCommand(statement, _connection.Connection, null);
        using var adapter = new SqlDataAdapter(cmd);
        adapter.Fill(result);
        return result;
    }

    public List<T> GetTableContent<T>(TableName tableName) where T : class
    {
        string statement = $"SELECT * FROM [{TestConfig.DefaultSchema}].[{tableName}]";
        return _connection.Connection.Query<T>(statement).ToList();
    }

    public void InsertQueueCompleted(QueueData data)
    {
        var statement =
            """
            INSERT INTO [{0}].[{1}_Completed]
            ([Id],[MessageId],[Priority],[NotBefore],[Enqueued],[Completed],[Failed],[Retries],[Headers],[Body])
            VALUES
            (@Id, @MessageId, @Priority, @NotBefore, @Enqueued, @Completed, @Failed, @Retries, @Headers, @Body)
            """;
        statement = string.Format(statement, TestConfig.DefaultSchema, TestConfig.DefaultQueue);
        _connection.Connection.Execute(statement, data);
    }
    
    public void InsertQueueFailed(QueueData data)
    {
        var statement =
            """
            INSERT INTO [{0}].[{1}_Failed]
            ([Id],[MessageId],[Priority],[NotBefore],[Enqueued],[Completed],[Failed],[Retries],[Headers],[Body])
            VALUES
            (@Id, @MessageId, @Priority, @NotBefore, @Enqueued, @Completed, @Failed, @Retries, @Headers, @Body)
            """;
        statement = string.Format(statement, TestConfig.DefaultSchema, TestConfig.DefaultQueue);
        _connection.Connection.Execute(statement, data);
    }

    public void InsertSubscribedMessage(SubscribedData data)
    {
        const string enqueueMessageStatement =
            """
            INSERT INTO [{0}].[Subscribed_Pending] WITH (ROWLOCK)
            ([SubscriberId], [Topic], [ValidUntil], [MessageId], [Priority], [NotBefore], [Enqueued], [Completed], [Failed], [Retries], [Headers], [Body])
            OUTPUT INSERTED.[Id]
            VALUES
            (@SubscriberId, @Topic, @ValidUntil, @MessageId, @Priority, @NotBefore, SYSUTCDATETIME(), NULL, NULL, 0, @Headers, @Body)
            """;
        ArgumentNullException.ThrowIfNull(data);
        string statement = string.Format(enqueueMessageStatement, TestConfig.DefaultSchema);
        data.Id = _connection.Connection.QueryFirst<Identity>(statement, data);
    }
}