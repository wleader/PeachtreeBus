using PeachtreeBus.ClassNames;
using PeachtreeBus.Data;
using PeachtreeBus.Queues;
using System;

namespace PeachtreeBus;

/// <inheritdoc/>
public abstract class Context : IContext
{
    /// <inheritdoc/>
    public required object Message { get; set; } = default!;

    /// <summary>
    /// Exposes the Dependency Injection Scope for the message being handled.
    /// Experimmental. This may be removed in a future update.
    /// </summary>
    public IWrappedScope? Scope { get; set; }

    /// <inheritdoc/>
    public abstract ClassName MessageClass { get; }
}

/// <summary>
/// Context information for incoming messages
/// </summary>
public abstract class IncomingContext<TQueueData>
    : Context
    , IIncomingContext
    where TQueueData : QueueData
{
    /// <summary>
    /// The Model of the message as was stored the database.
    /// </summary>
    public required TQueueData Data { get; set; } = default!;

    /// <summary>
    /// The internal headers used by the messaging library.
    /// </summary>
    public Headers Headers => Data.Headers!;

    /// <summary>
    /// The priority value of the message being handled.
    /// </summary>
    public int MessagePriority { get => Data.Priority; }

    /// <summary>
    /// The time the mesasage was sent.
    /// </summary>
    public UtcDateTime EnqueuedTime { get => Data.Enqueued; }

    /// <inheritdoc/>
    public UtcDateTime NotBefore { get => Data.NotBefore; }

    /// <summary>
    /// A Unique Id for the message.
    /// </summary>
    public UniqueIdentity MessageId { get => Data.MessageId; }

    /// <inheritdoc/>
    public override ClassName MessageClass { get => Headers.MessageClass; }

    /// <inheritdoc/>
    public IReadOnlyUserHeaders UserHeaders { get => Headers.UserHeaders; }
}

/// <inheritdoc/>
public abstract class OutgoingContext<TQueueData>
    : Context
    , IOutgoingContext
    where TQueueData : QueueData
{
    /// <summary>
    /// The internal data for the message.
    /// </summary>
    public required TQueueData Data { get; set; } = default!;

    /// <inheritdoc/>
    public UtcDateTime NotBefore { get => Data.NotBefore; set => Data.NotBefore = value; }

    /// <inheritdoc/>
    public int MessagePriority { get => Data.Priority; set => Data.Priority = value; }

    /// <inheritdoc/>
    public bool StartNewConversation { get; set; }

    /// <inheritdoc/>
    public UserHeaders UserHeaders { get => Data.Headers!.UserHeaders; }

    /// <inheritdoc/>
    public override ClassName MessageClass => Data.Headers!.MessageClass;
}




