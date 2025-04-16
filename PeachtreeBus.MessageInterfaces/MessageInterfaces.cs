namespace PeachtreeBus.Queues
{
    /// <summary>
    /// All queue messages passed through the queue system must implement this interface.
    /// </summary>
    public interface IQueueMessage;
}

namespace PeachtreeBus.Subscriptions
{

    /// <summary>
    /// All subscribed messages passed through the subscription system must implement this interface.
    /// </summary>
    public interface ISubscribedMessage { }
}

