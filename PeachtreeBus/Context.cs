using PeachtreeBus.Data;
using PeachtreeBus.Queues;

namespace PeachtreeBus;

public abstract class Context : IContext
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

    public UserHeaders Headers { get; set; } = [];
}

public abstract class IncomingContext<TQueueData>
    : Context
    , IIncomingContext
    where TQueueData : QueueData
{
    /// <summary>
    /// The Model of the message as was stored the database.
    /// </summary>
    public required TQueueData Data { get; set; } = default!;
    public required Headers InternalHeaders { get; set; }

    /// <summary>
    /// The priority value of the message being handled.
    /// </summary>
    public int MessagePriority { get => Data.Priority; }

    public UtcDateTime EnqueuedTime { get => Data.Enqueued; }
    public UniqueIdentity MessageId { get => Data.MessageId; }
    public string MessageClass { get => InternalHeaders.MessageClass; }
}

public abstract class OutgoingContext<TQueueData>
    : Context
    , IOutgoingContext
    where TQueueData : QueueData
{
    public UtcDateTime? NotBefore { get; set; }
    public int Priority { get; set; }
}





