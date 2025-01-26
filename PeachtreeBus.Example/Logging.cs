using Microsoft.Extensions.Logging;
using PeachtreeBus.Subscriptions;
using System;

namespace PeachtreeBus.Example
{
    internal static class Logging
    {
        private static readonly Action<ILogger, int, string, int, int, Exception?> DistributedTaskCompleteAction =
            LoggerMessage.Define<int, string, int, int>(LogLevel.Information, 1,
                "Distributed Task Complete: {ValueA} {Operation} {ValueB} = {Result}");
        internal static void DistributedTaskComplete(this ILogger logger, string operation, int valueA, int valueB, int result)
        {
            DistributedTaskCompleteAction(logger, valueA, operation, valueB, result, null);
        }

        private static readonly Action<ILogger, int, Exception?> PendingTasksRemainingAction =
            LoggerMessage.Define<int>(LogLevel.Information, 2,
                "{Count} tasks remaining.");
        internal static void PendingTasksRemaining(this ILogger logger, int count)
        {
            PendingTasksRemainingAction(logger, count, null);
        }

        private static readonly Action<ILogger, Guid, Exception?> CompletingSagaAction =
            LoggerMessage.Define<Guid>(LogLevel.Information, 3,
                "Completing Saga {SagaId}.");

        internal static void CompletingSaga(this ILogger logger, Guid sagaId)
        {
            CompletingSagaAction(logger, sagaId, null);
        }

        private static readonly Action<ILogger, Guid, Exception?> DistributingMoreWorkAction =
            LoggerMessage.Define<Guid>(LogLevel.Information, 4,
                "Distributing more work for Saga {SagaId}.");

        internal static void DistributingMoreWork(this ILogger logger, Guid sagaId)
        {
            DistributingMoreWorkAction(logger, sagaId, null);
        }

        private static readonly Action<ILogger, Guid, Exception?> StartingTasksAction =
            LoggerMessage.Define<Guid>(LogLevel.Information, 5,
                "Starting Tasks for Saga {SagaId}.");

        internal static void StartingTasks(this ILogger logger, Guid sagaId)
        {
            StartingTasksAction(logger, sagaId, null);
        }

        private static readonly Action<ILogger, SubscriberId, Guid, Exception?> SubscribedSagaCompleteAction =
            LoggerMessage.Define<SubscriberId, Guid>(LogLevel.Information, 6,
                "Subscriber {SubscriberId} got a Saga complete announcement {SagaId}");

        internal static void SubscribedSagaComplete(this ILogger logger, SubscriberId subscriberId, Guid sagaId)
        {
            SubscribedSagaCompleteAction(logger, subscriberId, sagaId, null);
        }

        private static readonly Action<ILogger, Exception?> ProcessingDistributedTaskAction =
            LoggerMessage.Define(LogLevel.Information, 7,
                "Processing Distributed Task.");
        internal static void ProcessingDistributedTask(this ILogger logger)
        {
            ProcessingDistributedTaskAction(logger, null);
        }

        private static readonly Action<ILogger, int, string, int, int, Exception?> DistributedTaskResultAction =
            LoggerMessage.Define<int, string, int, int>(LogLevel.Information, 8,
                "Distributed Task Result: {ValueA} {Operation} {ValueB} = {Result}");

        internal static void DistributedTaskResult(this ILogger logger, int valueA, string operation, int valueB, int result)
        {
            DistributedTaskResultAction(logger, valueA, operation, valueB, result, null);
        }

        private static readonly Action<ILogger, Exception?> DistributedSagaCompleteAction =
            LoggerMessage.Define(LogLevel.Information, 9,
                "Distributed Saga Complete!");

        internal static void DistributedSagaComplete(this ILogger logger)
        {
            DistributedSagaCompleteAction(logger, null);
        }
    }
}
