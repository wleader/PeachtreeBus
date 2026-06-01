using System;
using System.Data.Common;
using System.Threading;
using System.Threading.Tasks;

namespace PeachtreeBus.DatabaseSharing;

public interface IBaseConnection<out TConnection, TTransaction, TTransactionInterface>
    : IDisposable, IAsyncDisposable
    where TConnection : DbConnection, IDisposable, IAsyncDisposable
    where TTransaction : DbTransaction, IDisposable, IAsyncDisposable
    where TTransactionInterface : IBaseTransaction<TTransaction>, IDisposable, IAsyncDisposable
{
    bool Disposed { get; }
    TConnection Connection { get; }
    System.Data.ConnectionState State { get; }
    void Open();
    Task OpenAsync(CancellationToken cancellationToken = default);
    TTransactionInterface BeginTransaction();
    Task<TTransactionInterface> BeginTransactionAsync(CancellationToken cancellationToken = default);
    void Close();
    Task CloseAsync();
}

public abstract class BaseConnection<TConnection, TTransaction, TTransactionInterface>(TConnection connection)
    : BaseDisposable<TConnection>(connection)
    , IBaseConnection<TConnection, TTransaction, TTransactionInterface>
    where TConnection : DbConnection, IDisposable, IAsyncDisposable
    where TTransaction : DbTransaction, IDisposable, IAsyncDisposable
    where TTransactionInterface : IBaseTransaction<TTransaction>
{
    public TConnection Connection { get; } = connection;
    public System.Data.ConnectionState State => Connection.State; 

    public void Open() => 
        Connection.Open();
    public Task OpenAsync(CancellationToken cancellationToken = default) =>
        Connection.OpenAsync(cancellationToken);

    public void Close() =>
        Connection.Close();
    public Task CloseAsync() =>
        Connection.CloseAsync();

    public abstract TTransactionInterface BeginTransaction();

    public abstract Task<TTransactionInterface> BeginTransactionAsync(CancellationToken cancellationToken = default);
}