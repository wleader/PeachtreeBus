using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Dapper;
using Microsoft.Extensions.Logging;
using PeachtreeBus.Queues;
using PeachtreeBus.Sagas;
using PeachtreeBus.Subscriptions;
using PeachtreeBus.Telemetry;

namespace PeachtreeBus.Data;

public class PostgreSqlBusDataAccess(
    ILogger<PostgreSqlBusDataAccess> log,
    IDapperMethods dapper,
    IBusConfiguration configuration)
    : IBusDataAccess
{
    public void BeginTransaction()
    {
        throw new NotImplementedException();
    }

    public void CommitTransaction()
    {
        throw new NotImplementedException();
    }

    public void RollbackTransaction()
    {
        throw new NotImplementedException();
    }

    public void CreateSavepoint(string name)
    {
        throw new NotImplementedException();
    }

    public void RollbackToSavepoint(string name)
    {
        throw new NotImplementedException();
    }

    public void Reconnect()
    {
        throw new NotImplementedException();
    }

    public Task<QueueData?> GetPendingQueued(QueueName queueName)
    {
        throw new NotImplementedException();
    }

    public async Task<long> EstimateQueuePending(QueueName queueName)
    {
        // Gets the max and min IDs and does a diff on them.
        // It doesn't check that all the rows between those values
        // have a not-before less than now.
        // If there are rows in between the max and the min where the 
        // not-before is in the future, this will over-estimate.
        // It is ok to over-estimate.
        const string estimateQueuedStatement =
            """
            SELECT MAX(id)-MIN(id)+1 as count
            FROM {0}.{1}_Pending
            WHERE not_before < NOW()
            """;

        using var _ = StartActivity();

        var query = string.Format(estimateQueuedStatement, configuration.Schema, queueName);

        return await LogIfError(dapper.ExecuteScalar<long>(query));
    }

    public async Task<Identity> AddMessage(QueueData message, QueueName queueName)
    {
        const string enqueueMessageStatement =
            """
            INSERT INTO {0}.{1}_Pending
            (message_id, priority, not_before, enqueued, completed, failed, retries, headers, body)
            VALUES
            (@MessageId, @Priority, @NotBefore, NOW(), NULL, NULL, 0, @Headers, @Body)
            RETURNING id;
            """;

        ArgumentNullException.ThrowIfNull(message);

        using var _ = StartActivity();

        string statement = string.Format(enqueueMessageStatement, configuration.Schema, queueName);

        var p = new DynamicParameters();
        p.Add("@MessageId", message.MessageId);
        p.Add("@Priority", message.Priority);
        p.Add("@NotBefore", message.NotBefore);
        p.Add("@Headers", message.Headers);
        p.Add("@Body", message.Body);

        return await LogIfError(dapper.QueryFirst<Identity>(statement, p));
    }

    public Task CompleteMessage(QueueData message, QueueName queueName)
    {
        throw new NotImplementedException();
    }

    public Task FailMessage(QueueData message, QueueName queueName)
    {
        throw new NotImplementedException();
    }

    public Task UpdateMessage(QueueData message, QueueName queueName)
    {
        throw new NotImplementedException();
    }

    public Task<Identity> InsertSagaData(SagaData data, SagaName sagaName)
    {
        throw new NotImplementedException();
    }

    public Task UpdateSagaData(SagaData data, SagaName sagaName)
    {
        throw new NotImplementedException();
    }

    public Task<SagaData?> GetSagaData(SagaName sagaName, SagaKey key)
    {
        throw new NotImplementedException();
    }

    public Task DeleteSagaData(SagaName sagaName, SagaKey key)
    {
        throw new NotImplementedException();
    }

    public async Task<long> ExpireSubscriptions(int maxCount)
    {
        const string expireSubscriptionsStatement =
            """
            WITH deleted_rows AS (
                DELETE FROM  {0}.subscriptions
                WHERE id IN (
                    SELECT id FROM {0}.subscriptions
                    WHERE valid_until < NOW()
                    LIMIT @MaxCount
                    FOR UPDATE SKIP LOCKED
                )
                RETURNING id
            )
            SELECT COUNT(*) FROM deleted_rows;
            """;

        using var _ = StartActivity();

        string statement = string.Format(expireSubscriptionsStatement, configuration.Schema);

        var p = new DynamicParameters();
        p.Add("@MaxCount", maxCount);

        return await LogIfError(dapper.QueryFirst<long>(statement, p));
    }

    public async Task Subscribe(SubscriberId subscriberId, Topic topic, UtcDateTime until)
    {
        const string subscribeStatement =
            """
            INSERT INTO {0}.subscriptions (subscriber_id, topic, valid_until)
            VALUES (@SubscriberId, @Topic, @ValidUntil)
            ON CONFLICT (subscriber_id, topic)
            DO UPDATE SET valid_until = @ValidUntil;
            """;

        using var _ = StartActivity();

        string statement = string.Format(subscribeStatement, configuration.Schema);

        var p = new DynamicParameters();
        p.Add("@SubscriberId", subscriberId);
        p.Add("@Topic", topic);
        p.Add("@ValidUntil", until);

        await LogIfError(dapper.Execute(statement, p));
    }

    public async Task<SubscribedData?> GetPendingSubscribed(SubscriberId subscriberId)
    {
        // FOR UPDATE SKIP LOCKED
        // makes this row unavailable to other connections and transactions.
        // skip any rows that are locked by other connections and transactions.
        // not_before so we don't get messages that are scheduled for the future.
        const string statement =
            """
            SELECT id, subscriber_id, topic, valid_until, message_id, priority, not_before, enqueued, 
                   completed, failed, retries, headers, body
                FROM {0}.Subscribed_Pending
                WHERE not_before < NOW()
                AND subscriber_id = @SubscriberId
                ORDER BY priority DESC
                LIMIT 1
                FOR UPDATE SKIP LOCKED;
            """;

        using var _ = StartActivity();

        var query = string.Format(statement, configuration.Schema);

        var p = new DynamicParameters();
        p.Add("@SubscriberId", subscriberId);

        return await LogIfError(dapper.QueryFirstOrDefault<SubscribedData>(query, p));
    }

    public async Task<long> EstimateSubscribedPending(SubscriberId subscriberId)
    {
        const string estimateQueuedStatement =
            """
            SELECT COUNT(*)
            FROM {0}.subscribed_pending
            WHERE subscriber_id = @SubscriberId
            AND not_before < NOW()
            """;

        using var _ = StartActivity();

        var query = string.Format(estimateQueuedStatement, configuration.Schema);

        var p = new DynamicParameters();
        p.Add("@SubscriberId", subscriberId);

        return await LogIfError(dapper.ExecuteScalar<long>(query, p));
    }

    public Task<long> Publish(SubscribedData message, Topic topic)
    {
        throw new NotImplementedException();
    }

    public Task CompleteMessage(SubscribedData message)
    {
        throw new NotImplementedException();
    }

    public Task FailMessage(SubscribedData message)
    {
        throw new NotImplementedException();
    }

    public async Task UpdateMessage(SubscribedData message)
    {
        const string updateMessageStatement =
            """
            UPDATE {0}.Subscribed_Pending
            SET not_before = @NotBefore,
                retries = @Retries,
                headers = @Headers
            WHERE id = @Id
            """;

        using var _ = StartActivity();

        ArgumentNullException.ThrowIfNull(message);

        var statement = string.Format(updateMessageStatement, configuration.Schema);

        var p = new DynamicParameters();
        p.Add("@Id", message.Id);
        p.Add("@NotBefore", message.NotBefore);
        p.Add("@Retries", message.Retries);
        p.Add("@Headers", message.Headers);

        await LogIfError(dapper.Execute(statement, p));
    }

    public Task<long> ExpireSubscriptionMessages(int maxCount)
    {
        throw new NotImplementedException();
    }

    public async Task<long> CleanQueueFailed(QueueName queueName, UtcDateTime olderThan, int maxCount)
    {
        const string statementTemplate =
            """
            WITH deleted_rows AS (
                DELETE FROM {0}.{1}_failed
                WHERE id IN (
                    SELECT id FROM {0}.{1}_failed
                    WHERE failed < @OlderThan
                    LIMIT @MaxCount
                    FOR UPDATE SKIP LOCKED
                )
                RETURNING id
            )
            SELECT COUNT(*) FROM deleted_rows;
            """;

        using var _ = StartActivity();

        string statement = string.Format(statementTemplate, configuration.Schema, queueName);

        var p = new DynamicParameters();
        p.Add("@MaxCount", maxCount);
        p.Add("@OlderThan", olderThan);

        return await LogIfError(dapper.QueryFirst<long>(statement, p));
    }

    public async Task<long> CleanQueueCompleted(QueueName queueName, UtcDateTime olderThan, int maxCount)
    {
        const string statementTemplate =
            """
            WITH deleted_rows AS (
                DELETE FROM {0}.{1}_completed
                WHERE id IN (
                    SELECT id FROM {0}.{1}_completed
                    WHERE completed < @OlderThan
                    LIMIT @MaxCount
                    FOR UPDATE SKIP LOCKED
                )
                RETURNING id
            )
            SELECT COUNT(*) FROM deleted_rows;
            """;

        using var _ = StartActivity();

        string statement = string.Format(statementTemplate, configuration.Schema, queueName);

        var p = new DynamicParameters();
        p.Add("@MaxCount", maxCount);
        p.Add("@OlderThan", olderThan);

        return await LogIfError(dapper.QueryFirst<long>(statement, p));
    }

    public async Task<long> CleanSubscribedCompleted(UtcDateTime olderThan, int maxCount)
    {
        const string statementTemplate =
            """
            WITH deleted_rows AS (
                DELETE FROM {0}.subscribed_completed
                WHERE id IN (
                    SELECT id FROM {0}.subscribed_completed
                    WHERE completed < @OlderThan
                    LIMIT @MaxCount
                    FOR UPDATE SKIP LOCKED
                )
                RETURNING id
            )
            SELECT COUNT(*) FROM deleted_rows;
            """;

        using var _ = StartActivity();

        string statement = string.Format(statementTemplate, configuration.Schema);

        var p = new DynamicParameters();
        p.Add("@MaxCount", maxCount);
        p.Add("@OlderThan", olderThan);

        return await LogIfError(dapper.QueryFirst<long>(statement, p));
    }

    public async Task<long> CleanSubscribedFailed(UtcDateTime olderThan, int maxCount)
    {
        const string statementTemplate =
            """
            WITH deleted_rows AS (
                DELETE FROM {0}.subscribed_failed
                WHERE id IN (
                    SELECT id FROM {0}.subscribed_failed
                    WHERE failed < @OlderThan
                    LIMIT @MaxCount
                    FOR UPDATE SKIP LOCKED
                )
                RETURNING id
            )
            SELECT COUNT(*) FROM deleted_rows;
            """;

        using var _ = StartActivity();

        string statement = string.Format(statementTemplate, configuration.Schema);

        var p = new DynamicParameters();
        p.Add("@MaxCount", maxCount);
        p.Add("@OlderThan", olderThan);

        return await LogIfError(dapper.QueryFirst<long>(statement, p));
    }

    private async Task<T> LogIfError<T>(Task<T> task,
        [CallerMemberName] string caller = "Unnamed")
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

    private static Activity? StartActivity(
        [CallerMemberName] string caller = "Unnamed")
    {
        return ActivitySources.DataAccess.StartActivity(
            "peachtreebus.dataaccess " + caller)
            ?.AddTag("DatabaseType", "PostgreSql");
    }
}