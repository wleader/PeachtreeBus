using Microsoft.Data.SqlClient;
using System;
using System.Diagnostics.CodeAnalysis;

namespace PeachtreeBus.DatabaseSharing;

public class ExternallyManagedSqlTransaction(SqlTransaction transaction) : ISqlTransaction
{
    public bool Disposed { get; private set; } = false;
    public SqlTransaction Transaction { get; } = transaction;

    public void Commit()
    {
        throw new ExternallyManagedSqlConnectionException(
            "The transaction cannot be committed because it is an externally managed transaction.");
    }

    public void Rollback()
    {
        throw new ExternallyManagedSqlConnectionException(
            "The transaction cannot be rolled back because it is an externally managed transaction.");
    }

    [ExcludeFromCodeCoverage(Justification = "Testing requires a live connection.")]
    public void Rollback(string transactionName) => Transaction.Rollback(transactionName);

    [ExcludeFromCodeCoverage(Justification = "Testing requires a live connection.")]
    public void Save(string savePointName) => Transaction.Save(savePointName);

    public void Dispose()
    {
        // do not dispose the external transaction.
        // this object does not own it.
        GC.SuppressFinalize(this);
        Disposed = true;
    }
}
