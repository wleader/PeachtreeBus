using Microsoft.Data.SqlClient;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using System.Threading.Tasks;

namespace PeachtreeBus.DatabaseSharing;

public class ExternallyManagedSqlTransaction(SqlTransaction transaction) : ISqlTransaction
{
    public bool Disposed { get; private set; }
    public SqlTransaction Transaction { get; } = transaction;

    private const string CannotCommit =
        "The transaction cannot be committed because it is an externally managed transaction.";

    private const string CannotRollback =
        "The transaction cannot be rolled back because it is an externally managed transaction.";

    public void Commit() =>
        throw new ExternallyManagedSqlConnectionException(CannotCommit);

    public Task CommitAsync(CancellationToken _ = default) =>
        throw new ExternallyManagedSqlConnectionException(CannotCommit);

    public void Rollback() => 
        throw new ExternallyManagedSqlConnectionException(CannotRollback);
    
    public Task RollbackAsync(CancellationToken _ = default) =>
        throw new ExternallyManagedSqlConnectionException(CannotRollback);

    [ExcludeFromCodeCoverage(Justification = "Testing requires a live connection.")]
    public void Rollback(string transactionName) => 
        Transaction.Rollback(transactionName);
    
    [ExcludeFromCodeCoverage(Justification = "Testing requires a live connection.")]
    public Task RollbackAsync(string transactionName, CancellationToken cancellationToken = default) =>
        Transaction.RollbackAsync(transactionName, cancellationToken);

    [ExcludeFromCodeCoverage(Justification = "Testing requires a live connection.")]
    public void Save(string savePointName) => 
        Transaction.Save(savePointName);
    
    [ExcludeFromCodeCoverage(Justification = "Testing requires a live connection.")]
    public Task SaveAsync(string savePointName, CancellationToken cancellationToken = default) => 
        Transaction.SaveAsync(savePointName, cancellationToken);

    public void Dispose()
    {
        // do not dispose the external transaction.
        // this object does not own it.
        GC.SuppressFinalize(this);
        Disposed = true;
    }

    public ValueTask DisposeAsync()
    {
        // do not dispose the external transaction.
        // this object does not own it.
        GC.SuppressFinalize(this);
        Disposed = true;
        return ValueTask.CompletedTask;
    }
}