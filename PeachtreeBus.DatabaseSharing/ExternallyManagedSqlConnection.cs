using Microsoft.Data.SqlClient;
using System;
using System.Data;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using System.Threading.Tasks;

namespace PeachtreeBus.DatabaseSharing;

public class ExternallyManagedSqlConnectionException(string message) : SharedDatabaseException(message);

public class ExternallyManagedSqlConnection(
    SqlConnection connection)
    : ISqlConnection
{
    public bool Disposed { get; private set; }

    public SqlConnection Connection { get; } = connection;

    [ExcludeFromCodeCoverage(Justification = "Requires an initialized connection object.")]
    public ConnectionState State => Connection.State;

    private const string CannotBeginTransation =
        "A transaction can not be started when an externally managed transaction has been provided.";
    private const string CannotOpen =
        "Attempt to Open an Externally Managed Connection. An Externally Managed Connection's connection state cannot be changed.";
    private const string CannotClose =
        "Attempt to Close an Externally Managed Connection. An Externally Managed Connection's connection state cannot be changed.";

    public ISqlTransaction BeginTransaction() =>
        throw new ExternallyManagedSqlConnectionException(CannotBeginTransation);
    public Task<ISqlTransaction> BeginTransactionAsync(CancellationToken _ = default) =>
        throw new ExternallyManagedSqlConnectionException(CannotBeginTransation);
    public void Open() =>
        throw new ExternallyManagedSqlConnectionException(CannotOpen);
    public Task OpenAsync(CancellationToken _ = default) =>
        throw new ExternallyManagedSqlConnectionException(CannotOpen);
    public void Close() => 
        throw new ExternallyManagedSqlConnectionException(CannotClose);
    public Task CloseAsync()=> 
        throw new ExternallyManagedSqlConnectionException(CannotClose);

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