using PeachtreeBus.DatabaseSharing;
using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;

namespace PeachtreeBus.EntityFrameworkCore;

/// <summary>
/// A DB Context that automatically shares the transaction and connection with other
/// instances of itself within the same Dependency Injection Scope. Includes
/// some helper functions for Scalar and Non-Query statements.
/// </summary>
public abstract class SharedDatabaseDbContext : DbContext
{
    private readonly ISharedDatabase _sharedDatabase;
    public bool Disposed { get; private set; } = false;

    public SharedDatabaseDbContext(ISharedDatabase sharedDatabase)
    {
        _sharedDatabase = sharedDatabase;
        _sharedDatabase.TransactionStarted += SharedDatabase_TransactionStarted;
        _sharedDatabase.TransactionConsumed += SharedDatabase_TransactionConsumed;
        if (_sharedDatabase.Transaction != null)
        {
            // if a transaction was started on the connection before this instance was created,
            // enlist that transaction.
            Database.UseTransaction(_sharedDatabase.Transaction);
        }
    }

    public override void Dispose()
    {
        // unsubscribe from events
        _sharedDatabase.TransactionStarted -= SharedDatabase_TransactionStarted;
        _sharedDatabase.TransactionConsumed -= SharedDatabase_TransactionConsumed;
        base.Dispose();
        GC.SuppressFinalize(this);
        Disposed = true;
    }

    private void SharedDatabase_TransactionConsumed(object? sender, EventArgs e)
    {
        // The transaction was committed or rolled back, we no longer need to track it.
        Database.UseTransaction(null);
    }

    private void SharedDatabase_TransactionStarted(object? sender, EventArgs e)
    {
        // a transaction was started on a context that shares this connection.
        // if it wasn't this instance we need to enlist it.
        if (!ReferenceEquals(Database.CurrentTransaction?.GetDbTransaction(), _sharedDatabase.Transaction))
        {
            Database.UseTransaction(_sharedDatabase.Transaction);
        }
    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        // Makes EF use the existing SQL connection instead of creating its own.
        if (_sharedDatabase.Connection.State != System.Data.ConnectionState.Open)
            _sharedDatabase.Connection.Open();
        // tell EF that it does not own the connection so that it will not try to dispose it.
        optionsBuilder.UseSqlServer(_sharedDatabase.Connection, false);
    }
}
