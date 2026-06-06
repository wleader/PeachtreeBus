using System;
using System.Data;
using Microsoft.Data.SqlClient;
using PeachtreeBus.Data;
using PeachtreeBus.DataAccessTests;
using PeachtreeBus.DatabaseSharing;

namespace PeachtreeBus.MsSql.Tests;

public class MsSqlRowLock : IDisposable
{
    private readonly ISqlConnection _connection;
    private readonly ISqlTransaction _transaction;

    public DataSet DataSet { get; }
    private TestConfig TestConfig { get; } = new();

    public MsSqlRowLock(
        ISqlConnectionFactory connectionFactory,
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
             SELECT TOP (@Count) *
             FROM [{schemaName}].[{tableName}]
             WITH (UPDLOCK, READPAST, ROWLOCK)
             """;

        using var cmd = new SqlCommand(statement, _connection.Connection, _transaction.Transaction);
        cmd.Parameters.AddWithValue("@Count", count);

        var dataSet = new DataSet();
        using var adapter = new SqlDataAdapter(cmd);
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