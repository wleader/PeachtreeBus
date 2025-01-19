using PeachtreeBus.Cleaners;
using PeachtreeBus.Queues;
using SimpleInjector;
using System;

namespace PeachtreeBus.SimpleInjector
{
    public static partial class SimpleInjectorExtensions
    {
        /// <summary>
        /// Enables cleanup of the message queue backing tables.
        /// </summary>
        /// <param name="container">The Simple Injector container.</param>
        /// <param name="queueName">The queue that will be cleaned.</param>
        /// <param name="maxDeleteCount">The max number of rows to delete in a single DB operation.</param>
        /// <param name="cleanCompleted">Indicates if the completed table should be cleaned.</param>
        /// <param name="cleanFailed">Indicates if the failed table should be cleaned.</param>
        /// <param name="ageLimit">Determines how long after completion or failure before the message can be cleaned.</param>
        /// <param name="interval">determines how much time to wait between searching for data to clean.</param>
        /// <returns></returns>
        public static Container CleanupQueue(this Container container, QueueName queueName,
            int maxDeleteCount, bool cleanCompleted, bool cleanFailed,
            TimeSpan ageLimit, TimeSpan interval)
        {
            CleanupQueue(container, new QueueCleanerConfiguration(
                queueName,
                maxDeleteCount,
                cleanCompleted,
                cleanFailed,
                ageLimit,
                interval));
            return container;
        }

        /// <summary>
        /// Enables cleanup of the message queue backing tables.
        /// </summary>
        /// <param name="container">The Simple Injector container.</param>
        /// <param name="config">Configuration for the queue cleanup.</param>
        /// <returns></returns>
        public static Container CleanupQueue(this Container container, IQueueCleanerConfiguration config)
        {
            container.Register<IQueueCleanupThread, QueueCleanupThread>(Lifestyle.Scoped);
            container.Register<IQueueCleanupWork, QueueCleanupWork>(Lifestyle.Scoped);
            container.Register<IQueueCleaner, QueueCleaner>(Lifestyle.Scoped);
            container.RegisterSingleton(() => config);
            return container;
        }
    }
}
