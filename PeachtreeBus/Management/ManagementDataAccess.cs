using Dapper;
using Microsoft.Extensions.Logging;
using PeachtreeBus.Data;
using PeachtreeBus.DatabaseSharing;
using PeachtreeBus.Queues;
using PeachtreeBus.Subscriptions;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;

namespace PeachtreeBus.Management
{
    public class ManagementDataAccess(
        ISharedDatabase database,
        IDbSchemaConfiguration schemaConfig,
        ILogger<ManagementDataAccess> log)
        : IManagementDataAccess
    {
        static ManagementDataAccess()
        {
            UtcDateTimeHandler.AddTypeHandler();

            typeFields = new(new Dictionary<Type, string>()
            {
                {typeof(QueueMessage), "[Id], [MessageId], [Priority], [NotBefore], [Enqueued], [Completed], [Failed], [Retries], [Headers], [Body]"},
                {typeof(SubscribedMessage), "[Id], [SubscriberId], [ValidUntil], [MessageId], [Priority], [NotBefore], [Enqueued], [Completed], [Failed], [Retries], [Headers], [Body]"},
            });
        }

        private readonly IDbSchemaConfiguration _schemaConfig = schemaConfig;
        private readonly ISharedDatabase _database = database;
        private readonly ILogger<ManagementDataAccess> _log = log;

        private static readonly TableName Failed = new("Failed");
        private static readonly TableName Completed = new("Completed");
        private static readonly TableName Pending = new("Pending");
        private static readonly QueueName Subscribed = new("Subscribed");

        private static readonly ReadOnlyDictionary<Type, string> typeFields;

        private readonly record struct TableName
        {
            public string Value { get; }
            public TableName(string value)
            {
                DbSafeNameException.ThrowIfNotSafe(value, nameof(TableName));
                Value = value;
            }

            public override string ToString() => Value ?? throw new DbSafeNameException($"{nameof(TableName)} is not initialized.");
        }


        private async Task<List<T>> GetMessages<T>(QueueName queueName, TableName table, int skip, int take)
        {
            const string template =
                """
                    SELECT {3} FROM [{0}].[{1}_{2}]
                    WITH (READPAST)
                    ORDER BY [Enqueued] DESC
                    OFFSET @Skip ROWS
                    FETCH NEXT @Take ROWS ONLY
                """;

            if (!typeFields.TryGetValue(typeof(T), out string fields))
                throw new ArgumentException($"Type not supported{typeof(T)}");

            string statement = string.Format(template, _schemaConfig.Schema, queueName, table, fields);

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

        public Task<List<QueueMessage>> GetFailedQueueMessages(QueueName queueName, int skip, int take)
        {
            return GetMessages<QueueMessage>(queueName, Failed, skip, take);
        }

        public Task<List<QueueMessage>> GetCompletedQueueMessages(QueueName queueName, int skip, int take)
        {
            return GetMessages<QueueMessage>(queueName, Completed, skip, take);
        }

        public Task<List<QueueMessage>> GetPendingQueueMessages(QueueName queueName, int skip, int take)
        {
            return GetMessages<QueueMessage>(queueName, Pending, skip, take);
        }

        public Task<List<SubscribedMessage>> GetFailedSubscribedMessages(int skip, int take)
        {
            return GetMessages<SubscribedMessage>(Subscribed, Failed, skip, take);
        }

        public Task<List<SubscribedMessage>> GetCompletedSubscribedMessages(int skip, int take)
        {
            return GetMessages<SubscribedMessage>(Subscribed, Completed, skip, take);
        }

        public Task<List<SubscribedMessage>> GetPendingSubscribedMessages(int skip, int take)
        {
            return GetMessages<SubscribedMessage>(Subscribed, Pending, skip, take);
        }

        public async Task CancelPendingQueueMessage(QueueName queueName, long id)
        {
            const string CancelPendingQueuedStatement =
                """
                INSERT INTO [{0}].[{1}_Failed] WITH (ROWLOCK)
                ([Id], [MessageId], [Priority], [NotBefore], [Enqueued], [Completed], [Failed], [Retries], [Headers], [Body])
                SELECT D.[Id], D.[MessageId], D.[Priority], D.[NotBefore], D.[Enqueued], NULL, SYSUTCDATETIME(), D.[Retries], D.[Headers], D.[Body] FROM
                    (DELETE FROM [{0}].[{1}_Pending] WITH (ROWLOCK)
                        OUTPUT DELETED.*
                        WHERE [Id] = @Id) D
                """;

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
                INSERT INTO [{0}].[Subscribed_Failed] WITH (ROWLOCK) 
                ([Id], [SubscriberId], [ValidUntil], [MessageId], [Priority], [NotBefore], [Enqueued], [Completed], [Failed], [Retries], [Headers], [Body])
                SELECT D.[Id], D.[SubscriberId], D.[ValidUntil], D.[MessageId], D.[Priority], D.[NotBefore], D.[Enqueued], NULL, SYSUTCDATETIME(), D.[Retries], D.[Headers], D.[Body] FROM
                    (DELETE FROM [{0}].[Subscribed_Pending] WITH (ROWLOCK)
                        OUTPUT DELETED.*
                        WHERE [Id] = @Id) D
                """;

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

        public async Task RetryFailedQueueMessage(QueueName queueName, long id)
        {
            const string RetryFailedQueuedStatement =
                """
                INSERT INTO [{0}].[{1}_Pending] WITH (ROWLOCK)
                ([MessageId], [Priority], [NotBefore], [Enqueued], [Completed], [Failed], [Retries], [Headers], [Body])
                SELECT D.[MessageId], D.[Priority], D.[NotBefore], D.[Enqueued], NULL, NULL, 0, D.[Headers], D.[Body] FROM
                    (DELETE FROM [{0}].[{1}_Failed] WITH (ROWLOCK)
                        OUTPUT DELETED.*
                        WHERE [Id] = @Id) D
                """;

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
                INSERT INTO [{0}].[Subscribed_Pending] WITH (ROWLOCK) 
                ([SubscriberId], [ValidUntil], [MessageId], [Priority], [NotBefore], [Enqueued], [Completed], [Failed], [Retries], [Headers], [Body])
                SELECT D.[SubscriberId], D.[ValidUntil], D.[MessageId], D.[Priority], D.[NotBefore], D.[Enqueued], NULL, NULL, 0, D.[Headers], D.[Body] FROM
                    (DELETE FROM [{0}].[Subscribed_Failed] WITH (ROWLOCK)
                        OUTPUT DELETED.*
                        WHERE [Id] = @Id) D
                """;

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
