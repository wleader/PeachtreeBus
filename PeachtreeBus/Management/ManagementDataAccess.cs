using Dapper;
using Microsoft.Extensions.Logging;
using PeachtreeBus.Data;
using PeachtreeBus.DatabaseSharing;
using PeachtreeBus.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PeachtreeBus.Management
{
    public class ManagementDataAccess(
        ISharedDatabase database,
        IDbSchemaConfiguration schemaConfig,
        ILogger<ManagementDataAccess> log)
        : BaseDataAccess
        , IManagementDataAccess
    {
        static ManagementDataAccess()
        {
            DateTimeHandler.AddTypeHandler();
        }

        private readonly IDbSchemaConfiguration _schemaConfig = schemaConfig;
        private readonly ISharedDatabase _database = database;
        private readonly ILogger<ManagementDataAccess> _log = log;

        protected const string TableNameUnsafe = "The table name contains not allowable characters.";

        private async Task<List<T>> GetMessages<T>(string queueName, string table, int skip, int take)
        {
            const string GetFailedMessagesStatement =
                """
                    SELECT * FROM [{0}].[{1}_{2}]
                    WITH (READPAST)
                    ORDER BY [Enqueued] DESC
                    OFFSET @Skip ROWS
                    FETCH NEXT @Take ROWS ONLY
                """;

            if (IsUnsafe(_schemaConfig.Schema))
                throw new ArgumentException(SchemaUnsafe);
            if (IsUnsafe(queueName))
                throw new ArgumentException(QueueNameUnsafe);
            if (IsUnsafe(table))
                throw new ArgumentException(TableNameUnsafe);

            string statement = string.Format(GetFailedMessagesStatement, _schemaConfig.Schema, queueName, table);

            var p = new DynamicParameters();
            p.Add("@Skip", skip);
            p.Add("@Take", take);

            try
            {
                return (await _database.Connection.QueryAsync<T>(statement, p, _database.Transaction)).ToList();
            }
            catch (Exception ex)
            {
                _log.DapperDataAccess_DataAccessError(nameof(GetMessages), ex);
                throw;
            }
        }

        public Task<List<QueueMessage>> GetFailedQueueMessages(string queueName, int skip, int take)
        {
            return GetMessages<QueueMessage>(queueName, "Failed", skip, take);
        }

        public Task<List<QueueMessage>> GetCompletedQueueMessages(string queueName, int skip, int take)
        {
            return GetMessages<QueueMessage>(queueName, "Completed", skip, take);
        }

        public Task<List<QueueMessage>> GetPendingQueueMessages(string queueName, int skip, int take)
        {
            return GetMessages<QueueMessage>(queueName, "Pending", skip, take);
        }

        public Task<List<SubscribedMessage>> GetFailedSubscribedMessages(int skip, int take)
        {
            return GetMessages<SubscribedMessage>("Subscribed", "Failed", skip, take);
        }

        public Task<List<SubscribedMessage>> GetCompletedSubscribedMessages(int skip, int take)
        {
            return GetMessages<SubscribedMessage>("Subscribed", "Completed", skip, take);
        }

        public Task<List<SubscribedMessage>> GetPendingSubscribedMessages(int skip, int take)
        {
            return GetMessages<SubscribedMessage>("Subscribed", "Pending", skip, take);
        }

        public async Task CancelPendingQueueMessage(string queueName, long id)
        {
            const string CancelPendingQueuedStatement =
                """
                DECLARE
                    @MessageId UNIQUEIDENTIFIER,
                    @NotBefore DATETIME2,
                    @Enqueued DATETIME2,
                    @Retries TINYINT,
                    @Body NVARCHAR(MAX),
                    @Headers NVARCHAR(MAX);
                SELECT 
                    @MessageId = [MessageId],
                    @Enqueued = [Enqueued],
                    @Retries = [Retries],
                    @Body = [Body],
                    @NotBefore = [NotBefore],
                    @Headers = [Headers]
                    FROM [{0}].[{1}_Pending]
                    WHERE [Id] = @Id;
                INSERT INTO [{0}].[{1}_Failed]
                    ([Id], [MessageId], [NotBefore], [Enqueued], [Completed], [Failed], [Retries], [Headers], [Body])
                    VALUES
                    (@Id, @MessageId, @NotBefore, @Enqueued, NULL, SYSUTCDATETIME(), @Retries, @Headers, @Body);
                DELETE FROM [{0}].[{1}_Pending]
                    WHERE [Id] = @Id;
                """;

            if (IsUnsafe(_schemaConfig.Schema))
                throw new ArgumentException(SchemaUnsafe);
            if (IsUnsafe(queueName))
                throw new ArgumentException(QueueNameUnsafe);

            string statement = string.Format(CancelPendingQueuedStatement, _schemaConfig.Schema, queueName);

            var p = new DynamicParameters();
            p.Add("@Id", id);

            try
            {
                await _database.Connection.ExecuteAsync(statement, p, _database.Transaction);
            }
            catch (Exception ex)
            {
                _log.DapperDataAccess_DataAccessError(nameof(CancelPendingQueueMessage), ex);
                throw;
            }
        }

        public async Task CancelPendingSubscribedMessage(long id)
        {
            const string CancelPendingSubscribedStatement =
                """
                DECLARE
                    @SubscriberId UNIQUEIDENTIFIER,
                    @ValidUntil DATETIME2,
                    @MessageId UNIQUEIDENTIFIER,
                    @NotBefore DATETIME2,
                    @Enqueued DATETIME2,
                    @Retries TINYINT,
                    @Body NVARCHAR(MAX),
                    @Headers NVARCHAR(MAX);
                SELECT 
                    @SubscriberId = [SubscriberId],
                    @ValidUntil = [ValidUntil],
                    @MessageId = [MessageId],
                    @Enqueued = [Enqueued],
                    @Retries = [Retries],
                    @Body = [Body],
                    @NotBefore = [NotBefore],
                    @Headers = [Headers]
                    FROM [{0}].[Subscribed_Pending]
                    WHERE [Id] = @Id;
                INSERT INTO [{0}].[Subscribed_Failed]
                    ([Id], [SubscriberId], [ValidUntil], [MessageId], [NotBefore], [Enqueued], [Completed], [Failed], [Retries], [Headers], [Body])
                    VALUES
                    (@Id, @SubscriberId, @ValidUntil, @MessageId, @NotBefore, @Enqueued, NULL, SYSUTCDATETIME(), @Retries, @Headers, @Body);
                DELETE FROM [{0}].[Subscribed_Pending]
                    WHERE [Id] = @Id;
                """;

            if (IsUnsafe(_schemaConfig.Schema))
                throw new ArgumentException(SchemaUnsafe);

            string statement = string.Format(CancelPendingSubscribedStatement, _schemaConfig.Schema);

            var p = new DynamicParameters();
            p.Add("@Id", id);

            try
            {
                await _database.Connection.ExecuteAsync(statement, p, _database.Transaction);
            }
            catch (Exception ex)
            {
                _log.DapperDataAccess_DataAccessError(nameof(CancelPendingSubscribedMessage), ex);
                throw;
            }
        }

        public async Task RetryFailedQueueMessage(string queueName, long id)
        {
            const string RetryFailedQueuedStatement =
                """
                DECLARE
                    @MessageId UNIQUEIDENTIFIER,
                    @NotBefore DATETIME2,
                    @Enqueued DATETIME2,
                    @Retries TINYINT,
                    @Body NVARCHAR(MAX),
                    @Headers NVARCHAR(MAX);
                SELECT 
                    @MessageId = [MessageId],
                    @Enqueued = [Enqueued],
                    @Retries = [Retries],
                    @Body = [Body],
                    @NotBefore = [NotBefore],
                    @Headers = [Headers]
                    FROM [{0}].[{1}_Failed]
                    WHERE [Id] = @Id;
                INSERT INTO [{0}].[{1}_Pending]
                    ([MessageId], [NotBefore], [Enqueued], [Completed], [Failed], [Retries], [Headers], [Body])
                    VALUES
                    (@MessageId, @NotBefore, @Enqueued, NULL, NULL, 0, @Headers, @Body);
                DELETE FROM [{0}].[{1}_Failed]
                    WHERE [Id] = @Id;
                """;

            if (IsUnsafe(_schemaConfig.Schema))
                throw new ArgumentException(SchemaUnsafe);
            if (IsUnsafe(queueName))
                throw new ArgumentException(QueueNameUnsafe);

            string statement = string.Format(RetryFailedQueuedStatement, _schemaConfig.Schema, queueName);

            var p = new DynamicParameters();
            p.Add("@Id", id);

            try
            {
                await _database.Connection.ExecuteAsync(statement, p, _database.Transaction);
            }
            catch (Exception ex)
            {
                _log.DapperDataAccess_DataAccessError(nameof(RetryFailedQueueMessage), ex);
                throw;
            }
        }

        public async Task RetryFailedSubscribedMessage(long id)
        {
            const string RetryFailedSubscribedStatement =
            """
                DECLARE
                    @SubscriberId UNIQUEIDENTIFIER,
                    @ValidUntil DATETIME2,
                    @MessageId UNIQUEIDENTIFIER,
                    @NotBefore DATETIME2,
                    @Enqueued DATETIME2,
                    @Retries TINYINT,
                    @Body NVARCHAR(MAX),
                    @Headers NVARCHAR(MAX);
                SELECT 
                    @SubscriberId = [SubscriberId],
                    @ValidUntil = [ValidUntil],
                    @MessageId = [MessageId],
                    @Enqueued = [Enqueued],
                    @Retries = [Retries],
                    @Body = [Body],
                    @NotBefore = [NotBefore],
                    @Headers = [Headers]
                    FROM [{0}].[Subscribed_Failed]
                    WHERE [Id] = @Id;
                INSERT INTO [{0}].[Subscribed_Pending]
                    ([SubscriberId], [ValidUntil], [MessageId], [NotBefore], [Enqueued], [Completed], [Failed], [Retries], [Headers], [Body])
                    VALUES
                    (@SubscriberId, @ValidUntil, @MessageId, @NotBefore, @Enqueued, NULL, NULL, 0, @Headers, @Body);
                DELETE FROM [{0}].[Subscribed_Failed]
                    WHERE [Id] = @Id;
                """;

            if (IsUnsafe(_schemaConfig.Schema))
                throw new ArgumentException(SchemaUnsafe);

            string statement = string.Format(RetryFailedSubscribedStatement, _schemaConfig.Schema);

            var p = new DynamicParameters();
            p.Add("@Id", id);

            try
            {
                await _database.Connection.ExecuteAsync(statement, p, _database.Transaction);
            }
            catch (Exception ex)
            {
                _log.DapperDataAccess_DataAccessError(nameof(RetryFailedSubscribedMessage), ex);
                throw;
            }
        }

    }
}
