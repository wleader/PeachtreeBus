using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
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

    public Task Initialize()
    {
        _connection = connectionFactory.GetConnection();
        return _connection.OpenAsync();
    }

    public async Task CleanEverything()
    {
        await using var transaction = await _connection.BeginTransactionAsync();
        string statement =
        $"""
             DELETE FROM {TestConfig.DefaultSchema}.{TestConfig.QueueCompleted};
             DELETE FROM {TestConfig.DefaultSchema}.{TestConfig.QueueFailed};
             DELETE FROM {TestConfig.DefaultSchema}.{TestConfig.QueuePending};
             DELETE FROM {TestConfig.DefaultSchema}.{TestConfig.SagaData};
             DELETE FROM {TestConfig.DefaultSchema}.{TestConfig.Subscriptions};
             DELETE FROM {TestConfig.DefaultSchema}.{TestConfig.SubscribedPending};
             DELETE FROM {TestConfig.DefaultSchema}.{TestConfig.SubscribedFailed};
             DELETE FROM {TestConfig.DefaultSchema}.{TestConfig.SubscribedCompleted};
         """;
        await _connection.Connection.ExecuteAsync(statement, null, transaction.Transaction);
        await transaction.CommitAsync();
    }

    public Task CloseConnections() => _connection.CloseAsync();

    public async Task<long> CountRowsInTable(TableName tableName)
    {
        string statement = $"SELECT COUNT(*) FROM {TestConfig.DefaultSchema}.{tableName}";
        await using var cmd = new NpgsqlCommand(statement, _connection.Connection, null);
        return (long)(await cmd.ExecuteScalarAsync())!;
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

    public async Task<List<T>> GetTableContent<T>(TableName tableName) where T : class
    {
        string statement = $"SELECT * FROM {TestConfig.DefaultSchema}.{tableName}";
        return (await _connection.Connection.QueryAsync<T>(statement)).ToList();
    }

    public Task InsertQueueCompleted(QueueData data)
    {
        var statement =
            """
            INSERT INTO {0}.{1}_completed
            (id,message_id,priority,not_before,enqueued,completed,failed,retries,headers,body)
            VALUES
            (@Id, @MessageId, @Priority, @NotBefore, @Enqueued, @Completed, @Failed, @Retries, @Headers, @Body)
            """;
        statement = string.Format(statement, TestConfig.DefaultSchema, TestConfig.DefaultQueue);
        return _connection.Connection.ExecuteAsync(statement, data);
    }
    
    public Task InsertQueueFailed(QueueData data)
    {
        var statement =
            """
            INSERT INTO {0}.{1}_failed
            (id,message_id,priority,not_before,enqueued,completed,failed,retries,headers,body)
            VALUES
            (@Id, @MessageId, @Priority, @NotBefore, @Enqueued, @Completed, @Failed, @Retries, @Headers, @Body)
            """;
        statement = string.Format(statement, TestConfig.DefaultSchema, TestConfig.DefaultQueue);
        return _connection.Connection.ExecuteAsync(statement, data);
    }

    public async Task InsertSubscribedPending(SubscribedData data)
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
        data.Id = await _connection.Connection.QueryFirstAsync<Identity>(statement, data);
    }
    
    public Task InsertSubscribedCompleted(SubscribedData data)
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
        return _connection.Connection.ExecuteAsync(statement, data);
    }
    
    public Task InsertSubscribedFailed(SubscribedData data)
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
        return _connection.Connection.ExecuteAsync(statement, data);
    }

    public ILockedRows<T> LockRows<T>(TableName tableName, int count) => 
        new PostgreSqlRowLock<T>(connectionFactory, tableName, count);
}