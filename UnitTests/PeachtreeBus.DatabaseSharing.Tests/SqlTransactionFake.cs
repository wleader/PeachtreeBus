using Microsoft.Data.SqlClient;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using PeachtreeBus.Testing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace PeachtreeBus.DatabaseSharing.Tests;

public class SqlTransactionFake : ISqlTransaction
{
    public bool Disposed { get; private set; }

    public SqlTransaction Transaction { get; } = SqlServerTesting.CreateTransaction();

    public bool Committed { get; private set; }

    public void Commit()
    {
        Assert.IsFalse(Disposed, "Attempt to commit a disposed transaction.");
        Assert.IsFalse(Committed, "Transaction has been committed twice.");
        Committed = true;
    }

    public Task CommitAsync(CancellationToken cancellationToken = default)
    {
        Commit();
        return Task.CompletedTask;
    }

    public void Dispose()
    {
        Disposed = true;
        GC.SuppressFinalize(this);
    }

    public ValueTask DisposeAsync()
    {
        Disposed = true;
        GC.SuppressFinalize(this);
        return ValueTask.CompletedTask;
    }

    public bool RolledBack { get; private set; }

    public void Rollback()
    {
        Assert.IsFalse(RolledBack, "Transaction has been rolled back twice.");
        RolledBack = true;
    }

    public Task RollbackAsync(CancellationToken cancellationToken = default)
    {
        Rollback();
        return Task.CompletedTask;
    }

    public List<string> SavePoints { get; } = [];

    public string? LastRollbackName { get; private set; }

    public void Rollback(string transactionName)
    {
        CollectionAssert.Contains(SavePoints, transactionName, "Attempt to rollback to a savepoint that was not created.");
        var index = SavePoints.IndexOf(transactionName);
        while (SavePoints.Count > index)
        {
            SavePoints.RemoveAt(index);
        }

        LastRollbackName = transactionName;
    }

    public Task RollbackAsync(string transactionName, CancellationToken cancellationToken = default)
    {
        Rollback(transactionName);
        return Task.CompletedTask;
    }

    public string? LastSaveName => SavePoints.LastOrDefault();

    public void Save(string savePointName)
    {
        CollectionAssert.DoesNotContain(SavePoints, LastSaveName, "Attempt to re-use an existing save point.");
        SavePoints.Add(savePointName);
    }

    public Task SaveAsync(string savePointName, CancellationToken cancellationToken = default)
    {
        Save(savePointName);
        return Task.CompletedTask;
    }
}
