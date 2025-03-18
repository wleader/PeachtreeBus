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
        Type type,
        object message,
        DateTime? notBefore = null,
        int priority = 0,
        UserHeaders? userHeaders = null);
}

/// <summary>
///  Adds a message to a queue using an IBusDataAccess.
/// </summary>
public class QueueWriter(
    ISendPipelineInvoker pipelineInvoker) : IQueueWriter
{
    private readonly ISendPipelineInvoker pipelineInvoker = pipelineInvoker;

    public async Task WriteMessage(
        QueueName queueName,
        Type type,
        object message,
        DateTime? notBefore = null,
        int priority = 0,
        UserHeaders? userHeaders = null)
    {
        ArgumentNullException.ThrowIfNull(message, nameof(message));
        ArgumentNullException.ThrowIfNull(type, nameof(type));
        TypeIsNotIQueueMessageException.ThrowIfMissingInterface(type);

        var context = new SendContext()
        {
            Destination = queueName,
            NotBefore = notBefore,
            Priority = priority,
            Message = message,
            Headers = userHeaders ?? [],
        };

        await pipelineInvoker.Invoke(context);
    }

}

public static class QueueWriterExtensions
{
    /// <summary>
    /// Writes a message to a queue
    /// </summary>
    public static async Task WriteMessage<T>(
        this IQueueWriter writer,
        QueueName queueName,
        T message,
        DateTime? NotBefore = null,
        int priority = 0,
        UserHeaders? userHeaders = null)
        where T : notnull
    {
        await writer.WriteMessage(queueName, typeof(T), message, NotBefore, priority, userHeaders);
    }
}
