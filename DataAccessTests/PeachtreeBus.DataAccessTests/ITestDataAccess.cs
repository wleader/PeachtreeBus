using System.Collections.Generic;
using System.Data;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using PeachtreeBus.Data;
using PeachtreeBus.Queues;

namespace PeachtreeBus.DataAccessTests;

public interface ITestDataAccess
{
    void Initialize();
    void CleanEverything();
    void CloseConnections();
    long CountRowsInTable(TableName tableName);
    DataSet GetTableContent(TableName tableName);
    List<T> GetTableContent<T>(TableName tableName) where T : class;
    void InsertQueueCompleted(QueueData data);
}

public static class TestDataAccessExtensions
{
    public static void Then_TableHasCount(this ITestDataAccess dataAccess, TableName tableName, int expectedCount) =>
        Assert.AreEqual(expectedCount, dataAccess.CountRowsInTable(tableName));

    public static void Then_TableIsEmpty(this ITestDataAccess dataAccess, TableName tableName) =>
        Then_TableHasCount(dataAccess, tableName, 0);
}