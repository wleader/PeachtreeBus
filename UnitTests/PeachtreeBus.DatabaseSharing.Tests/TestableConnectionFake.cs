using Microsoft.Data.SqlClient;
using PeachtreeBus.Testing;
using System;
using System.Data;
using System.Data.Common;
using System.Threading;
using System.Threading.Tasks;

namespace PeachtreeBus.DatabaseSharing.Tests;

public class TestableConnectionFake : ITestableConnection
{
    public bool Disposed { get; private set; }

    public TestableTransactionFake? LastTransaction { get; private set; }

    public DbConnection Connection { get; } = UninitializedObjects.Create<SqlConnection>();

    public ConnectionState State { get; set; }

    public ITestableTransaction BeginTransaction()
    {
        return LastTransaction = new TestableTransactionFake();
    }

    public Task<ITestableTransaction> BeginTransactionAsync(CancellationToken _ = default)
    {
        LastTransaction = new();
        return Task.FromResult<ITestableTransaction>(LastTransaction);
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
