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

namespace PeachtreeBus.PostgreSql.Tests;

public class PostgreSqlTestDataAccess(INpgSqlConnectionFactory connectionFactory) : ITestDataAccess
{
    private TestConfig TestConfig { get; } = new();
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
            INSERT INTO {0}.{1}_Completed
            (id,message_id,priority,not_before,enqueued,completed,failed,retries,headers,body)
            VALUES
            (@Id, @MessageId, @Priority, @NotBefore, @Enqueued, @Completed, @Failed, @Retries, @Headers, @Body)
            """;
        statement = string.Format(statement, TestConfig.DefaultSchema, TestConfig.DefaultQueue);
        _connection.Connection.Execute(statement, data);
    }
}