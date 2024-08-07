﻿using Dapper;
using Microsoft.Extensions.Logging;
using PeachtreeBus.DatabaseSharing;
using PeachtreeBus.Model;
using System;
using System.Collections.Generic;
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
    /// <param name="schemaConfig">Configures which DB Schema to find all the tables in.</param>
    public class DapperDataAccess(
        ISharedDatabase database,
        IDbSchemaConfiguration schemaConfig,
        ILogger<DapperDataAccess> log)
        : BaseDataAccess
        , IBusDataAccess
    {
        static DapperDataAccess()
        {
            DateTimeHandler.AddTypeHandler();
        }

        private readonly ISharedDatabase _database = database;
        private readonly ILogger<DapperDataAccess> _log = log;
        private readonly IDbSchemaConfiguration _schemaConfig = schemaConfig;

        const string SagaNameUnsafe = "The saga name contains not allowable characters.";

        /// <summary>
        /// Adds a queue message to the queue's pending table.
        /// </summary>
        /// <param name="message"></param>
        /// <param name="queueName"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentException"></exception>
        /// <exception cref="ArgumentNullException"></exception>
        public async Task<long> AddMessage(QueueMessage message, string queueName)
        {
            const string EnqueueMessageStatement =
                """
                INSERT INTO [{0}].[{1}_Pending] WITH (ROWLOCK)
                ([MessageId], [NotBefore], [Enqueued], [Completed], [Failed], [Retries], [Headers], [Body])
                OUTPUT INSERTED.[Id]
                VALUES
                (@MessageId, @NotBefore, SYSUTCDATETIME(), NULL, NULL, 0, @Headers, @Body)
                """;

            if (IsUnsafe(_schemaConfig.Schema))
                throw new ArgumentException(SchemaUnsafe);
            if (IsUnsafe(queueName))
                throw new ArgumentException(QueueNameUnsafe);
            if (message == null)
                throw new ArgumentNullException(nameof(message));
            if (message.MessageId == null)
                throw new ArgumentNullException($"{nameof(message)}.{nameof(message.MessageId)}");
            if (message.NotBefore == null)
                throw new ArgumentNullException($"{nameof(message)}.{nameof(message.NotBefore)}");
            if (message.NotBefore.Kind == DateTimeKind.Unspecified)
                throw new ArgumentException($"{nameof(message)}.{nameof(message.NotBefore)}.{nameof(message.NotBefore.Kind)} cannot be unspecified.");
            if (string.IsNullOrEmpty(message.Headers))
                throw new ArgumentException($"{nameof(message)}.{nameof(message.Headers)} must be not null and not empty.");
            if (string.IsNullOrEmpty(message.Body))
                throw new ArgumentException($"{nameof(message)}.{nameof(message.Body)} must be not null and not empty.");

            string statement = string.Format(EnqueueMessageStatement, _schemaConfig.Schema, queueName);

            var p = new DynamicParameters();
            p.Add("@MessageId", message.MessageId);
            p.Add("@NotBefore", message.NotBefore.ToUniversalTime());
            p.Add("@Headers", message.Headers);
            p.Add("@Body", message.Body);

            try
            {
                return message.Id = await _database.Connection.QueryFirstAsync<long>(statement, p, _database.Transaction);
            }
            catch (Exception ex)
            {
                _log.DapperDataAccess_DataAccessError(nameof(AddMessage), ex);
                throw;
            }
        }

        /// <summary>
        /// Gets an eligble pending message from the pending table.
        /// </summary>
        /// <param name="queueName"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentException"></exception>
        public async Task<QueueMessage?> GetPendingQueued(string queueName)
        {
            // UPDLOCK makes this row unavailable to other connections and transactions.
            // READPAST to skip any rows that are locked by other connections and transactions.
            // ROWLOCK hint to tell the server to lock at the row level instead of the default page lock.
            // NotBefore so we don't get messages that are scheduled for the future.
            // Completed and Failed are null means not previously processed and not previously exceeded retry count.
            const string GetOnePendingMessageStatement =
                """
                SELECT TOP 1 [Id], [MessageId], [NotBefore], [Enqueued], [Completed], [Failed], [Retries], [Headers], [Body]
                FROM[{0}].[{1}_Pending] WITH(UPDLOCK, READPAST, ROWLOCK)
                WHERE NotBefore < SYSUTCDATETIME()
                AND Completed IS NULL 
                AND Failed IS NULL
                """;

            if (IsUnsafe(_schemaConfig.Schema))
                throw new ArgumentException(SchemaUnsafe);
            if (IsUnsafe(queueName))
                throw new ArgumentException(QueueNameUnsafe);

            var query = string.Format(GetOnePendingMessageStatement, _schemaConfig.Schema, queueName);

            try
            {
                return await _database.Connection.QueryFirstOrDefaultAsync<QueueMessage>(query, transaction: _database.Transaction);
            }
            catch (Exception ex)
            {
                _log.DapperDataAccess_DataAccessError(nameof(GetPendingQueued), ex);
                throw;
            }
        }

        /// <summary>
        /// Moves a message from the pending table to the completed table.
        /// </summary>
        /// <param name="message"></param>
        /// <param name="queueName"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentException"></exception>
        /// <exception cref="ArgumentNullException"></exception>
        public async Task CompleteMessage(QueueMessage message, string queueName)
        {
            const string CompleteMessageStatement =
                """
                INSERT INTO [{0}].[{1}_Completed] WITH (ROWLOCK) 
                ([Id], [MessageId], [NotBefore], [Enqueued], [Completed], [Failed], [Retries], [Headers], [Body])
                SELECT D.[Id], D.[MessageId], D.[NotBefore], D.[Enqueued], SYSUTCDATETIME(), NULL, D.[Retries], D.[Headers], D.[Body]
                FROM (DELETE FROM [{0}].[{1}_Pending] WITH (ROWLOCK)
                      OUTPUT DELETED.*
                      WHERE [Id] = @Id) D
                """;

            if (IsUnsafe(_schemaConfig.Schema))
                throw new ArgumentException(SchemaUnsafe);
            if (IsUnsafe(queueName))
                throw new ArgumentException(QueueNameUnsafe);
            if (message == null)
                throw new ArgumentNullException(nameof(message));
            if (string.IsNullOrEmpty(message.Headers))
                throw new ArgumentException($"{nameof(message)}.{nameof(message.Headers)} must be not null and not empty.");

            string statement = string.Format(CompleteMessageStatement, _schemaConfig.Schema, queueName);

            var p = new DynamicParameters();
            p.Add("@Id", message.Id);
            p.Add("@Headers", message.Headers);

            try
            {
                await _database.Connection.ExecuteAsync(statement, p, _database.Transaction);
            }
            catch (Exception ex)
            {
                _log.DapperDataAccess_DataAccessError(nameof(CompleteMessage), ex);
                throw;
            }
        }

        /// <summary>
        /// Moves a message from the pending table to the failed table.
        /// </summary>
        /// <param name="message"></param>
        /// <param name="queueName"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentException"></exception>
        /// <exception cref="ArgumentNullException"></exception>
        public async Task FailMessage(QueueMessage message, string queueName)
        {
            const string FailMessageStatement =
                """
                INSERT INTO [{0}].[{1}_Failed] WITH (ROWLOCK)  
                ([Id], [MessageId], [NotBefore], [Enqueued], [Completed], [Failed], [Retries], [Headers], [Body])
                SELECT D.[Id], D.[MessageId], D.[NotBefore], D.[Enqueued], NULL, SYSUTCDATETIME(), D.[Retries], @Headers, D.[Body]
                FROM (DELETE FROM [{0}].[{1}_Pending] WITH (ROWLOCK)
                      OUTPUT DELETED.*
                      WHERE [Id] = @Id) D
                """;

            if (IsUnsafe(_schemaConfig.Schema))
                throw new ArgumentException(SchemaUnsafe);
            if (IsUnsafe(queueName))
                throw new ArgumentException(QueueNameUnsafe);
            if (message == null)
                throw new ArgumentNullException(nameof(message));
            if (string.IsNullOrEmpty(message.Headers))
                throw new ArgumentException($"{nameof(message)}.{nameof(message.Headers)} must be not null and not empty.");

            string statement = string.Format(FailMessageStatement, _schemaConfig.Schema, queueName);

            var p = new DynamicParameters();
            p.Add("@Id", message.Id);
            p.Add("@Headers", message.Headers);

            try
            {
                await _database.Connection.ExecuteAsync(statement, p, _database.Transaction);
            }
            catch (Exception ex)
            {
                _log.DapperDataAccess_DataAccessError(nameof(FailMessage), ex);
                throw;
            }
        }

        /// <summary>
        /// Updates a message in the pending table
        /// </summary>
        /// <param name="message"></param>
        /// <param name="queueName"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentException"></exception>
        /// <exception cref="ArgumentNullException"></exception>
        public async Task Update(QueueMessage message, string queueName)
        {
            const string UpdateMessageStatement =
                """
                UPDATE [{0}].[{1}_Pending] WITH (ROWLOCK) SET
                [NotBefore] = @NotBefore,
                [Retries] = @Retries,
                [Headers] = @Headers
                WHERE [Id] = @Id
                """;

            if (IsUnsafe(_schemaConfig.Schema))
                throw new ArgumentException(SchemaUnsafe);
            if (IsUnsafe(queueName))
                throw new ArgumentException(QueueNameUnsafe);
            if (message == null)
                throw new ArgumentNullException(nameof(message));
            if (message.NotBefore == null)
                throw new ArgumentNullException($"{nameof(message)}.{nameof(message.NotBefore)}");
            if (message.NotBefore.Kind == DateTimeKind.Unspecified)
                throw new ArgumentException($"{nameof(message)}.{nameof(message.NotBefore)}.{nameof(message.NotBefore.Kind)} cannot be unspecified.");
            if (string.IsNullOrEmpty(message.Headers))
                throw new ArgumentException($"{nameof(message)}.{nameof(message.Headers)} must be not null and not empty.");

            var statement = string.Format(UpdateMessageStatement, _schemaConfig.Schema, queueName);

            var p = new DynamicParameters();
            p.Add("@Id", message.Id);
            p.Add("@NotBefore", message.NotBefore.ToUniversalTime());
            p.Add("@Retries", message.Retries);
            p.Add("@Headers", message.Headers);

            try
            {
                await _database.Connection.ExecuteAsync(statement, p, _database.Transaction);
            }
            catch (Exception ex)
            {
                _log.DapperDataAccess_DataAccessError(nameof(Update), ex);
                throw;
            }
        }

        /// <summary>
        /// Inserts a row into a saga data table.
        /// </summary>
        /// <param name="data"></param>
        /// <param name="sagaName"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentException"></exception>
        /// <exception cref="ArgumentNullException"></exception>
        public async Task<long> Insert(SagaData data, string sagaName)
        {
            const string InsertSagaStatement =
                """
                INSERT INTO[{0}].[{1}_SagaData] WITH (ROWLOCK)
                ([SagaId], [Key], [Data])
                OUTPUT INSERTED.[Id]
                VALUES
                (@SagaId, @Key, @Data)
                """;

            if (IsUnsafe(_schemaConfig.Schema))
                throw new ArgumentException(SchemaUnsafe);
            if (IsUnsafe(sagaName))
                throw new ArgumentException(SagaNameUnsafe);
            if (data == null)
                throw new ArgumentNullException(nameof(data));
            if (data.SagaId == null)
                throw new ArgumentNullException($"{nameof(data)}.{nameof(data.SagaId)}");
            if (string.IsNullOrEmpty(data.Key))
                throw new ArgumentException($"{nameof(data)}.{nameof(data.Key)} must be not null and not empty.");
            if (string.IsNullOrEmpty(data.Data))
                throw new ArgumentException($"{nameof(data)}.{nameof(data.Data)} must be not null and not empty.");

            string statement = string.Format(InsertSagaStatement, _schemaConfig.Schema, sagaName);

            var p = new DynamicParameters();
            p.Add("@SagaId", data.SagaId);
            p.Add("@Key", data.Key);
            p.Add("@Data", data.Data);

            try
            {
                return await _database.Connection.QueryFirstAsync<long>(statement, p, _database.Transaction);
            }
            catch (Exception ex)
            {
                _log.DapperDataAccess_DataAccessError(nameof(Insert), ex);
                throw;
            }
        }

        /// <summary>
        /// Updates a row in the saga data table.
        /// </summary>
        /// <param name="data"></param>
        /// <param name="sagaName"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentException"></exception>
        /// <exception cref="ArgumentNullException"></exception>
        public async Task Update(SagaData data, string sagaName)
        {
            const string UpdateSagaStatement =
                """
                UPDATE [{0}].[{1}_SagaData] WITH (ROWLOCK) SET
                [Data] = @Data
                WHERE [Id] = @Id
                """;

            if (IsUnsafe(_schemaConfig.Schema))
                throw new ArgumentException(SchemaUnsafe);
            if (IsUnsafe(sagaName))
                throw new ArgumentException(SagaNameUnsafe);
            if (data == null)
                throw new ArgumentNullException(nameof(data));
            if (string.IsNullOrEmpty(data.Data))
                throw new ArgumentException($"{nameof(data)}.{nameof(data.Data)} must be not null and not empty.");

            var statement = string.Format(UpdateSagaStatement, _schemaConfig.Schema, sagaName);

            var p = new DynamicParameters();
            p.Add("@Id", data.Id);
            p.Add("@Data", data.Data);

            try
            {
                await _database.Connection.ExecuteAsync(statement, p, _database.Transaction);
            }
            catch (Exception ex)
            {
                _log.DapperDataAccess_DataAccessError(nameof(Update), ex);
                throw;
            }
        }

        /// <summary>
        /// Deletes a row from a saga data table.
        /// </summary>
        /// <param name="sagaName"></param>
        /// <param name="key"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentException"></exception>
        public async Task DeleteSagaData(string sagaName, string key)
        {
            const string DeleteSagaStatement =
                """
                DELETE FROM [{0}].[{1}_SagaData] WITH (ROWLOCK)
                WHERE [Key] = @Key
                """;

            if (IsUnsafe(_schemaConfig.Schema))
                throw new ArgumentException(SchemaUnsafe);
            if (IsUnsafe(sagaName))
                throw new ArgumentException(SagaNameUnsafe);
            if (string.IsNullOrEmpty(key))
                throw new ArgumentException($"{nameof(key)} must be not null and not empty.");

            string statement = string.Format(DeleteSagaStatement, _schemaConfig.Schema, sagaName);
            var p = new DynamicParameters();
            p.Add("@Key", key);

            try
            {
                await _database.Connection.ExecuteAsync(statement, p, _database.Transaction);
            }
            catch (Exception ex)
            {
                _log.DapperDataAccess_DataAccessError(nameof(DeleteSagaData), ex);
                throw;
            }
        }

        /// <summary>
        /// Reads a row from a saga data table.
        /// </summary>
        /// <param name="sagaName"></param>
        /// <param name="key"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentException"></exception>
        public async Task<SagaData?> GetSagaData(string sagaName, string key)
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
                    SELECT -1 as [Id],
                           CONVERT(uniqueidentifier, '00000000-0000-0000-0000-000000000000') as [SagaId],
                           @Key as [Key],
                           '' as [Data],
                           1 as [Blocked]
                END CATCH
                """;

            if (IsUnsafe(_schemaConfig.Schema))
                throw new ArgumentException(SchemaUnsafe);
            if (IsUnsafe(sagaName))
                throw new ArgumentException(SagaNameUnsafe);
            if (string.IsNullOrEmpty(key))
                throw new ArgumentException($"{nameof(key)} must be not null and not empty.");

            var query = string.Format(GetSagaDataStatement, _schemaConfig.Schema, sagaName);

            var p = new DynamicParameters();
            p.Add("@Key", key);

            try
            {
                return await _database.Connection.QueryFirstOrDefaultAsync<SagaData>(query, p, _database.Transaction);
            }
            catch (Exception ex)
            {
                _log.DapperDataAccess_DataAccessError(nameof(GetSagaData), ex);
                throw;
            }
        }

        /// <summary>
        /// Deletes expired rows from the subscriptions table.
        /// </summary>
        /// <returns></returns>
        /// <exception cref="ArgumentException"></exception>
        public async Task ExpireSubscriptions()
        {
            const string ExpireSubscriptionsStatement =
                """
                DELETE FROM [{0}].[Subscriptions] WITH (ROWLOCK, READPAST)
                WHERE [ValidUntil] < SYSUTCDATETIME()
                """;

            if (IsUnsafe(_schemaConfig.Schema))
                throw new ArgumentException(SchemaUnsafe);

            string statement = string.Format(ExpireSubscriptionsStatement, _schemaConfig.Schema);

            try
            {
                await _database.Connection.ExecuteAsync(statement, null, _database.Transaction);
            }
            catch (Exception ex)
            {
                _log.DapperDataAccess_DataAccessError(nameof(ExpireSubscriptions), ex);
                throw;
            }
        }

        /// <summary>
        /// Adds or updates a row in the subscriptions table.
        /// </summary>
        /// <param name="subscriberId"></param>
        /// <param name="category"></param>
        /// <param name="until"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentException"></exception>
        public async Task Subscribe(Guid subscriberId, string category, DateTime until)
        {
            const string SubscribeStatement =
                """
                UPDATE [{0}].[Subscriptions] WITH (UPDLOCK, SERIALIZABLE)
                    SET [ValidUntil] = @ValidUntil
                    WHERE [SubscriberId] = @SubscriberId
                    AND [Category] = @Category
                IF @@ROWCOUNT = 0
                BEGIN
                    INSERT INTO [{0}].[Subscriptions] WITH (ROWLOCK)
                    ([SubscriberId], [Category], [ValidUntil])
                    VALUES
                    (@SubscriberId, @Category, @ValidUntil)
                END
                """;

            if (IsUnsafe(_schemaConfig.Schema))
                throw new ArgumentException(SchemaUnsafe);

            if (subscriberId == Guid.Empty)
                throw new ArgumentException($"{nameof(subscriberId)} must not be Guid.Empty");

            string statement = string.Format(SubscribeStatement, _schemaConfig.Schema);

            var p = new DynamicParameters();
            p.Add("@SubscriberId", subscriberId);
            p.Add("@Category", category);
            p.Add("@ValidUntil", until);

            try
            {
                await _database.Connection.ExecuteAsync(statement, p, _database.Transaction);
            }
            catch (Exception ex)
            {
                _log.DapperDataAccess_DataAccessError(nameof(Subscribe), ex);
                throw;
            }
        }

        /// <summary>
        /// Gets an eligible pending subscribed message.
        /// </summary>
        /// <param name="subscriberId"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentException"></exception>
        public async Task<SubscribedMessage?> GetPendingSubscribed(Guid subscriberId)
        {
            // UPDLOCK makes this row unavailable to other connections and transactions.
            // READPAST to skip any rows that are locked by other connections and transactions.
            // ROWLOCK hint to tell the server to lock at the row level instead of the default page lock.
            // NotBefore so we don't get messages that are scheduled for the future.
            // Completed and Failed are null means not previously processed and not previously exceeded retry count.
            const string statement =
                """
                SELECT TOP 1 [Id], [SubscriberId], [ValidUntil], [MessageId], [NotBefore], [Enqueued], [Completed], [Failed], [Retries], [Headers], [Body]
                    FROM[{0}].[Subscribed_Pending] WITH(UPDLOCK, READPAST, ROWLOCK)
                    WHERE NotBefore < SYSUTCDATETIME()
                    AND SubscriberId = @SubscriberId
                    AND Completed IS NULL
                    AND Failed IS NULL
                """;

            if (IsUnsafe(_schemaConfig.Schema))
                throw new ArgumentException(SchemaUnsafe);

            if (subscriberId == Guid.Empty)
                throw new ArgumentException($"{nameof(subscriberId)} must not be Guid.Empty");

            var query = string.Format(statement, _schemaConfig.Schema);

            var p = new DynamicParameters();
            p.Add("@SubscriberId", subscriberId);

            try
            {
                return await _database.Connection.QueryFirstOrDefaultAsync<SubscribedMessage>(query, p, _database.Transaction);
            }
            catch (Exception ex)
            {
                _log.DapperDataAccess_DataAccessError(nameof(GetPendingSubscribed), ex);
                throw;
            }
        }

        /// <summary>
        /// Stores a subscribed message in the pending table.
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentException"></exception>
        /// <exception cref="ArgumentNullException"></exception>
        public async Task<long> AddMessage(SubscribedMessage message)
        {
            const string EnqueueMessageStatement =
                """
                INSERT INTO [{0}].[Subscribed_Pending] WITH (ROWLOCK)
                ([SubscriberId], [ValidUntil], [MessageId], [NotBefore], [Enqueued], [Completed], [Failed], [Retries], [Headers], [Body])
                OUTPUT INSERTED.[Id]
                VALUES
                (@SubscriberId, @ValidUntil, @MessageId,@NotBefore, SYSUTCDATETIME(), NULL, NULL, 0, @Headers, @Body)
                """;

            if (IsUnsafe(_schemaConfig.Schema))
                throw new ArgumentException(SchemaUnsafe);
            if (message == null)
                throw new ArgumentNullException(nameof(message));
            if (message.MessageId == null)
                throw new ArgumentNullException($"{nameof(message)}.{nameof(message.MessageId)}");
            if (message.NotBefore == null)
                throw new ArgumentNullException($"{nameof(message)}.{nameof(message.NotBefore)}");
            if (message.NotBefore.Kind == DateTimeKind.Unspecified)
                throw new ArgumentException($"{nameof(message)}.{nameof(message.NotBefore)}.{nameof(message.NotBefore.Kind)} cannot be unspecified.");
            if (message.ValidUntil == null)
                throw new ArgumentNullException($"{nameof(message)}.{nameof(message.ValidUntil)}");
            if (message.SubscriberId == Guid.Empty)
                throw new ArgumentException($"{nameof(message)}.{nameof(message.SubscriberId)} must not be Guid.Empty");
            if (message.ValidUntil.Kind == DateTimeKind.Unspecified)
                throw new ArgumentException($"{nameof(message)}.{nameof(message.ValidUntil)}.{nameof(message.ValidUntil.Kind)} cannot be unspecified.");
            if (string.IsNullOrEmpty(message.Headers))
                throw new ArgumentException($"{nameof(message)}.{nameof(message.Headers)} must be not null and not empty.");
            if (string.IsNullOrEmpty(message.Body))
                throw new ArgumentException($"{nameof(message)}.{nameof(message.Body)} must be not null and not empty.");

            string statement = string.Format(EnqueueMessageStatement, _schemaConfig.Schema);

            var p = new DynamicParameters();
            p.Add("@MessageId", message.MessageId);
            p.Add("@SubscriberId", message.SubscriberId);
            p.Add("@ValidUntil", message.ValidUntil.ToUniversalTime());
            p.Add("@NotBefore", message.NotBefore.ToUniversalTime());
            p.Add("@Headers", message.Headers);
            p.Add("@Body", message.Body);

            try
            {
                return message.Id = await _database.Connection.QueryFirstAsync<long>(statement, p, _database.Transaction);
            }
            catch (Exception ex)
            {
                _log.DapperDataAccess_DataAccessError(nameof(AddMessage), ex);
                throw;
            }
        }

        /// <summary>
        /// Moves a subscribed message from the pending table to the compelted table.
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentException"></exception>
        /// <exception cref="ArgumentNullException"></exception>
        public async Task CompleteMessage(SubscribedMessage message)
        {
            const string completeStatement =
                """
                INSERT INTO [{0}].[Subscribed_Completed] WITH (ROWLOCK)
                ([Id], [SubscriberId], [ValidUntil], [MessageId], [NotBefore], [Enqueued], [Completed], [Failed], [Retries], [Headers], [Body])
                SELECT D.[Id], D.[SubscriberId], D.[ValidUntil], D.[MessageId], D.[NotBefore], D.[Enqueued], SYSUTCDATETIME(), NULL, D.[Retries], D.[Headers], D.[Body] FROM
                    (DELETE FROM [{0}].[Subscribed_Pending] WITH (ROWLOCK)
                        OUTPUT DELETED.*
                        WHERE [Id] = @Id) D
                """;

            if (IsUnsafe(_schemaConfig.Schema))
                throw new ArgumentException(SchemaUnsafe);
            if (message == null)
                throw new ArgumentNullException(nameof(message));
            if (string.IsNullOrEmpty(message.Headers))
                throw new ArgumentException($"{nameof(message)}.{nameof(message.Headers)} must be not null and not empty.");

            string statement = string.Format(completeStatement, _schemaConfig.Schema);

            var p = new DynamicParameters();
            p.Add("@Id", message.Id);
            p.Add("@Headers", message.Headers);

            try
            {
                await _database.Connection.ExecuteAsync(statement, p, _database.Transaction);
            }
            catch (Exception ex)
            {
                _log.DapperDataAccess_DataAccessError(nameof(CompleteMessage), ex);
                throw;
            }
        }

        /// <summary>
        /// Moves a subscribed message from the pending table to the failed table.
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentException"></exception>
        /// <exception cref="ArgumentNullException"></exception>
        public async Task FailMessage(SubscribedMessage message)
        {
            const string FailMessageStatement =
                """
                INSERT INTO [{0}].[Subscribed_Failed] WITH (ROWLOCK)  
                ([Id], [SubscriberId], [ValidUntil], [MessageId], [NotBefore], [Enqueued], [Completed], [Failed], [Retries], [Headers], [Body])
                SELECT D.[Id], D.[SubscriberId], D.[ValidUntil], D.[MessageId], D.[NotBefore], D.[Enqueued], NULL, SYSUTCDATETIME(), D.[Retries], @Headers, D.[Body] FROM
                    (DELETE FROM [{0}].[Subscribed_Pending] WITH (ROWLOCK)
                        OUTPUT DELETED.*
                        WHERE [Id] = @Id) D
                """;

            if (IsUnsafe(_schemaConfig.Schema))
                throw new ArgumentException(SchemaUnsafe);
            if (message == null)
                throw new ArgumentNullException(nameof(message));
            if (string.IsNullOrEmpty(message.Headers))
                throw new ArgumentException($"{nameof(message)}.{nameof(message.Headers)} must be not null and not empty.");

            string statement = string.Format(FailMessageStatement, _schemaConfig.Schema);

            var p = new DynamicParameters();
            p.Add("@Id", message.Id);
            p.Add("@Headers", message.Headers);

            try
            {
                await _database.Connection.ExecuteAsync(statement, p, _database.Transaction);
            }
            catch (Exception ex)
            {
                _log.DapperDataAccess_DataAccessError(nameof(FailMessage), ex);
                throw;
            }
        }

        /// <summary>
        /// Updates a subscribed message in the pending table
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentException"></exception>
        /// <exception cref="ArgumentNullException"></exception>
        public async Task Update(SubscribedMessage message)
        {
            const string UpdateMessageStatement =
                """
                UPDATE[{0}].[Subscribed_Pending] WITH (ROWLOCK)
                SET [NotBefore] = @NotBefore,
                    [Retries] = @Retries,
                    [Headers] = @Headers
                WHERE [Id] = @Id
                """;

            if (IsUnsafe(_schemaConfig.Schema))
                throw new ArgumentException(SchemaUnsafe);
            if (message == null)
                throw new ArgumentNullException(nameof(message));
            if (message.NotBefore == null)
                throw new ArgumentNullException($"{nameof(message)}.{nameof(message.NotBefore)}");
            if (message.NotBefore.Kind == DateTimeKind.Unspecified)
                throw new ArgumentException($"{nameof(message)}.{nameof(message.NotBefore)}.{nameof(message.NotBefore.Kind)} cannot be unspecified.");
            if (string.IsNullOrEmpty(message.Headers))
                throw new ArgumentException($"{nameof(message)}.{nameof(message.Headers)} must be not null and not empty.");

            var statement = string.Format(UpdateMessageStatement, _schemaConfig.Schema);

            var p = new DynamicParameters();
            p.Add("@Id", message.Id);
            p.Add("@NotBefore", message.NotBefore.ToUniversalTime());
            p.Add("@Retries", message.Retries);
            p.Add("@Headers", message.Headers);

            try
            {
                await _database.Connection.ExecuteAsync(statement, p, _database.Transaction);
            }
            catch (Exception ex)
            {
                _log.DapperDataAccess_DataAccessError(nameof(Update), ex);
                throw;
            }
        }

        /// <summary>
        /// Removes subscribed messages that have expired (Time is after Valid Until)
        /// </summary>
        /// <returns></returns>
        /// <exception cref="ArgumentException"></exception>
        public async Task ExpireSubscriptionMessages()
        {
            // move to failed based on ValidUntil.

            const string ExpireStatement =
                """
                INSERT INTO [{0}].[Subscribed_Failed] WITH (ROWLOCK)
                ([Id], [SubscriberId], [ValidUntil], [MessageId], [NotBefore], [Enqueued], [Completed], [Failed], [Retries], [Headers], [Body])
                SELECT D.[Id], D.[SubscriberId], D.[ValidUntil], D.[MessageId], D.[NotBefore], D.[Enqueued], NULL, SYSUTCDATETIME(), D.[Retries], D.[Headers], D.[Body] FROM
                    (DELETE FROM [{0}].[Subscribed_Pending] WITH (ROWLOCK)
                        OUTPUT DELETED.*
                        WHERE [ValidUntil] < SYSUTCDATETIME()) D
                """;

            if (IsUnsafe(_schemaConfig.Schema))
                throw new ArgumentException(SchemaUnsafe);

            var statement = string.Format(ExpireStatement, _schemaConfig.Schema);

            try
            {
                await _database.Connection.ExecuteAsync(statement, null, _database.Transaction);
            }
            catch (Exception ex)
            {
                _log.DapperDataAccess_DataAccessError(nameof(ExpireSubscriptionMessages), ex);
                throw;
            }
        }

        /// <summary>
        /// Gets a list of subscribers for a subscriptin category.
        /// </summary>
        /// <param name="category"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentException"></exception>
        public async Task<IEnumerable<Guid>> GetSubscribers(string category)
        {
            const string GetStatement =
                """
                SELECT [SubscriberId]
                    FROM [{0}].[Subscriptions] WITH (READPAST)
                    WHERE [Category] = @Category
                    AND [ValidUntil] > SYSUTCDATETIME()
                """;

            if (IsUnsafe(_schemaConfig.Schema))
                throw new ArgumentException(SchemaUnsafe);

            var statement = string.Format(GetStatement, _schemaConfig.Schema);

            var p = new DynamicParameters();
            p.Add("@Category", category);

            try
            {
                return await _database.Connection.QueryAsync<Guid>(statement, p, _database.Transaction);
            }
            catch (Exception ex)
            {
                _log.DapperDataAccess_DataAccessError(nameof(GetSubscribers), ex);
                throw;
            }
        }

        /// <summary>
        /// Removes rows from the Subscribed Compelted Table.
        /// </summary>
        /// <param name="olderthan"></param>
        /// <param name="maxCount"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentException"></exception>
        public async Task<long> CleanSubscribedCompleted(DateTime olderthan, int maxCount)
        {
            const string statementTemplate =
                """
                DELETE TOP (@MaxCount) 
                    FROM [{0}].[Subscribed_Completed] WITH (ROWLOCK, READPAST)
                    WHERE Completed < @OlderThan
                SELECT @@ROWCOUNT
                """;

            if (IsUnsafe(_schemaConfig.Schema))
                throw new ArgumentException(SchemaUnsafe);

            string statement = string.Format(statementTemplate, _schemaConfig.Schema);

            var p = new DynamicParameters();
            p.Add("@MaxCount", maxCount);
            p.Add("@OlderThan", olderthan);

            try
            {
                return await _database.Connection.QueryFirstAsync<long>(statement, p, _database.Transaction);
            }
            catch (Exception ex)
            {
                _log.DapperDataAccess_DataAccessError(nameof(CleanSubscribedCompleted), ex);
                throw;
            }
        }

        /// <summary>
        /// Removes rows from the Subscribed Failed Table
        /// </summary>
        /// <param name="olderthan"></param>
        /// <param name="maxCount"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentException"></exception>
        public async Task<long> CleanSubscribedFailed(DateTime olderthan, int maxCount)
        {
            const string statementTemplate =
                """
                DELETE TOP (@MaxCount) 
                    FROM [{0}].[Subscribed_Failed] WITH (ROWLOCK, READPAST)
                    WHERE Failed < @OlderThan
                SELECT @@ROWCOUNT
                """;

            if (IsUnsafe(_schemaConfig.Schema))
                throw new ArgumentException(SchemaUnsafe);

            string statement = string.Format(statementTemplate, _schemaConfig.Schema);

            var p = new DynamicParameters();
            p.Add("@MaxCount", maxCount);
            p.Add("@OlderThan", olderthan);

            try
            {
                return await _database.Connection.QueryFirstAsync<long>(statement, p, _database.Transaction);
            }
            catch (Exception ex)
            {
                _log.DapperDataAccess_DataAccessError(nameof(CleanSubscribedFailed), ex);
                throw;
            }
        }

        /// <summary>
        /// Removes rows from a queue's completed table.
        /// </summary>
        /// <param name="queueName"></param>
        /// <param name="olderthan"></param>
        /// <param name="maxCount"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentException"></exception>
        public async Task<long> CleanQueueCompleted(string queueName, DateTime olderthan, int maxCount)
        {
            const string statementTemplate =
                """
                DELETE TOP (@MaxCount) FROM [{0}].[{1}_Completed] WITH (ROWLOCK, READPAST)
                    WHERE Completed < @OlderThan
                SELECT @@ROWCOUNT
                """;

            if (IsUnsafe(_schemaConfig.Schema))
                throw new ArgumentException(SchemaUnsafe);
            if (IsUnsafe(queueName))
                throw new ArgumentException(QueueNameUnsafe);

            string statement = string.Format(statementTemplate, _schemaConfig.Schema, queueName);

            var p = new DynamicParameters();
            p.Add("@MaxCount", maxCount);
            p.Add("@OlderThan", olderthan);

            try
            {
                return await _database.Connection.QueryFirstAsync<long>(statement, p, _database.Transaction);
            }
            catch (Exception ex)
            {
                _log.DapperDataAccess_DataAccessError(nameof(CleanQueueCompleted), ex);
                throw;
            }
        }

        /// <summary>
        /// Removes rows from a queue's Failed Table.
        /// </summary>
        /// <param name="queueName"></param>
        /// <param name="olderthan"></param>
        /// <param name="maxCount"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentException"></exception>
        public async Task<long> CleanQueueFailed(string queueName, DateTime olderthan, int maxCount)
        {
            const string statementTemplate =
                """
                DELETE TOP (@MaxCount) FROM [{0}].[{1}_Failed] WITH (ROWLOCK, READPAST)
                    WHERE Failed < @OlderThan
                SELECT @@ROWCOUNT
                """;

            if (IsUnsafe(_schemaConfig.Schema))
                throw new ArgumentException(SchemaUnsafe);
            if (IsUnsafe(queueName))
                throw new ArgumentException(QueueNameUnsafe);

            string statement = string.Format(statementTemplate, _schemaConfig.Schema, queueName);

            var p = new DynamicParameters();
            p.Add("@MaxCount", maxCount);
            p.Add("@OlderThan", olderthan);

            try
            {
                return await _database.Connection.QueryFirstAsync<long>(statement, p, _database.Transaction);
            }
            catch (Exception ex)
            {
                _log.DapperDataAccess_DataAccessError(nameof(CleanQueueFailed), ex);
                throw;
            }
        }

        /// <summary>
        /// Begins a database transaction.
        /// </summary>
        public void BeginTransaction()
        {
            _database.BeginTransaction();
        }

        /// <summary>
        /// Commits the current database transaction.
        /// </summary>
        public void CommitTransaction()
        {
            _database.CommitTransaction();
        }

        /// <summary>
        /// Creats a database save point.
        /// </summary>
        /// <param name="name"></param>
        public void CreateSavepoint(string name)
        {
            _database.CreateSavepoint(name);
        }

        /// <summary>
        /// Rolls back to a database save point
        /// </summary>
        /// <param name="name"></param>
        public void RollbackToSavepoint(string name)
        {
            _database.RollbackToSavepoint(name);
        }

        /// <summary>
        /// Rolls back a database transaction.
        /// </summary>
        public void RollbackTransaction()
        {
            _database.RollbackTransaction();
        }

        public void Reconnect()
        {
            _database.Reconnect();
        }
    }
}
