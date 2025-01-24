using Microsoft.Data.SqlClient;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;

namespace PeachtreeBus.DatabaseSharing.Tests;

public class SqlTransactionFake : ISqlTransaction
{
    public SqlTransactionFake()
    {
        Transaction = (SqlTransaction)RuntimeHelpers.GetUninitializedObject(typeof(SqlTransaction));
    }

    public bool Disposed { get; private set; } = false;

    public SqlTransaction Transaction { get; }

    public bool Committed { get; private set; } = false;

    public void Commit()
    {
        Assert.IsFalse(Disposed, "Attempt to commit a disposed transaction.");
        Assert.IsFalse(Committed, "Transaction has been committed twice.");
        Committed = true;
    }

    public void Dispose()
    {
        Disposed = true;
        GC.SuppressFinalize(this);
    }

    public bool RolledBack { get; private set; } = false;

    public void Rollback()
    {
        Assert.IsFalse(RolledBack, "Transaction has been rolled back twice.");
        RolledBack = true;
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

    public string? LastSaveName { get => SavePoints.LastOrDefault(); }

    public void Save(string savePointName)
    {
        CollectionAssert.DoesNotContain(SavePoints, LastSaveName, "Attempt to re-use an existing save point.");
        SavePoints.Add(savePointName);
    }
}
