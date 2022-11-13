using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PeachtreeBus.Example
{
    internal static class Logging
    {
        private static readonly Action<ILogger, int, string, int, int, Exception> DistributedTaskCompleteAction =
            LoggerMessage.Define<int, string, int, int>(LogLevel.Information, 1,
                "Distributed Task Complete: {ValueA} {Operation} {ValueB} = {Result}");

        private static readonly Action<ILogger, int, Exception> PendingTasksRemainingAction =
            LoggerMessage.Define<int>(LogLevel.Information, 2,
                "{Count} tasks remaining.");

        private static readonly Action<ILogger, Guid, Exception> CompletingSagaAction =
            LoggerMessage.Define<Guid>(LogLevel.Information, 3,
                "Completing Saga {SagaId}.");

        private static readonly Action<ILogger, Guid, Exception> DistributingMoreWorkAction =
            LoggerMessage.Define<Guid>(LogLevel.Information, 4,
                "Distributing more work for Saga {SagaId}.");

        private static readonly Action<ILogger, Guid, Exception> StartingTasksAction =
            LoggerMessage.Define<Guid>(LogLevel.Information, 5,
                "Starting Tasks for Saga {SagaId}.");

        private static readonly Action<ILogger, Guid, Guid, Exception> SubscribedSagaCompleteAction =
            LoggerMessage.Define<Guid, Guid>(LogLevel.Information, 6,
                "Subscriber {SubscriberId} got a Saga complete announcement {SagaId}");

        private static readonly Action<ILogger, Exception> ProcessingDistributedTaskAction =
            LoggerMessage.Define(LogLevel.Information, 7,
                "Processing Distributed Task.");

        private static readonly Action<ILogger, int, string, int, int, Exception> DistributedTaskResultAction =
            LoggerMessage.Define<int, string, int, int>(LogLevel.Information, 8,
                "Distributed Task Result: {ValueA} {Operation} {ValueB} = {Result}");

        private static readonly Action<ILogger, Exception> DistributedSagaCompleteAction =
            LoggerMessage.Define(LogLevel.Information, 9,
                "Distributed Saga Complete!");

        internal static void DistributedTaskComplete(this ILogger logger, string operation, int valueA, int valueB, int result)
        {
            DistributedTaskCompleteAction(logger, valueA, operation, valueB, result, null);
        }

        internal static void PendingTasksRemaining(this ILogger logger, int count)
        {
            PendingTasksRemainingAction(logger, count, null);
        }

        internal static void CompletingSaga(this ILogger logger, Guid sagaId)
        {
            CompletingSagaAction(logger, sagaId, null);
        }

        internal static void DistributingMoreWork(this ILogger logger, Guid sagaId)
        {
            DistributingMoreWorkAction(logger, sagaId, null);
        }

        internal static void StartingTasks(this ILogger logger, Guid sagaId)
        {
            StartingTasksAction(logger, sagaId, null);
        }

        internal static void SubscribedSagaComplete(this ILogger logger, Guid subscriberId, Guid sagaId)
        {
            SubscribedSagaCompleteAction(logger, subscriberId, sagaId, null);
        }

        internal static void ProcessingDistributedTask(this ILogger logger)
        {
            ProcessingDistributedTaskAction(logger, null);
        }

        internal static void DistributedTaskResult(this ILogger logger, string operation, int valueA, int valueB, int result)
        {
            DistributedTaskResultAction(logger, valueA, operation, valueB, result, null);
        }

        internal static void DistributedSagaComplete(this ILogger logger)
        {
            DistributedSagaCompleteAction(logger, null);
        }
    }
}
