using System;
using System.Data;
using Npgsql;
using PeachtreeBus.Data;
using PeachtreeBus.DataAccessTests;
using PeachtreeBus.DatabaseSharing.PostgreSql;

namespace PeachtreeBus.PostgreSql.Tests;

public class PostgreSqlRowLock : IDisposable
{
    private readonly INpgSqlConnection _connection;
    private readonly INpgSqlTransaction _transaction;

    public DataSet DataSet { get; }
    private TestConfig TestConfig { get; } = new();

    public PostgreSqlRowLock(
        INpgSqlConnectionFactory connectionFactory,
        TableName tableName,
        int count = int.MaxValue,
        SchemaName? schema = null)
    {
        _connection = connectionFactory.GetConnection();
        _connection.Open();
        _transaction = _connection.BeginTransaction();

        var schemaName = schema ?? TestConfig.DefaultSchema;

        string statement =
            $"""
             SELECT *
             FROM {schemaName}.{tableName}
             LIMIT @Count
             FOR UPDATE SKIP LOCKED;
             """;

        using var cmd = new NpgsqlCommand(statement, _connection.Connection, _transaction.Transaction);
        cmd.Parameters.AddWithValue("@Count", count);

        var dataSet = new DataSet();
        using var adapter = new NpgsqlDataAdapter(cmd);
        adapter.Fill(dataSet);

        DataSet = dataSet;
    }

    public void Dispose()
    {
        _transaction.Rollback();
        _transaction.Dispose();
        _connection.Dispose();
        GC.SuppressFinalize(this);
    }
}