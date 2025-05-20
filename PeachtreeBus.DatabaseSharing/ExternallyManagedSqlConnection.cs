using Microsoft.Data.SqlClient;
using System;
using System.Data;
using System.Diagnostics.CodeAnalysis;

namespace PeachtreeBus.DatabaseSharing;

public class ExternallyManagedSqlConnectionException(string message) : SharedDatabaseException(message);

public class ExternallyManagedSqlConnection(
    SqlConnection connection)
    : ISqlConnection
{
    public bool Disposed { get; private set; } = false;

    public SqlConnection Connection { get; } = connection;

    [ExcludeFromCodeCoverage(Justification = "Requires an initialized connection object.")]
    public ConnectionState State => Connection.State;

    public ISqlTransaction BeginTransaction()
    {
        throw new ExternallyManagedSqlConnectionException(
            "A transaction can not be started when an externally managed transaction has been provided.");
    }

    public void Open()
    {
        throw new ExternallyManagedSqlConnectionException(
            "Attempt to Open an Externally Managed Connection. An Externally Managed Connection's connection state cannot be changed.");
    }

    public void Close()
    {
        throw new ExternallyManagedSqlConnectionException(
            "Attempt to Close an Externally Managed Connection. An Externally Managed Connection's connection state cannot be changed.");
    }

    public void Dispose()
    {
        // do not dispose the connection or transaction.
        // This object does not own it.
        Disposed = true;
        GC.SuppressFinalize(this);
    }
}
