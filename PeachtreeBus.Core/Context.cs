using PeachtreeBus.ClassNames;
using PeachtreeBus.Data;
using PeachtreeBus.Queues;
using System;

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

    public abstract ClassName MessageClass { get; }
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

    public Headers Headers => Data.Headers!;

    /// <summary>
    /// The priority value of the message being handled.
    /// </summary>
    public int MessagePriority { get => Data.Priority; }

    public UtcDateTime EnqueuedTime { get => Data.Enqueued; }
    public UtcDateTime NotBefore { get => Data.NotBefore; }
    public UniqueIdentity MessageId { get => Data.MessageId; }
    public override ClassName MessageClass { get => Headers.MessageClass; }
    public IReadOnlyUserHeaders UserHeaders { get => Headers.UserHeaders; }
}

public abstract class OutgoingContext<TQueueData>
    : Context
    , IOutgoingContext
    where TQueueData : QueueData
{
    public required TQueueData Data { get; set; } = default!;

    public UtcDateTime NotBefore { get => Data.NotBefore; set => Data.NotBefore = value; }
    
    public int MessagePriority { get => Data.Priority; set => Data.Priority = value; }
    
    public bool StartNewConversation { get; set; }
    
    public UserHeaders UserHeaders { get => Data.Headers!.UserHeaders; }

    public override ClassName MessageClass => Data.Headers!.MessageClass;
}




