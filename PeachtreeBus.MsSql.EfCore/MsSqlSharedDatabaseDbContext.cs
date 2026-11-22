using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using PeachtreeBus.DatabaseSharing;

namespace PeachtreeBus.MsSql.EfCore;

/// <summary>
/// A DB Context that automatically shares the transaction and connection with other
/// instances of itself within the same Dependency Injection Scope.
/// </summary>
public abstract class MsSqlSharedDatabaseDbContext : DbContext
{
    protected readonly ISqlSharedDatabase SharedDatabase;

    protected MsSqlSharedDatabaseDbContext(ISqlSharedDatabase sharedDatabase)
    {
        SharedDatabase = sharedDatabase;
        SharedDatabase.TransactionStarted += SharedDatabase_TransactionStarted;
        SharedDatabase.TransactionConsumed += SharedDatabase_TransactionConsumed;

        // if a transaction was started on the connection
        // before this instance was created, enlist that transaction.
        // if its null, that's OK too.

        // This class is meant to be inherited. If someone wants to override
        // The .Database property, then its on them to make sure this works.
        // ReSharper disable once VirtualMemberCallInConstructor
        Database.UseTransaction(SharedDatabase.NativeTransaction);
    }

    public override void Dispose()
    {
        // unsubscribe from events
        SharedDatabase.TransactionStarted -= SharedDatabase_TransactionStarted;
        SharedDatabase.TransactionConsumed -= SharedDatabase_TransactionConsumed;
        base.Dispose();
        GC.SuppressFinalize(this);
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
        if (!ReferenceEquals(
                Database.CurrentTransaction?.GetDbTransaction(),
                SharedDatabase.NativeTransaction))
        {
            Database.UseTransaction(SharedDatabase.NativeTransaction);
        }
    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        if (SharedDatabase.NativeConnection.State != System.Data.ConnectionState.Open)
            SharedDatabase.NativeConnection.Open();

        // Makes EF use the existing SQL connection instead of creating its own.
        // contextOwnsConnection: false tells the context to not dispose the connection
        // whe it disposes.
        optionsBuilder.UseSqlServer(SharedDatabase.NativeConnection, false);
    }
}
