using PeachtreeBus.Data;
using PeachtreeBus.Queues;

namespace PeachtreeBus;

public interface IBaseContext
{
    public IWrappedScope? Scope { get; internal set; }
    public IHeaders Headers { get; }
    public object Message { get; }
    public UtcDateTime EnqueuedTime { get; }
    public UniqueIdentity MessageId { get; }
    public int MessagePriority { get; }
}

public interface IBaseContext<TQueueMessage> : IBaseContext
    where TQueueMessage : QueueMessage
{
    internal TQueueMessage MessageData { get; set; }
}


public abstract class BaseContext<TQueueMessage> : IBaseContext<TQueueMessage>
    where TQueueMessage : QueueMessage
{
    /// <summary>
    /// The message itself.
    /// </summary>
    public required object Message { get; set; } = default!;

    /// <summary>
    /// The Model of the message as was stored the database.
    /// </summary>
    public required TQueueMessage MessageData { get; set; } = default!;

    /// <summary>
    /// Exposes the Dependency Injection Scope for the message being handled.
    /// Experimmental. This may be removed in a future update.
    /// </summary>
    public IWrappedScope? Scope { get; set; }

    /// <summary>
    /// The priority value of the message being handled.
    /// </summary>
    public int MessagePriority { get => MessageData.Priority; }

    /// <summary>
    /// Headers that were stored with the message.
    /// </summary>
    public Headers Headers { get; set; } = new Headers();

    IHeaders IBaseContext.Headers { get => Headers; }

    public UtcDateTime EnqueuedTime { get => MessageData.Enqueued; }
    public UniqueIdentity MessageId { get => MessageData.MessageId; }
}
