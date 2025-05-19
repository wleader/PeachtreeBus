using Microsoft.Data.SqlClient;
using System;
using System.Data;

namespace PeachtreeBus.DatabaseSharing;

public class ExternallManagedSqlConnectionException(string message) : SharedDatabaseException(message);

public class ExternallyManagedSqlConnection(
    SqlConnection connection,
    SqlTransaction? transaction)
    : ISqlConnection
{
    public bool Disposed { get; private set; } = false;

    public SqlConnection Connection { get; } = connection;

    public ConnectionState State => Connection.State;

    private SqlTransaction? _transaction = transaction;

    public ISqlTransaction BeginTransaction()
    {
        if (_transaction is not null)
            throw new ExternallManagedSqlConnectionException(
                "A transaction can not be started when an externally managed transaction has been provided.");

        return new SqlTransactionProxy(Connection.BeginTransaction());
    }

    public void Open()
    {
        throw new ExternallManagedSqlConnectionException(
            "Attempt to Open an Externally Managed Connection. An Externally Managed Connection's connection state cannot be changed.");
    }

    public void Close()
    {
        throw new ExternallManagedSqlConnectionException(
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
