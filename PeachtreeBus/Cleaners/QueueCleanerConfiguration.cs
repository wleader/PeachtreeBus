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
    public class QueueCleanerConfiguration : BaseCleanupConfiguration, IQueueCleanerConfiguration
    {
        public string QueueName { get; private set; }

        public QueueCleanerConfiguration(string queueName, int maxDeleteCount, bool cleanCompleted, bool cleanFailed,
            TimeSpan ageLimit, TimeSpan interval)
            : base(maxDeleteCount, cleanCompleted, cleanFailed, ageLimit, interval)
        {
            QueueName = queueName;
        }
    }
}
