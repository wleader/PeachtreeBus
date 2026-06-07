using System;
using System.Collections.Generic;
using System.Linq;
using Dapper;
using PeachtreeBus.Data;
using PeachtreeBus.DataAccessTests;
using PeachtreeBus.DatabaseSharing;

namespace PeachtreeBus.MsSql.Tests;

public class MsSqlRowLock<T> : ILockedRows<T>
{
    private readonly ISqlConnection _connection;
    private readonly ISqlTransaction _transaction;
    private TestConfig TestConfig { get; } = new();

    public List<T> Data { get; }
    
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