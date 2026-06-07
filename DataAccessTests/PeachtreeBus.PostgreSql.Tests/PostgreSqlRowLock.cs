using System;
using System.Collections.Generic;
using System.Linq;
using Dapper;
using PeachtreeBus.Data;
using PeachtreeBus.DataAccessTests;
using PeachtreeBus.DatabaseSharing.PostgreSql;

namespace PeachtreeBus.PostgreSql.Tests;

public class PostgreSqlRowLock<T> : ILockedRows<T>
{
    private readonly INpgSqlConnection _connection;
    private readonly INpgSqlTransaction _transaction;

    private TestConfig TestConfig { get; } = new();
    
    public List<T> Data { get; }

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

        var p = new DynamicParameters();
        p.Add("@Count", count);

        Data = _connection.Connection.Query<T>(statement, p, _transaction.Transaction).ToList();
    }

    public void Dispose()
    {
        _transaction.Rollback();
        _transaction.Dispose();
        _connection.Dispose();
        GC.SuppressFinalize(this);
    }
}