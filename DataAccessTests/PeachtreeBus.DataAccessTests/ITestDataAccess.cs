using System;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using PeachtreeBus.Data;
using PeachtreeBus.Queues;
using PeachtreeBus.Subscriptions;

namespace PeachtreeBus.DataAccessTests;

public interface ILockedRows<T> : IDisposable
{
    List<T> Data { get; }
}

public interface ITestDataAccess
{
    Task Initialize();
    Task CleanEverything();
    Task CloseConnections();
    Task<long> CountRowsInTable(TableName tableName);
    DataSet GetTableContent(TableName tableName);
    Task<List<T>> GetTableContent<T>(TableName tableName) where T : class;
    Task InsertQueueCompleted(QueueData data);
    Task InsertQueueFailed(QueueData data);
    Task InsertSubscribedPending(SubscribedData data);
    Task InsertSubscribedCompleted(SubscribedData data);
    Task InsertSubscribedFailed(SubscribedData data);
    ITestConfig TestConfig { get; }
    ILockedRows<T> LockRows<T>(TableName tableName, int count = int.MaxValue);
}

public static class TestDataAccessExtensions
{
    public static async Task Then_TableHasCount(this ITestDataAccess dataAccess, TableName tableName, int expectedCount) =>
        Assert.AreEqual(expectedCount, await dataAccess.CountRowsInTable(tableName));

    public static Task Then_TableIsEmpty(this ITestDataAccess dataAccess, TableName tableName) =>
        Then_TableHasCount(dataAccess, tableName, 0);

    public static Task<List<SubscriptionsRow>> GetSubscriptions(this ITestDataAccess dataAccess) =>
        dataAccess.GetTableContent<SubscriptionsRow>(dataAccess.TestConfig.Subscriptions);

    public static Task<List<SubscribedData>> GetSubscribedPending(this ITestDataAccess dataAccess) =>
        dataAccess.GetTableContent<SubscribedData>(dataAccess.TestConfig.SubscribedPending);

    public static Task<List<SubscribedData>> GetSubscribedFailed(this ITestDataAccess dataAccess) =>
        dataAccess.GetTableContent<SubscribedData>(dataAccess.TestConfig.SubscribedFailed);

    public static Task<List<SubscribedData>> GetSubscribedCompleted(this ITestDataAccess dataAccess) =>
        dataAccess.GetTableContent<SubscribedData>(dataAccess.TestConfig.SubscribedCompleted);

    public static Task<List<QueueData>> GetQueuedPending(this ITestDataAccess dataAccess) =>
        dataAccess.GetTableContent<QueueData>(dataAccess.TestConfig.QueuePending);

    public static Task<List<QueueData>> GetQueuedCompleted(this ITestDataAccess dataAccess) =>
        dataAccess.GetTableContent<QueueData>(dataAccess.TestConfig.QueueCompleted);

    public static Task<List<QueueData>> GetQueuedFailed(this ITestDataAccess dataAccess) =>
        dataAccess.GetTableContent<QueueData>(dataAccess.TestConfig.QueueFailed);
}