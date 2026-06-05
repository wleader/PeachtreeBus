using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using Dapper;
using Npgsql;
using PeachtreeBus.Data;
using PeachtreeBus.DataAccessTests;
using PeachtreeBus.DatabaseSharing.PostgreSql;

namespace PeachtreeBus.PostgreSql.Tests;

public class PostgreSqlTestDataAccess(INpgSqlConnectionFactory connectionFactory) : ITestDataAccess
{
    private TestConfig TestConfig { get; } = new();
    protected INpgSqlConnection Connection = null!;

    public void Initialize()
    {
        Connection = connectionFactory.GetConnection();
        Connection.Open();
    }

    public void CleanEverything()
    {
        using var transaction = Connection.BeginTransaction();
        string statement =
            $"TRUNCATE TABLE {TestConfig.DefaultSchema}.{TestConfig.QueueCompleted}; " +
            $"TRUNCATE TABLE {TestConfig.DefaultSchema}.{TestConfig.QueueFailed}; " +
            $"TRUNCATE TABLE {TestConfig.DefaultSchema}.{TestConfig.QueuePending}; " +
            $"TRUNCATE TABLE {TestConfig.DefaultSchema}.{TestConfig.SagaData}; " +
            $"TRUNCATE TABLE {TestConfig.DefaultSchema}.{TestConfig.Subscriptions}; " +
            $"TRUNCATE TABLE {TestConfig.DefaultSchema}.{TestConfig.SubscribedPending}; " +
            $"TRUNCATE TABLE {TestConfig.DefaultSchema}.{TestConfig.SubscribedFailed}; " +
            $"TRUNCATE TABLE {TestConfig.DefaultSchema}.{TestConfig.SubscribedCompleted}; ";
        using var cmd = new NpgsqlCommand(statement, Connection.Connection, transaction.Transaction);
        cmd.ExecuteNonQuery();
        transaction.Commit();
    }

    public void CloseConnections()
    {
        Connection.Close();
    }

    public long CountRowsInTable(TableName tableName)
    {
        string statement = $"SELECT COUNT(*) FROM {TestConfig.DefaultSchema}.{tableName}";
        using var cmd = new NpgsqlCommand(statement, Connection.Connection, null);
        return (long)(cmd.ExecuteScalar() ?? throw new ApplicationException("Scalar not returned."));
    }

    public DataSet GetTableContent(TableName tableName)
    {
        var result = new DataSet();
        string statement = $"SELECT * FROM {TestConfig.DefaultSchema}.{tableName}";
        using var cmd = new NpgsqlCommand(statement, Connection.Connection, null);
        using var adapter = new NpgsqlDataAdapter(cmd);
        adapter.Fill(result);
        return result;
    }

    public List<T> GetTableContent<T>(TableName tableName) where T : class
    {
        string statement = $"SELECT * FROM {TestConfig.DefaultSchema}.{tableName}";
        return Connection.Connection.Query<T>(statement).ToList();
    }
}