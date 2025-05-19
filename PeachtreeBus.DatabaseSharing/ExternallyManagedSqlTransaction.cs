using Microsoft.Data.SqlClient;
using System;

namespace PeachtreeBus.DatabaseSharing;

public class ExternallyManagedSqlTransaction(SqlTransaction transaction) : ISqlTransaction
{
    public bool Disposed { get; private set; } = false;
    public SqlTransaction Transaction { get; } = transaction;

    public void Commit()
    {
        throw new ExternallManagedSqlConnectionException(
            "The transaction cannot be committed because it is an externally managed transaction.");
    }

    public void Rollback()
    {
        throw new ExternallManagedSqlConnectionException(
            "The transaction cannot be rolled back because it is an externally managed transaction.");
    }

    public void Rollback(string transactionName) => Transaction.Rollback(transactionName);
    public void Save(string savePointName) => Transaction.Save(savePointName);

    public void Dispose()
    {
        // do not dispose the external transaction.
        // this object does not own it.
        GC.SuppressFinalize(this);
        Disposed = true;
    }
}
