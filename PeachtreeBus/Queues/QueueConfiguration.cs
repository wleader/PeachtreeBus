namespace PeachtreeBus.Queues
{
    /// <summary>
    /// Provides configuration to threads about
    /// which queue it should use.
    /// </summary>
    public interface IQueueConfiguration
    {
        string QueueName { get; }
    }

    /// <summary>
    /// A default implementation of IQueueConfiguration
    /// </summary>
    public class QueueConfiguration : IQueueConfiguration
    {
        public string QueueName { get; private set; }

        public QueueConfiguration(string queueName)
        {
            QueueName = queueName;
        }
    }
}
