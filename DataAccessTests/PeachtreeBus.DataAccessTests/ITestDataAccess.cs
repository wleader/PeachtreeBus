using System;
using System.Collections.Generic;
using System.Data;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using PeachtreeBus.Data;
using PeachtreeBus.Queues;
using PeachtreeBus.Subscriptions;

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
    void InsertQueueFailed(QueueData data);
    void InsertSubscribedPending(SubscribedData data);
    void InsertSubscribedCompleted(SubscribedData data);
    void InsertSubscribedFailed(SubscribedData data);
    ITestConfig TestConfig { get; }
    IDisposable LockRows(TableName tableName, int count);
}

public static class TestDataAccessExtensions
{
    public static void Then_TableHasCount(this ITestDataAccess dataAccess, TableName tableName, int expectedCount) =>
        Assert.AreEqual(expectedCount, dataAccess.CountRowsInTable(tableName));

    public static void Then_TableIsEmpty(this ITestDataAccess dataAccess, TableName tableName) =>
        Then_TableHasCount(dataAccess, tableName, 0);

    public static List<SubscriptionsRow> GetSubscriptions(this ITestDataAccess dataAccess) =>
        dataAccess.GetTableContent<SubscriptionsRow>(dataAccess.TestConfig.Subscriptions);

    public static List<SubscribedData> GetSubscribedPending(this ITestDataAccess dataAccess) =>
        dataAccess.GetTableContent<SubscribedData>(dataAccess.TestConfig.SubscribedPending);

    public static List<SubscribedData> GetSubscribedFailed(this ITestDataAccess dataAccess) =>
        dataAccess.GetTableContent<SubscribedData>(dataAccess.TestConfig.SubscribedFailed);

    public static List<SubscribedData> GetSubscribedCompleted(this ITestDataAccess dataAccess) =>
        dataAccess.GetTableContent<SubscribedData>(dataAccess.TestConfig.SubscribedCompleted);

    public static List<QueueData> GetQueuedPending(this ITestDataAccess dataAccess) =>
        dataAccess.GetTableContent<QueueData>(dataAccess.TestConfig.QueuePending);

    public static List<QueueData> GetQueuedCompleted(this ITestDataAccess dataAccess) =>
        dataAccess.GetTableContent<QueueData>(dataAccess.TestConfig.QueueCompleted);

    public static List<QueueData> GetQueuedFailed(this ITestDataAccess dataAccess) =>
        dataAccess.GetTableContent<QueueData>(dataAccess.TestConfig.QueueFailed);
}