using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Dapper;
using Microsoft.Extensions.Logging;
using PeachtreeBus.Data;
using PeachtreeBus.Queues;
using PeachtreeBus.Subscriptions;
using PeachtreeBus.Telemetry;

namespace PeachtreeBus.Management;

public class PostgreSqlManagementDataAccess(
    IBusConfiguration configuration,
    ILogger<PostgreSqlManagementDataAccess> log,
    IDapperMethods dapper) : IManagementDataAccess
{
    private const string QueueFields = "id, message_id, priority, not_before, enqueued, completed, failed, retries, headers, body";
    
    private static readonly TableName Pending = new("pending");
    
    private async Task<List<T>> GetMessages<T>(string fields, QueueName queueName, TableName table, int skip, int take)
    {
        const string template =
            """
            SELECT {3} FROM {0}.{1}_{2}
            ORDER BY enqueued DESC
            LIMIT @Take
            OFFSET @Skip;
            """;

        string statement = string.Format(template, configuration.Schema, queueName, table, fields);

        var p = new DynamicParameters();
        p.Add("@Skip", skip);
        p.Add("@Take", take);

        return [.. (await LogIfError(dapper.Query<T>(statement, p)))];
    }
    
    public Task<List<QueueData>> GetFailedQueueMessages(QueueName queueName, int skip, int take)
    {
        throw new NotImplementedException();
    }

    public Task<List<QueueData>> GetCompletedQueueMessages(QueueName queueName, int skip, int take)
    {
        throw new NotImplementedException();
    }

    public async Task<List<QueueData>> GetPendingQueueMessages(QueueName queueName, int skip, int take)
    {
        using var _ = StartActivity();
        return await GetMessages<QueueData>(QueueFields, queueName, Pending, skip, take);
    }

    public Task CancelPendingQueueMessage(QueueName queueName, Identity id)
    {
        throw new NotImplementedException();
    }

    public Task RetryFailedQueueMessage(QueueName queueName, Identity id)
    {
        throw new NotImplementedException();
    }

    public Task<List<SubscribedData>> GetFailedSubscribedMessages(int skip, int take)
    {
        throw new NotImplementedException();
    }

    public Task<List<SubscribedData>> GetCompletedSubscribedMessages(int skip, int take)
    {
        throw new NotImplementedException();
    }

    public Task<List<SubscribedData>> GetPendingSubscribedMessages(int skip, int take)
    {
        throw new NotImplementedException();
    }

    public Task CancelPendingSubscribedMessage(Identity id)
    {
        throw new NotImplementedException();
    }

    public Task RetryFailedSubscribedMessage(Identity id)
    {
        throw new NotImplementedException();
    }
    
    [ExcludeFromCodeCoverage]
    private async Task<T> LogIfError<T>(Task<T> task, [CallerMemberName] string caller = "Unnamed")
    {
        try
        {
            return await task;
        }
        catch (Exception ex)
        {
            log.DataAccessError(caller, ex);
            throw;
        }
    }

    private static Activity? StartActivity([CallerMemberName] string caller = "Unnamed")
    {
        return ActivitySources.DataAccess.StartActivity(
            "peachtreebus.managementdataaccess " + caller);
    }
}