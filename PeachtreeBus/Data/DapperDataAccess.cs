using Dapper;
using Microsoft.Extensions.Logging;
using PeachtreeBus.DatabaseSharing;
using PeachtreeBus.Queues;
using PeachtreeBus.Sagas;
using PeachtreeBus.Subscriptions;
using PeachtreeBus.Telemetry;
using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace PeachtreeBus.Data
{

    /// <summary>
    /// An implemenatin of IBusDataAccess that uses Dapper to accees the SQL database.
    /// </summary>
    /// <remarks>
    /// Constructor
    /// </remarks>
    /// <param name="database">A Shared Database connection.</param>
    /// <param name="configuration">Configures which DB Schema to find all the tables in.</param>
    public class DapperDataAccess(
        ISharedDatabase database,
        IBusConfiguration configuration,
        ILogger<DapperDataAccess> log)
        : IBusDataAccess
    {
        static DapperDataAccess()
        {
            DapperTypeHandlers.AddHandlers();
        }

        private readonly ISharedDatabase _database = database;
        private readonly ILogger<DapperDataAccess> _log = log;
        private readonly IBusConfiguration _configuration = configuration;

        /// <summary>
        /// Adds a queue message to the queue's pending table.
        /// </summary>
        /// <param name="message"></param>
        /// <param name="queueName"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentException"></exception>
        /// <exception cref="ArgumentNullException"></exception>
        public async Task<Identity> AddMessage(QueueData message, QueueName queueName)
        {
            const string EnqueueMessageStatement =
                """
                INSERT INTO [{0}].[{1}_Pending] WITH (ROWLOCK)
                ([MessageId], [Priority], [NotBefore], [Enqueued], [Completed], [Failed], [Retries], [Headers], [Body])
                OUTPUT INSERTED.[Id]
                VALUES
                (@MessageId, @Priority, @NotBefore, SYSUTCDATETIME(), NULL, NULL, 0, @Headers, @Body)
                """;

            ArgumentNullException.ThrowIfNull(message);

            using var _ = StartActivity(nameof(AddMessage));

            string statement = string.Format(EnqueueMessageStatement, _configuration.Schema, queueName);

            var p = new DynamicParameters();
            p.Add("@MessageId", message.MessageId);
            p.Add("@Priority", message.Priority);
            p.Add("@NotBefore", message.NotBefore);
            p.Add("@Headers", message.Headers);
            p.Add("@Body", message.Body);

            return message.Id = await LogIfError(
                _database.Connection.QueryFirstAsync<Identity>(statement, p, _database.Transaction));
        }

        /// <summary>
        /// Gets an eligble pending message from the pending table.
        /// </summary>
        /// <param name="queueName"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentException"></exception>
        public async Task<QueueData?> GetPendingQueued(QueueName queueName)
        {
            // UPDLOCK makes this row unavailable to other connections and transactions.
            // READPAST to skip any rows that are locked by other connections and transactions.
            // ROWLOCK hint to tell the server to lock at the row level instead of the default page lock.
            // NotBefore so we don't get messages that are scheduled for the future.
            // Completed and Failed are null means not previously processed and not previously exceeded retry count.
            const string GetOnePendingMessageStatement =
                """
                SELECT TOP 1 [Id], [MessageId], [Priority], [NotBefore], [Enqueued], [Completed], [Failed], [Retries], [Headers], [Body]
                FROM [{0}].[{1}_Pending] WITH(UPDLOCK, READPAST, ROWLOCK)
                WHERE NotBefore < SYSUTCDATETIME()
                ORDER BY [Priority] DESC
                """;

            using var _ = StartActivity(nameof(GetPendingQueued));

            var query = string.Format(GetOnePendingMessageStatement, _configuration.Schema, queueName);

            return await LogIfError(
                _database.Connection.QueryFirstOrDefaultAsync<QueueData>(query, transaction: _database.Transaction));
        }

        /// <inheritdoc/>
        public async Task<long> EstimateQueuePending(QueueName queueName)
        {
            // Gets the max and min IDs and does a diff on them.
            // It doesn't check that all the rows between those values
            // have a not-before less than now.
            // If there are rows in between the max and the min where the 
            // not before is in the future, this will over-estimate.
            // It is ok to over-estimate.
            const string EstimateQueuedStatement =
                """
                SELECT ISNULL(CAST(MAX(Id) - MIN([Id]) + 1 AS BIGINT), 0)
                FROM [{0}].[{1}_Pending] WITH (READPAST, READCOMMITTEDLOCK)
                WHERE NotBefore < SYSUTCDATETIME()
                """;

            using var _ = StartActivity(nameof(EstimateQueuePending));

            var query = string.Format(EstimateQueuedStatement, _configuration.Schema, queueName);

            return await LogIfError(
                _database.Connection.ExecuteScalarAsync<long>(query, transaction: _database.Transaction));
        }

        /// <summary>
        /// Moves a message from the pending table to the completed table.
        /// </summary>
        /// <param name="message"></param>
        /// <param name="queueName"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentException"></exception>
        /// <exception cref="ArgumentNullException"></exception>
        public async Task CompleteMessage(QueueData message, QueueName queueName)
        {
            const string CompleteMessageStatement =
                """
                INSERT INTO [{0}].[{1}_Completed] WITH (ROWLOCK) 
                ([Id], [MessageId], [Priority], [NotBefore], [Enqueued], [Completed], [Failed], [Retries], [Headers], [Body])
                SELECT D.[Id], D.[MessageId], D.[Priority], D.[NotBefore], D.[Enqueued], SYSUTCDATETIME(), NULL, D.[Retries], D.[Headers], D.[Body]
                FROM (DELETE FROM [{0}].[{1}_Pending] WITH (ROWLOCK)
                      OUTPUT DELETED.*
                      WHERE [Id] = @Id) D
                """;

            ArgumentNullException.ThrowIfNull(message);

            using var _ = StartActivity(nameof(CompleteMessage));

            string statement = string.Format(CompleteMessageStatement, _configuration.Schema, queueName);

            var p = new DynamicParameters();
            p.Add("@Id", message.Id);

            await LogIfError(
                _database.Connection.ExecuteAsync(statement, p, _database.Transaction));
        }

        /// <summary>
        /// Moves a message from the pending table to the failed table.
        /// </summary>
        /// <param name="message"></param>
        /// <param name="queueName"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentException"></exception>
        /// <exception cref="ArgumentNullException"></exception>
        public async Task FailMessage(QueueData message, QueueName queueName)
        {
            const string FailMessageStatement =
                """
                INSERT INTO [{0}].[{1}_Failed] WITH (ROWLOCK)  
                ([Id], [MessageId], [Priority], [NotBefore], [Enqueued], [Completed], [Failed], [Retries], [Headers], [Body])
                SELECT D.[Id], D.[MessageId], D.[Priority], D.[NotBefore], D.[Enqueued], NULL, SYSUTCDATETIME(), D.[Retries], @Headers, D.[Body]
                FROM (DELETE FROM [{0}].[{1}_Pending] WITH (ROWLOCK)
                      OUTPUT DELETED.*
                      WHERE [Id] = @Id) D
                """;

            ArgumentNullException.ThrowIfNull(message);

            using var _ = StartActivity(nameof(FailMessage));

            string statement = string.Format(FailMessageStatement, _configuration.Schema, queueName);

            var p = new DynamicParameters();
            p.Add("@Id", message.Id);
            p.Add("@Headers", message.Headers);

            await LogIfError(
                _database.Connection.ExecuteAsync(statement, p, _database.Transaction));
        }

        /// <summary>
        /// Updates a message in the pending table
        /// </summary>
        /// <param name="message"></param>
        /// <param name="queueName"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentException"></exception>
        /// <exception cref="ArgumentNullException"></exception>
        public async Task UpdateMessage(QueueData message, QueueName queueName)
        {
            const string UpdateMessageStatement =
                """
                UPDATE [{0}].[{1}_Pending] WITH (ROWLOCK) SET
                [NotBefore] = @NotBefore,
                [Retries] = @Retries,
                [Headers] = @Headers
                WHERE [Id] = @Id
                """;

            ArgumentNullException.ThrowIfNull(message);

            using var _ = StartActivity(nameof(UpdateMessage));

            var statement = string.Format(UpdateMessageStatement, _configuration.Schema, queueName);

            var p = new DynamicParameters();
            p.Add("@Id", message.Id);
            p.Add("@NotBefore", message.NotBefore);
            p.Add("@Retries", message.Retries);
            p.Add("@Headers", message.Headers);

            await LogIfError(
                _database.Connection.ExecuteAsync(statement, p, _database.Transaction));
        }

        /// <summary>
        /// Inserts a row into a saga data table.
        /// </summary>
        /// <param name="data"></param>
        /// <param name="sagaName"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentException"></exception>
        /// <exception cref="ArgumentNullException"></exception>
        public async Task<Identity> InsertSagaData(SagaData data, SagaName sagaName)
        {
            const string InsertSagaStatement =
                """
                INSERT INTO[{0}].[{1}_SagaData] WITH (ROWLOCK)
                ([SagaId], [Key], [Data])
                OUTPUT INSERTED.[Id]
                VALUES
                (@SagaId, @Key, @Data)
                """;

            ArgumentNullException.ThrowIfNull(data);

            using var _ = StartActivity(nameof(InsertSagaData));

            string statement = string.Format(InsertSagaStatement, _configuration.Schema, sagaName);

            var p = new DynamicParameters();
            p.Add("@SagaId", data.SagaId);
            p.Add("@Key", data.Key);
            p.Add("@Data", data.Data);

            return await LogIfError(
                _database.Connection.QueryFirstAsync<Identity>(statement, p, _database.Transaction));
        }

        /// <summary>
        /// Updates a row in the saga data table.
        /// </summary>
        /// <param name="data"></param>
        /// <param name="sagaName"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentException"></exception>
        /// <exception cref="ArgumentNullException"></exception>
        public async Task UpdateSagaData(SagaData data, SagaName sagaName)
        {
            const string UpdateSagaStatement =
                """
                UPDATE [{0}].[{1}_SagaData] WITH (ROWLOCK) SET
                [Data] = @Data
                WHERE [Id] = @Id
                """;

            ArgumentNullException.ThrowIfNull(data);

            using var _ = StartActivity(nameof(UpdateSagaData));

            var statement = string.Format(UpdateSagaStatement, _configuration.Schema, sagaName);

            var p = new DynamicParameters();
            p.Add("@Id", data.Id);
            p.Add("@Data", data.Data);

            await LogIfError(
                _database.Connection.ExecuteAsync(statement, p, _database.Transaction));
        }

        /// <summary>
        /// Deletes a row from a saga data table.
        /// </summary>
        /// <param name="sagaName"></param>
        /// <param name="key"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentException"></exception>
        public async Task DeleteSagaData(SagaName sagaName, SagaKey key)
        {
            const string DeleteSagaStatement =
                """
                DELETE FROM [{0}].[{1}_SagaData] WITH (ROWLOCK)
                WHERE [Key] = @Key
                """;

            using var _ = StartActivity(nameof(DeleteSagaData));

            string statement = string.Format(DeleteSagaStatement, _configuration.Schema, sagaName);
            var p = new DynamicParameters();
            p.Add("@Key", key);

            await LogIfError(
                _database.Connection.ExecuteAsync(statement, p, _database.Transaction));
        }

        /// <summary>
        /// Reads a row from a saga data table.
        /// </summary>
        /// <param name="sagaName"></param>
        /// <param name="key"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentException"></exception>
        public async Task<SagaData?> GetSagaData(SagaName sagaName, SagaKey key)
        {
            // This is a multi-step operation.
            // First we try to select and lock the target row into variables with an UPDLOCK and NOWAIT.
            // If the first select locked the row, 
            //      The @@ROWCOUNT will be 1, and we can return the selected data.
            // If the first select could not lock the row,
            //      The @@ROWCOUNT will be 0. It could be because another thread has locked it,
            //      or the row doesn't exist. To Determine if the row exists, we select again with only the
            //      NOWAIT hint.
            // If the second select succeeds, then it means that another thread has locked it, and we can return
            //      a result with Bockled as 1 (true).
            // If the second select throws,
            //      it means that the row really is locked by someone else.
            // If any select fails with error 1222, then the row is locked by someone else
            //      return a blocked result.
            const string GetSagaDataStatement =
                """
                DECLARE
                    @Id bigint,
                    @SagaId uniqueidentifier,
                    @Data nvarchar(max)
                BEGIN TRY
                    SELECT @Id = [Id],
                           @SagaId = [SagaId],
                           @Data = [Data]
                        FROM [{0}].[{1}_SagaData] WITH (NOWAIT, UPDLOCK, ROWLOCK)
                        WHERE[Key] = @Key

                    IF @@ROWCOUNT > 0
                        SELECT @Id as [Id],
                               @SagaId as [SagaId],
                               @Key as [Key],
                               @Data as [Data],
                               0 as [Blocked]
                    ELSE
                        SELECT [Id], [SagaId], [Key], [Data], 1 as [Blocked]
                            FROM [{0}].[{1}_SagaData] WITH (NOWAIT)
                            WHERE [Key] = @Key
                END TRY
                BEGIN CATCH
                    IF (ERROR_NUMBER() != 1222) THROW
                    SELECT CAST(0 as BIGINT) as [Id],
                           CONVERT(uniqueidentifier, '00000000-0000-0000-0000-000000000000') as [SagaId],
                           @Key as [Key],
                           'BLOCKED' as [Data],
                           1 as [Blocked]
                END CATCH
                """;

            using var _ = StartActivity(nameof(GetSagaData));

            var query = string.Format(GetSagaDataStatement, _configuration.Schema, sagaName);

            var p = new DynamicParameters();
            p.Add("@Key", key);

            return await LogIfError(
                _database.Connection.QueryFirstOrDefaultAsync<SagaData>(query, p, _database.Transaction));
        }

        /// <summary>
        /// Deletes expired rows from the subscriptions table.
        /// </summary>
        /// <returns></returns>
        /// <exception cref="ArgumentException"></exception>
        public async Task<long> ExpireSubscriptions(int maxCount)
        {
            const string ExpireSubscriptionsStatement =
                """
                DELETE TOP (@MaxCount) FROM [{0}].[Subscriptions] WITH (ROWLOCK, READPAST)
                WHERE [ValidUntil] < SYSUTCDATETIME()
                SELECT @@ROWCOUNT
                """;

            using var _ = StartActivity(nameof(ExpireSubscriptions));

            string statement = string.Format(ExpireSubscriptionsStatement, _configuration.Schema);

            var p = new DynamicParameters();
            p.Add("@MaxCount", maxCount);

            return await LogIfError(
                _database.Connection.QueryFirstAsync<long>(statement, p, _database.Transaction));
        }

        /// <summary>
        /// Adds or updates a row in the subscriptions table.
        /// </summary>
        /// <param name="subscriberId"></param>
        /// <param name="topic"></param>
        /// <param name="until"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentException"></exception>
        public async Task Subscribe(SubscriberId subscriberId, Topic topic, UtcDateTime until)
        {
            const string SubscribeStatement =
                """
                UPDATE [{0}].[Subscriptions] WITH (UPDLOCK, SERIALIZABLE)
                    SET [ValidUntil] = @ValidUntil
                    WHERE [SubscriberId] = @SubscriberId
                    AND [Topic] = @Topic
                IF @@ROWCOUNT = 0
                BEGIN
                    INSERT INTO [{0}].[Subscriptions] WITH (ROWLOCK)
                    ([SubscriberId], [Topic], [ValidUntil])
                    VALUES
                    (@SubscriberId, @Topic, @ValidUntil)
                END
                """;

            using var _ = StartActivity(nameof(Subscribe));

            string statement = string.Format(SubscribeStatement, _configuration.Schema);

            var p = new DynamicParameters();
            p.Add("@SubscriberId", subscriberId);
            p.Add("@Topic", topic);
            p.Add("@ValidUntil", until);

            await LogIfError(
                _database.Connection.ExecuteAsync(statement, p, _database.Transaction));
        }

        /// <summary>
        /// Gets an eligible pending subscribed message.
        /// </summary>
        /// <param name="subscriberId"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentException"></exception>
        public async Task<SubscribedData?> GetPendingSubscribed(SubscriberId subscriberId)
        {
            // UPDLOCK makes this row unavailable to other connections and transactions.
            // READPAST to skip any rows that are locked by other connections and transactions.
            // ROWLOCK hint to tell the server to lock at the row level instead of the default page lock.
            // NotBefore so we don't get messages that are scheduled for the future.
            // Completed and Failed are null means not previously processed and not previously exceeded retry count.
            const string statement =
                """
                SELECT TOP 1 [Id], [SubscriberId], [Topic], [ValidUntil], [MessageId], [Priority], [NotBefore], [Enqueued], [Completed], [Failed], [Retries], [Headers], [Body]
                    FROM [{0}].[Subscribed_Pending] WITH(UPDLOCK, READPAST, ROWLOCK)
                    WHERE NotBefore < SYSUTCDATETIME()
                    AND SubscriberId = @SubscriberId
                    ORDER BY [Priority] DESC
                """;

            using var _ = StartActivity(nameof(GetPendingSubscribed));

            var query = string.Format(statement, _configuration.Schema);

            var p = new DynamicParameters();
            p.Add("@SubscriberId", subscriberId);

            return await LogIfError(
                _database.Connection.QueryFirstOrDefaultAsync<SubscribedData>(query, p, _database.Transaction));
        }

        /// <inheritdoc/>
        public async Task<long> EstimateSubscribedPending(SubscriberId subscriberId)
        {
            // this table has an index that works well for this
            // and the volume of susbcribed messages is expected to be lower than
            // for queued, so an actual count is feasable here.
            const string EstimateQueuedStatement =
                """
                SELECT COUNT(*)
                FROM [{0}].[Subscribed_Pending] WITH (READPAST, READCOMMITTEDLOCK)
                WHERE SubscriberId = @SubscriberId
                AND NotBefore < SYSUTCDATETIME()
                """;

            using var _ = StartActivity(nameof(EstimateSubscribedPending));

            var query = string.Format(EstimateQueuedStatement, _configuration.Schema);

            var p = new DynamicParameters();
            p.Add("@SubscriberId", subscriberId);

            return await LogIfError(
                _database.Connection.ExecuteScalarAsync<long>(query, p, transaction: _database.Transaction));
        }

        public async Task<long> Publish(SubscribedData message, Topic topic)
        {
            const string PublishStatement =
                """
                INSERT INTO [{0}].[Subscribed_Pending] WITH (ROWLOCK)
                ([SubscriberId], [Topic], [ValidUntil], [MessageId], [Priority], [NotBefore], [Enqueued], [Completed], [Failed], [Retries], [Headers], [Body])
                SELECT [SubscriberId], @Topic, @ValidUntil, NEWID(), @Priority, @NotBefore, SYSUTCDATETIME(), NULL, NULL, 0, @Headers, @Body
                FROM [{0}].[Subscriptions]
                WHERE Topic = @Topic
                AND ValidUntil > SYSUTCDATETIME()
                SELECT @@ROWCOUNT
                """;

            ArgumentNullException.ThrowIfNull(message);

            using var _ = StartActivity(nameof(Publish));

            string statement = string.Format(PublishStatement, _configuration.Schema);

            var p = new DynamicParameters();
            p.Add("@Priority", message.Priority);
            p.Add("@ValidUntil", message.ValidUntil);
            p.Add("@NotBefore", message.NotBefore);
            p.Add("@Headers", message.Headers);
            p.Add("@Body", message.Body);
            p.Add("@Topic", topic);

            return await LogIfError(
                _database.Connection.QueryFirstAsync<long>(statement, p, _database.Transaction));
        }

        /// <summary>
        /// Moves a subscribed message from the pending table to the compelted table.
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentException"></exception>
        /// <exception cref="ArgumentNullException"></exception>
        public async Task CompleteMessage(SubscribedData message)
        {
            const string completeStatement =
                """
                INSERT INTO [{0}].[Subscribed_Completed] WITH (ROWLOCK)
                ([Id], [SubscriberId], [Topic], [ValidUntil], [MessageId], [Priority], [NotBefore], [Enqueued], [Completed], [Failed], [Retries], [Headers], [Body])
                SELECT D.[Id], D.[SubscriberId], D.[Topic], D.[ValidUntil], D.[MessageId], D.[Priority], D.[NotBefore], D.[Enqueued], SYSUTCDATETIME(), NULL, D.[Retries], D.[Headers], D.[Body] FROM
                    (DELETE FROM [{0}].[Subscribed_Pending] WITH (ROWLOCK)
                        OUTPUT DELETED.*
                        WHERE [Id] = @Id) D
                """;

            ArgumentNullException.ThrowIfNull(message);

            using var _ = StartActivity(nameof(CompleteMessage));

            string statement = string.Format(completeStatement, _configuration.Schema);

            var p = new DynamicParameters();
            p.Add("@Id", message.Id);

            await LogIfError(
                _database.Connection.ExecuteAsync(statement, p, _database.Transaction));
        }

        /// <summary>
        /// Moves a subscribed message from the pending table to the failed table.
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentException"></exception>
        /// <exception cref="ArgumentNullException"></exception>
        public async Task FailMessage(SubscribedData message)
        {
            const string FailMessageStatement =
                """
                INSERT INTO [{0}].[Subscribed_Failed] WITH (ROWLOCK)  
                ([Id], [SubscriberId], [Topic], [ValidUntil], [MessageId], [Priority], [NotBefore], [Enqueued], [Completed], [Failed], [Retries], [Headers], [Body])
                SELECT D.[Id], D.[SubscriberId], D.[Topic], D.[ValidUntil], D.[MessageId], D.[Priority], D.[NotBefore], D.[Enqueued], NULL, SYSUTCDATETIME(), D.[Retries], @Headers, D.[Body] FROM
                    (DELETE FROM [{0}].[Subscribed_Pending] WITH (ROWLOCK)
                        OUTPUT DELETED.*
                        WHERE [Id] = @Id) D
                """;

            ArgumentNullException.ThrowIfNull(message);

            using var _ = StartActivity(nameof(FailMessage));

            string statement = string.Format(FailMessageStatement, _configuration.Schema);

            var p = new DynamicParameters();
            p.Add("@Id", message.Id);
            p.Add("@Headers", message.Headers);

            await LogIfError(
                _database.Connection.ExecuteAsync(statement, p, _database.Transaction));
        }

        /// <summary>
        /// Updates a subscribed message in the pending table
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentException"></exception>
        /// <exception cref="ArgumentNullException"></exception>
        public async Task UpdateMessage(SubscribedData message)
        {
            const string UpdateMessageStatement =
                """
                UPDATE[{0}].[Subscribed_Pending] WITH (ROWLOCK)
                SET [NotBefore] = @NotBefore,
                    [Retries] = @Retries,
                    [Headers] = @Headers
                WHERE [Id] = @Id
                """;

            ArgumentNullException.ThrowIfNull(message);

            using var _ = StartActivity(nameof(UpdateMessage));

            var statement = string.Format(UpdateMessageStatement, _configuration.Schema);

            var p = new DynamicParameters();
            p.Add("@Id", message.Id);
            p.Add("@NotBefore", message.NotBefore);
            p.Add("@Retries", message.Retries);
            p.Add("@Headers", message.Headers);

            await LogIfError(
                _database.Connection.ExecuteAsync(statement, p, _database.Transaction));
        }

        /// <summary>
        /// Removes subscribed messages that have expired (Time is after Valid Until)
        /// </summary>
        /// <returns></returns>
        /// <exception cref="ArgumentException"></exception>
        public async Task<long> ExpireSubscriptionMessages(int maxCount)
        {
            // move to failed based on ValidUntil.

            const string ExpireStatement =
                """
                INSERT INTO [{0}].[Subscribed_Failed] WITH (ROWLOCK)
                ([Id], [SubscriberId], [Topic], [ValidUntil], [MessageId], [Priority], [NotBefore], [Enqueued], [Completed], [Failed], [Retries], [Headers], [Body])
                SELECT D.[Id], D.[SubscriberId], D.[Topic], D.[ValidUntil], D.[MessageId], D.[Priority], D.[NotBefore], D.[Enqueued], NULL, SYSUTCDATETIME(), D.[Retries], D.[Headers], D.[Body] FROM
                    (DELETE TOP (@MaxCount) FROM [{0}].[Subscribed_Pending] WITH (ROWLOCK)
                        OUTPUT DELETED.*
                        WHERE [ValidUntil] < SYSUTCDATETIME()) D
                SELECT @@ROWCOUNT
                """;

            using var _ = StartActivity(nameof(ExpireSubscriptionMessages));

            var statement = string.Format(ExpireStatement, _configuration.Schema);

            var p = new DynamicParameters();
            p.Add("@MaxCount", maxCount);

            return await LogIfError(
                _database.Connection.QueryFirstAsync<long>(statement, p, _database.Transaction));
        }

        /// <summary>
        /// Removes rows from the Subscribed Compelted Table.
        /// </summary>
        /// <param name="olderthan"></param>
        /// <param name="maxCount"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentException"></exception>
        public async Task<long> CleanSubscribedCompleted(UtcDateTime olderthan, int maxCount)
        {
            const string statementTemplate =
                """
                DELETE TOP (@MaxCount) 
                    FROM [{0}].[Subscribed_Completed] WITH (ROWLOCK, READPAST)
                    WHERE Completed < @OlderThan
                SELECT @@ROWCOUNT
                """;

            using var _ = StartActivity(nameof(CleanSubscribedFailed));

            string statement = string.Format(statementTemplate, _configuration.Schema);

            var p = new DynamicParameters();
            p.Add("@MaxCount", maxCount);
            p.Add("@OlderThan", olderthan);

            return await LogIfError(
                _database.Connection.QueryFirstAsync<long>(statement, p, _database.Transaction));
        }

        /// <summary>
        /// Removes rows from the Subscribed Failed Table
        /// </summary>
        /// <param name="olderthan"></param>
        /// <param name="maxCount"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentException"></exception>
        public async Task<long> CleanSubscribedFailed(UtcDateTime olderthan, int maxCount)
        {
            const string statementTemplate =
                """
                DELETE TOP (@MaxCount) 
                    FROM [{0}].[Subscribed_Failed] WITH (ROWLOCK, READPAST)
                    WHERE Failed < @OlderThan
                SELECT @@ROWCOUNT
                """;

            using var _ = StartActivity(nameof(CleanSubscribedFailed));

            string statement = string.Format(statementTemplate, _configuration.Schema);

            var p = new DynamicParameters();
            p.Add("@MaxCount", maxCount);
            p.Add("@OlderThan", olderthan);

            return await LogIfError(
                _database.Connection.QueryFirstAsync<long>(statement, p, _database.Transaction));
        }

        /// <summary>
        /// Removes rows from a queue's completed table.
        /// </summary>
        /// <param name="queueName"></param>
        /// <param name="olderthan"></param>
        /// <param name="maxCount"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentException"></exception>
        public async Task<long> CleanQueueCompleted(QueueName queueName, UtcDateTime olderthan, int maxCount)
        {
            const string statementTemplate =
                """
                DELETE TOP (@MaxCount) FROM [{0}].[{1}_Completed] WITH (ROWLOCK, READPAST)
                    WHERE Completed < @OlderThan
                SELECT @@ROWCOUNT
                """;

            using var _ = StartActivity(nameof(CleanQueueCompleted));

            string statement = string.Format(statementTemplate, _configuration.Schema, queueName);

            var p = new DynamicParameters();
            p.Add("@MaxCount", maxCount);
            p.Add("@OlderThan", olderthan);

            return await LogIfError(
                _database.Connection.QueryFirstAsync<long>(statement, p, _database.Transaction));
        }

        /// <summary>
        /// Removes rows from a queue's Failed Table.
        /// </summary>
        /// <param name="queueName"></param>
        /// <param name="olderthan"></param>
        /// <param name="maxCount"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentException"></exception>
        public async Task<long> CleanQueueFailed(QueueName queueName, UtcDateTime olderthan, int maxCount)
        {
            const string statementTemplate =
                """
                DELETE TOP (@MaxCount) FROM [{0}].[{1}_Failed] WITH (ROWLOCK, READPAST)
                    WHERE Failed < @OlderThan
                SELECT @@ROWCOUNT
                """;

            using var _ = StartActivity(nameof(CleanQueueFailed));

            string statement = string.Format(statementTemplate, _configuration.Schema, queueName);

            var p = new DynamicParameters();
            p.Add("@MaxCount", maxCount);
            p.Add("@OlderThan", olderthan);

            return await LogIfError(
                _database.Connection.QueryFirstAsync<long>(statement, p, _database.Transaction));
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
                _log.DapperDataAccess_DataAccessError(caller, ex);
                throw;
            }
        }

        private static Activity? StartActivity(string name) =>
            ActivitySources.DataAccess.StartActivity(
                "peachtreebus.dataaccess " + name,
                ActivityKind.Internal);

        /// <summary>
        /// Begins a database transaction.
        /// </summary>
        public void BeginTransaction()
        {
            using var _ = StartActivity(nameof(BeginTransaction));
            _database.BeginTransaction();
        }

        /// <summary>
        /// Commits the current database transaction.
        /// </summary>
        public void CommitTransaction()
        {
            using var _ = StartActivity(nameof(CommitTransaction));
            _database.CommitTransaction();
        }

        /// <summary>
        /// Creats a database save point.
        /// </summary>
        /// <param name="name"></param>
        public void CreateSavepoint(string name)
        {
            using var _ = StartActivity(nameof(CreateSavepoint));
            _database.CreateSavepoint(name);
        }

        /// <summary>
        /// Rolls back to a database save point
        /// </summary>
        /// <param name="name"></param>
        public void RollbackToSavepoint(string name)
        {
            using var _ = StartActivity(nameof(RollbackToSavepoint));
            _database.RollbackToSavepoint(name);
        }

        /// <summary>
        /// Rolls back a database transaction.
        /// </summary>
        public void RollbackTransaction()
        {
            using var _ = StartActivity(nameof(RollbackTransaction));
            _database.RollbackTransaction();
        }

        public void Reconnect()
        {
            using var _ = StartActivity(nameof(Reconnect));
            _database.Reconnect();
        }
    }
}
