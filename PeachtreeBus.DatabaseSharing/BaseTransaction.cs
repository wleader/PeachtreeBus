using System;
using System.Data.Common;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using System.Threading.Tasks;

namespace PeachtreeBus.DatabaseSharing;

public interface IBaseTransaction<out TTransaction>
    : IDisposable, IAsyncDisposable
    where TTransaction : DbTransaction, IDisposable, IAsyncDisposable
{
    bool Disposed { get; }
    TTransaction Transaction { get; }
    void Commit();
    void Rollback();
    void Rollback(string transactionName);
    void Save(string savePointName);
    Task CommitAsync(CancellationToken cancellationToken = default);
    Task RollbackAsync(CancellationToken cancellationToken = default);
    Task RollbackAsync(string transactionName, CancellationToken cancellationToken = default);
    Task SaveAsync(string savePointName, CancellationToken cancellationToken = default);
}

[ExcludeFromCodeCoverage(Justification =
    "This object requires a connection to a real database to test it.")]
public abstract class BaseTransaction<TTransaction>(TTransaction transaction)
    : BaseDisposable<TTransaction>(transaction), IBaseTransaction<TTransaction>
    where TTransaction : DbTransaction, IDisposable, IAsyncDisposable
{
    public TTransaction Transaction { get; } = transaction;

    public void Commit() =>
        Transaction.Commit();

    public Task CommitAsync(CancellationToken cancellationToken = default) =>
        Transaction.CommitAsync(cancellationToken);

    public void Rollback() =>
        Transaction.Rollback();

    public Task RollbackAsync(CancellationToken cancellationToken = default) =>
        Transaction.RollbackAsync(cancellationToken);

    public void Rollback(string savePointName) =>
        Transaction.Rollback(savePointName);

    public Task RollbackAsync(string savePointName, CancellationToken cancellationToken = default) =>
        Transaction.RollbackAsync(savePointName, cancellationToken);

    public void Save(string savePointName) =>
        Transaction.Save(savePointName);

    public Task SaveAsync(string savePointName, CancellationToken cancellationToken = default) =>
        Transaction.SaveAsync(savePointName, cancellationToken);
}