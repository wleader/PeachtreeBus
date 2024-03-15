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
        public string QueueName { get; }
    }

    /// <summary>
    /// A default implementation of IQueueCleanerConfiguration
    /// </summary>
    public class QueueCleanerConfiguration(
        string queueName,
        int maxDeleteCount,
        bool cleanCompleted,
        bool cleanFailed,
        TimeSpan ageLimit,
        TimeSpan interval)
        : BaseCleanupConfiguration(maxDeleteCount, cleanCompleted, cleanFailed, ageLimit, interval)
        , IQueueCleanerConfiguration
    {
        public string QueueName { get; private set; } = queueName;
    }
}
