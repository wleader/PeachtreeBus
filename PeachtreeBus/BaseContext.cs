using PeachtreeBus.Data;
using PeachtreeBus.Queues;
using System;

namespace PeachtreeBus;

public interface IBaseContext
{
    public IWrappedScope? Scope { get; internal set; }
    public object Message { get; }
}

public abstract class BaseContext : IBaseContext
{
    /// <summary>
    /// The message itself.
    /// </summary>
    public required object Message { get; set; } = default!;

    /// <summary>
    /// Exposes the Dependency Injection Scope for the message being handled.
    /// Experimmental. This may be removed in a future update.
    /// </summary>
    public IWrappedScope? Scope { get; set; }
}

public interface IBaseIncomingContext<TQueueMessage> : IBaseContext
    where TQueueMessage : QueueMessage
{
    public IHeaders Headers { get; }
    internal TQueueMessage MessageData { get; set; }
    public UtcDateTime EnqueuedTime { get; }
    public UniqueIdentity MessageId { get; }
    public int MessagePriority { get; }
}

public abstract class BaseIncomingContext<TQueueMessage>
    : BaseContext
    , IBaseIncomingContext<TQueueMessage>
    where TQueueMessage : QueueMessage
{
    /// <summary>
    /// The Model of the message as was stored the database.
    /// </summary>
    public required TQueueMessage MessageData { get; set; } = default!;

    /// <summary>
    /// The priority value of the message being handled.
    /// </summary>
    public int MessagePriority { get => MessageData.Priority; }

    public UtcDateTime EnqueuedTime { get => MessageData.Enqueued; }
    public UniqueIdentity MessageId { get => MessageData.MessageId; }

    /// <summary>
    /// Headers that were stored with the message.
    /// </summary>
    public Headers Headers { get; set; } = new Headers();

    IHeaders IBaseIncomingContext<TQueueMessage>.Headers { get => Headers; }
}

public interface IBaseOutgoingContext<TQueueMessage> : IBaseContext
    where TQueueMessage : QueueMessage
{
    public UserHeaders? UserHeaders { get; set; }
    public UtcDateTime? NotBefore { get; set; }
    public int Priority { get; set; }
    public Type Type { get; }
}

public abstract class BaseOutgoingContext<TQueueMessage>
    : BaseContext
    , IBaseOutgoingContext<TQueueMessage>
    where TQueueMessage : QueueMessage
{
    public UserHeaders? UserHeaders { get; set; }
    public UtcDateTime? NotBefore { get; set; }
    public int Priority { get; set; }
    public required Type Type { get; set; }
}





