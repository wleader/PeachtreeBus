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
    /// <param name="type">The Type of the message.</param>
    /// <param name="message">an instance of an IQueueMessage.</param>
    /// <param name="notBefore">Indicates when this message can be processed. Will default to 'Now' if not provided.</param>
    /// <param name="priority">A priority for the message. Higher priorities are processed first.</param>
    /// <param name="userHeaders">Any user headers to be included with the message.</param>
    /// <returns></returns>
    Task WriteMessage(
        QueueName queueName,
        IQueueMessage message,
        DateTime? notBefore = null,
        int priority = 0,
        UserHeaders? userHeaders = null);
}

/// <summary>
///  Adds a message to a queue using an IBusDataAccess.
/// </summary>
public class QueueWriter(
    ISystemClock clock,
    ISendPipelineInvoker pipelineInvoker) : IQueueWriter
{
    private readonly ISystemClock _clock = clock;
    private readonly ISendPipelineInvoker pipelineInvoker = pipelineInvoker;

    public async Task WriteMessage(
        QueueName queueName,
        IQueueMessage message,
        DateTime? notBefore = null,
        int priority = 0,
        UserHeaders? userHeaders = null)
    {
        ArgumentNullException.ThrowIfNull(message, nameof(message));

        var context = new SendContext()
        {
            Destination = queueName,
            NotBefore = notBefore ?? _clock.UtcNow,
            MessagePriority = priority,
            Message = message,
            Headers = userHeaders ?? [],
        };

        await pipelineInvoker.Invoke(context);
    }
}

