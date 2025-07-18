﻿using Dapper;
using Microsoft.Extensions.Logging;
using PeachtreeBus.Data;
using PeachtreeBus.Queues;
using PeachtreeBus.Subscriptions;
using PeachtreeBus.Telemetry;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace PeachtreeBus.Management
{
    public class ManagementDataAccess(
        IBusConfiguration configuration,
        ILogger<ManagementDataAccess> log,
        IDapperMethods dapper)
        : IManagementDataAccess
    {
        private readonly IBusConfiguration _configuration = configuration;
        private readonly ILogger<ManagementDataAccess> _log = log;

        private const string QueueFields = "[Id], [MessageId], [Priority], [NotBefore], [Enqueued], [Completed], [Failed], [Retries], [Headers], [Body]";
        private const string SubscribedFields = "[Id], [SubscriberId], [ValidUntil], [MessageId], [Priority], [NotBefore], [Enqueued], [Completed], [Failed], [Retries], [Headers], [Body]";

        private static readonly TableName Failed = new("Failed");
        private static readonly TableName Completed = new("Completed");
        private static readonly TableName Pending = new("Pending");
        private static readonly QueueName Subscribed = new("Subscribed");

        private async Task<List<T>> GetMessages<T>(string fields, QueueName queueName, TableName table, int skip, int take)
        {
            const string template =
                """
                SELECT {3} FROM [{0}].[{1}_{2}]
                WITH (READPAST)
                ORDER BY [Enqueued] DESC
                OFFSET @Skip ROWS
                FETCH NEXT @Take ROWS ONLY
                """;

            string statement = string.Format(template, _configuration.Schema, queueName, table, fields);

            var p = new DynamicParameters();
            p.Add("@Skip", skip);
            p.Add("@Take", take);

            return [.. (await LogIfError(dapper.Query<T>(statement, p)))];
        }

        public async Task<List<QueueData>> GetFailedQueueMessages(QueueName queueName, int skip, int take)
        {
            using var _ = StartActivity();
            return await GetMessages<QueueData>(QueueFields, queueName, Failed, skip, take);
        }

        public async Task<List<QueueData>> GetCompletedQueueMessages(QueueName queueName, int skip, int take)
        {
            using var _ = StartActivity();
            return await GetMessages<QueueData>(QueueFields, queueName, Completed, skip, take);
        }

        public async Task<List<QueueData>> GetPendingQueueMessages(QueueName queueName, int skip, int take)
        {
            using var _ = StartActivity();
            return await GetMessages<QueueData>(QueueFields, queueName, Pending, skip, take);
        }

        public async Task<List<SubscribedData>> GetFailedSubscribedMessages(int skip, int take)
        {
            using var _ = StartActivity();
            return await GetMessages<SubscribedData>(SubscribedFields, Subscribed, Failed, skip, take);
        }

        public async Task<List<SubscribedData>> GetCompletedSubscribedMessages(int skip, int take)
        {
            using var _ = StartActivity();
            return await GetMessages<SubscribedData>(SubscribedFields, Subscribed, Completed, skip, take);
        }

        public async Task<List<SubscribedData>> GetPendingSubscribedMessages(int skip, int take)
        {
            using var _ = StartActivity();
            return await GetMessages<SubscribedData>(SubscribedFields, Subscribed, Pending, skip, take);
        }

        public async Task CancelPendingQueueMessage(QueueName queueName, Identity id)
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

            using var _ = StartActivity();

            string statement = string.Format(CancelPendingQueuedStatement, _configuration.Schema, queueName);

            var p = new DynamicParameters();
            p.Add("@Id", id);

            await LogIfError(dapper.Execute(statement, p));
        }

        public async Task CancelPendingSubscribedMessage(Identity id)
        {
            const string CancelPendingSubscribedStatement =
                """
                INSERT INTO [{0}].[Subscribed_Failed] WITH (ROWLOCK) 
                ([Id], [SubscriberId], [Topic], [ValidUntil], [MessageId], [Priority], [NotBefore], [Enqueued], [Completed], [Failed], [Retries], [Headers], [Body])
                SELECT D.[Id], D.[SubscriberId], D.[Topic], D.[ValidUntil], D.[MessageId], D.[Priority], D.[NotBefore], D.[Enqueued], NULL, SYSUTCDATETIME(), D.[Retries], D.[Headers], D.[Body] FROM
                    (DELETE FROM [{0}].[Subscribed_Pending] WITH (ROWLOCK)
                        OUTPUT DELETED.*
                        WHERE [Id] = @Id) D
                """;

            using var _ = StartActivity();

            string statement = string.Format(CancelPendingSubscribedStatement, _configuration.Schema);

            var p = new DynamicParameters();
            p.Add("@Id", id);

            await LogIfError(dapper.Execute(statement, p));
        }

        public async Task RetryFailedQueueMessage(QueueName queueName, Identity id)
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

            using var _ = StartActivity();

            string statement = string.Format(RetryFailedQueuedStatement, _configuration.Schema, queueName);

            var p = new DynamicParameters();
            p.Add("@Id", id);

            await LogIfError(dapper.Execute(statement, p));
        }

        public async Task RetryFailedSubscribedMessage(Identity id)
        {
            const string RetryFailedSubscribedStatement =
                """
                INSERT INTO [{0}].[Subscribed_Pending] WITH (ROWLOCK) 
                ([SubscriberId], [Topic], [ValidUntil], [MessageId], [Priority], [NotBefore], [Enqueued], [Completed], [Failed], [Retries], [Headers], [Body])
                SELECT D.[SubscriberId], D.[Topic], D.[ValidUntil], D.[MessageId], D.[Priority], D.[NotBefore], D.[Enqueued], NULL, NULL, 0, D.[Headers], D.[Body] FROM
                    (DELETE FROM [{0}].[Subscribed_Failed] WITH (ROWLOCK)
                        OUTPUT DELETED.*
                        WHERE [Id] = @Id) D
                """;

            using var _ = StartActivity();

            string statement = string.Format(RetryFailedSubscribedStatement, _configuration.Schema);

            var p = new DynamicParameters();
            p.Add("@Id", id);

            await LogIfError(dapper.Execute(statement, p));
        }

        [ExcludeFromCodeCoverage]
        private async Task<T> LogIfError<T>(Task<T> task, [CallerMemberName] string caller = "Unnammed")
        {
            try
            {
                return await task;
            }
            catch (Exception ex)
            {
                _log.DataAccessError(caller, ex);
                throw;
            }
        }

        private static Activity? StartActivity([CallerMemberName] string caller = "Unnamed")
        {
            return ActivitySources.DataAccess.StartActivity(
                "peachtreebus.managementdataaccess " + caller,
                ActivityKind.Internal);
        }
    }
}
