using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using Dapper;
using Npgsql;
using PeachtreeBus.Data;
using PeachtreeBus.DataAccessTests;
using PeachtreeBus.DatabaseSharing.PostgreSql;
using PeachtreeBus.Queues;
using PeachtreeBus.Subscriptions;

namespace PeachtreeBus.PostgreSql.Tests;

public class PostgreSqlTestDataAccess(
    INpgSqlConnectionFactory connectionFactory,
    ITestConfig testConfig) : ITestDataAccess
{
    public ITestConfig TestConfig { get; } = testConfig;
    private INpgSqlConnection _connection = null!;

    public void Initialize()
    {
        _connection = connectionFactory.GetConnection();
        _connection.Open();
    }

    public void CleanEverything()
    {
        using var transaction = _connection.BeginTransaction();
        string statement =
            $"TRUNCATE TABLE {TestConfig.DefaultSchema}.{TestConfig.QueueCompleted}; " +
            $"TRUNCATE TABLE {TestConfig.DefaultSchema}.{TestConfig.QueueFailed}; " +
            $"TRUNCATE TABLE {TestConfig.DefaultSchema}.{TestConfig.QueuePending}; " +
            $"TRUNCATE TABLE {TestConfig.DefaultSchema}.{TestConfig.SagaData}; " +
            $"TRUNCATE TABLE {TestConfig.DefaultSchema}.{TestConfig.Subscriptions}; " +
            $"TRUNCATE TABLE {TestConfig.DefaultSchema}.{TestConfig.SubscribedPending}; " +
            $"TRUNCATE TABLE {TestConfig.DefaultSchema}.{TestConfig.SubscribedFailed}; " +
            $"TRUNCATE TABLE {TestConfig.DefaultSchema}.{TestConfig.SubscribedCompleted}; ";
        using var cmd = new NpgsqlCommand(statement, _connection.Connection, transaction.Transaction);
        cmd.ExecuteNonQuery();
        transaction.Commit();
    }

    public void CloseConnections()
    {
        _connection.Close();
    }

    public long CountRowsInTable(TableName tableName)
    {
        string statement = $"SELECT COUNT(*) FROM {TestConfig.DefaultSchema}.{tableName}";
        using var cmd = new NpgsqlCommand(statement, _connection.Connection, null);
        return (long)(cmd.ExecuteScalar() ?? throw new ApplicationException("Scalar not returned."));
    }

    public DataSet GetTableContent(TableName tableName)
    {
        var result = new DataSet();
        string statement = $"SELECT * FROM {TestConfig.DefaultSchema}.{tableName}";
        using var cmd = new NpgsqlCommand(statement, _connection.Connection, null);
        using var adapter = new NpgsqlDataAdapter(cmd);
        adapter.Fill(result);
        return result;
    }

    public List<T> GetTableContent<T>(TableName tableName) where T : class
    {
        string statement = $"SELECT * FROM {TestConfig.DefaultSchema}.{tableName}";
        return _connection.Connection.Query<T>(statement).ToList();
    }

    public void InsertQueueCompleted(QueueData data)
    {
        var statement =
            """
            INSERT INTO {0}.{1}_completed
            (id,message_id,priority,not_before,enqueued,completed,failed,retries,headers,body)
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
            INSERT INTO {0}.{1}_failed
            (id,message_id,priority,not_before,enqueued,completed,failed,retries,headers,body)
            VALUES
            (@Id, @MessageId, @Priority, @NotBefore, @Enqueued, @Completed, @Failed, @Retries, @Headers, @Body)
            """;
        statement = string.Format(statement, TestConfig.DefaultSchema, TestConfig.DefaultQueue);
        _connection.Connection.Execute(statement, data);
    }

    public void InsertSubscribedPending(SubscribedData data)
    {
        const string enqueueMessageStatement =
            """
            INSERT INTO {0}.subscribed_pending
            (subscriber_id, topic, valid_until, message_id, priority, not_before, enqueued, completed, failed, retries, headers, body)
            VALUES
            (@SubscriberId, @Topic, @ValidUntil, @MessageId, @Priority, @NotBefore, NOW(), NULL, NULL, 0, @Headers, @Body)
            RETURNING id;
            """;
        ArgumentNullException.ThrowIfNull(data);
        string statement = string.Format(enqueueMessageStatement, TestConfig.DefaultSchema);
        data.Id = _connection.Connection.QueryFirst<Identity>(statement, data);
    }
    
    public void InsertSubscribedCompleted(SubscribedData data)
    {
        const string enqueueMessageStatement =
            """
            INSERT INTO {0}.subscribed_completed
            (id, subscriber_id, topic, valid_until, message_id, priority, not_before, enqueued, completed, 
             failed, retries, headers, body)
            VALUES
            (@id, @SubscriberId, @Topic, @ValidUntil, @MessageId, @Priority, @NotBefore, @Enqueued, @Completed,
             @Failed, @Retries, @Headers, @Body);
            """;
        ArgumentNullException.ThrowIfNull(data);
        string statement = string.Format(enqueueMessageStatement, TestConfig.DefaultSchema);
        _connection.Connection.Execute(statement, data);
    }
    
    public void InsertSubscribedFailed(SubscribedData data)
    {
        const string enqueueMessageStatement =
            """
            INSERT INTO {0}.subscribed_failed
            (id, subscriber_id, topic, valid_until, message_id, priority, not_before, enqueued, completed, 
             failed, retries, headers, body)
            VALUES
            (@id, @SubscriberId, @Topic, @ValidUntil, @MessageId, @Priority, @NotBefore, @Enqueued, @Completed,
             @Failed, @Retries, @Headers, @Body);
            """;
        ArgumentNullException.ThrowIfNull(data);
        string statement = string.Format(enqueueMessageStatement, TestConfig.DefaultSchema);
        _connection.Connection.Execute(statement, data);
    }

    public IDisposable LockRows(TableName tableName, int count) => 
        new PostgreSqlRowLock(connectionFactory, tableName, count);
}