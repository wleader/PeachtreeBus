using Microsoft.Data.SqlClient;
using PeachtreeBus.Testing;
using System;
using System.Data;
using System.Threading;
using System.Threading.Tasks;

namespace PeachtreeBus.DatabaseSharing.Tests;

public class SqlConnectionFake : ISqlConnection
{
    public bool Disposed { get; private set; }

    public SqlTransactionFake? LastTransaction { get; private set; }

    public SqlConnection Connection { get; } = SqlServerTesting.CreateConnection();

    public ConnectionState State { get; set; }

    public ISqlTransaction BeginTransaction()
    {
        return LastTransaction = new SqlTransactionFake();
    }

    public Task<ISqlTransaction> BeginTransactionAsync(CancellationToken _ = default)
    {
        LastTransaction = new();
        return Task.FromResult<ISqlTransaction>(LastTransaction);
    }

    public void Close() => State = ConnectionState.Closed;

    public Task CloseAsync()
    {
        State = ConnectionState.Closed;
        return Task.CompletedTask;
    }

    public void Dispose()
    {
        Disposed = true;
        GC.SuppressFinalize(this);
    }

    public ValueTask DisposeAsync()
    {
        Disposed = true;
        GC.SuppressFinalize(this);
        return ValueTask.CompletedTask;
    }

    public void Open() => State = ConnectionState.Open;
    
    public Task OpenAsync(CancellationToken _ = default)
    {
        State = ConnectionState.Open;
        return Task.CompletedTask;
    }
}
