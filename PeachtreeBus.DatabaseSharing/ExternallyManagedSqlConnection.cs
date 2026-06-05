using System;
using System.Data;
using System.Data.Common;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Data.SqlClient;

namespace PeachtreeBus.DatabaseSharing;

public class ExternallyManagedConnectionException(string message) : SharedDatabaseException(message);

public abstract class ExternallyManagedConnection<TConnection, TTransaction, TTransactionInterface>(
    TConnection connection)
    : IBaseConnection<TConnection, TTransaction, TTransactionInterface>
    where TConnection : DbConnection
    where TTransaction : DbTransaction
    where TTransactionInterface : IBaseTransaction<TTransaction>
{
    public bool Disposed { get; private set; }

    public TConnection Connection { get; } = connection;

    [ExcludeFromCodeCoverage(Justification = "Requires an initialized connection object.")]
    public ConnectionState State => Connection.State;

    private const string CannotBeginTransaction =
        "A transaction can not be started when an externally managed transaction has been provided.";
    private const string CannotOpen =
        "Attempt to Open an Externally Managed Connection. An Externally Managed Connection's connection state cannot be changed.";
    private const string CannotClose =
        "Attempt to Close an Externally Managed Connection. An Externally Managed Connection's connection state cannot be changed.";

    public TTransactionInterface BeginTransaction() =>
        throw new ExternallyManagedConnectionException(CannotBeginTransaction);
    public Task<TTransactionInterface> BeginTransactionAsync(CancellationToken _ = default) =>
        throw new ExternallyManagedConnectionException(CannotBeginTransaction);
    public void Open() =>
        throw new ExternallyManagedConnectionException(CannotOpen);
    public Task OpenAsync(CancellationToken _ = default) =>
        throw new ExternallyManagedConnectionException(CannotOpen);
    public void Close() => 
        throw new ExternallyManagedConnectionException(CannotClose);
    public Task CloseAsync()=> 
        throw new ExternallyManagedConnectionException(CannotClose);

    public void Dispose()
    {
        // do not dispose the connection or transaction.
        // This object does not own it.
        Disposed = true;
        GC.SuppressFinalize(this);
    }

    public ValueTask DisposeAsync()
    {
        Disposed = true;
        GC.SuppressFinalize(this);
        return ValueTask.CompletedTask;
    }
}

public class ExternallyManagedSqlConnection(SqlConnection connection) 
    : ExternallyManagedConnection<SqlConnection, SqlTransaction, ISqlTransaction>(connection)
    ,ISqlConnection;