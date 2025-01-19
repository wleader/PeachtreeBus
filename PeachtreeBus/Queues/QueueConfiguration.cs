namespace PeachtreeBus.Queues
{
    /// <summary>
    /// Provides configuration to threads about
    /// which queue it should use.
    /// </summary>
    public interface IQueueConfiguration
    {
        QueueName QueueName { get; }
    }

    /// <summary>
    /// A default implementation of IQueueConfiguration
    /// </summary>
    public class QueueConfiguration(
        QueueName queueName)
        : IQueueConfiguration
    {
        public QueueName QueueName { get; private set; } = queueName;
    }
}
