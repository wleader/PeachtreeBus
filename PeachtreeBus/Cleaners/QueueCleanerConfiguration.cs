using PeachtreeBus.Queues;
using System;

namespace PeachtreeBus.Cleaners
{
    /// <summary>
    /// Describes configuration needed by QueueCleaner
    /// </summary>
    public interface IQueueCleanerConfiguration : IBaseCleanupConfiguration
    {
        /// <summary>
        /// The Queue that will be cleaned.
        /// </summary>
        public QueueName QueueName { get; }
    }

    /// <summary>
    /// A default implementation of IQueueCleanerConfiguration
    /// </summary>
    public class QueueCleanerConfiguration(
        QueueName queueName,
        int maxDeleteCount,
        bool cleanCompleted,
        bool cleanFailed,
        TimeSpan ageLimit,
        TimeSpan interval)
        : BaseCleanupConfiguration(maxDeleteCount, cleanCompleted, cleanFailed, ageLimit, interval)
        , IQueueCleanerConfiguration
    {
        public QueueName QueueName { get; private set; } = queueName;
    }
}
