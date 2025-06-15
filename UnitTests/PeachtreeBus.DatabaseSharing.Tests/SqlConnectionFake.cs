using Microsoft.Data.SqlClient;
using System;
using System.Data;
using System.Runtime.CompilerServices;

namespace PeachtreeBus.DatabaseSharing.Tests;

public class SqlConnectionFake : ISqlConnection
{
    public SqlConnectionFake()
    {
        Connection = (SqlConnection)RuntimeHelpers.GetUninitializedObject(typeof(SqlConnection));
    }

    public bool Disposed { get; set; } = false;

    public SqlTransactionFake? LastTransaction { get; private set; }

    public SqlConnection Connection { get; }

    public ConnectionState State { get; set; }

    public ISqlTransaction BeginTransaction()
    {
        return LastTransaction = new SqlTransactionFake();
    }

    public void Close()
    {
        State = ConnectionState.Closed;
    }

    public void Dispose()
    {
        Disposed = true;
        GC.SuppressFinalize(this);
    }

    public void Open()
    {
        State = ConnectionState.Open;
    }
}
