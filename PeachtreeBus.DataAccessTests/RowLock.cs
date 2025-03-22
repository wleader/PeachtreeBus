using Microsoft.Data.SqlClient;
using PeachtreeBus.Data;
using PeachtreeBus.DatabaseSharing;
using System;
using System.Data;

namespace PeachtreeBus.DataAccessTests;

/// <summary>
/// A convenience class that locks rows in a table.
/// IDisposable, so use with a using statement to have it
/// unlock when the using block ends.
/// </summary>
public class RowLock : IDisposable
{
    private readonly SqlConnectionProxy _connection;
    private readonly ISqlTransaction _transaction;

    public DataSet DataSet { get; }

    public RowLock(SchemaName schemaName, TableName tableName, int count = int.MaxValue)
    {
        _connection = new SqlConnectionProxy(AssemblyInitialize.DbConnectionString);
        _connection.Open();
        _transaction = _connection.BeginTransaction();

        string statement =
            $"""
            SELECT TOP (@Count) *
            FROM [{schemaName}].[{tableName}]
            WITH (UPDLOCK, READPAST, ROWLOCK)
            """;

        using var cmd = new SqlCommand(statement, _connection.Connection, _transaction.Transaction);
        cmd.Parameters.AddWithValue("@Count", count);

        var dataSet = new DataSet();
        using var adpater = new SqlDataAdapter(cmd);
        adpater.Fill(dataSet);

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
