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
    public class QueueConfiguration(
        string queueName)
        : IQueueConfiguration
    {
        public string QueueName { get; private set; } = queueName;
    }
}
