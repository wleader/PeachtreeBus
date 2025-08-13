using System;
using System.Threading.Tasks;

namespace PeachtreeBus.Queues;

/// <summary>
/// Defines an interface for adding a message to a queue.
/// </summary>
public interface IQueueWriter
{
    /// <summary>
    /// Writes a message to a queue
    /// </summary>
    /// <param name="queueName">Which queue to write to.</param>
    /// <param name="message">an instance of an IQueueMessage.</param>
    /// <param name="notBefore">Indicates when this message can be processed. Will default to 'Now' if not provided.</param>
    /// <param name="priority">A priority for the message. Higher priorities are processed first.</param>
    /// <param name="userHeaders">Any user headers to be included with the message.</param>
    Task WriteMessage(
        QueueName queueName,
        IQueueMessage message,
        DateTime? notBefore = null,
        int priority = 0,
        UserHeaders? userHeaders = null,
        bool newConversation = false);
}