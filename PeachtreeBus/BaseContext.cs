using PeachtreeBus.Data;
using PeachtreeBus.Queues;
using System;

namespace PeachtreeBus;

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

public static class ContextExtensions
{
    internal static void SetScope(this IBaseContext context, IWrappedScope scope)
    {
        if (context is BaseContext baseContext)
            baseContext.Scope = scope;
    }
}

public interface IBaseIncomingContext<TData> : IBaseContext
    where TData : QueueData
{
    public IHeaders Headers { get; }
    internal TData Data { get; set; }
    public UtcDateTime EnqueuedTime { get; }
    public UniqueIdentity MessageId { get; }
    public int MessagePriority { get; }
}

public abstract class BaseIncomingContext<TQueueData>
    : BaseContext
    , IBaseIncomingContext<TQueueData>
    where TQueueData : QueueData
{
    /// <summary>
    /// The Model of the message as was stored the database.
    /// </summary>
    public required TQueueData Data { get; set; } = default!;

    /// <summary>
    /// The priority value of the message being handled.
    /// </summary>
    public int MessagePriority { get => Data.Priority; }

    public UtcDateTime EnqueuedTime { get => Data.Enqueued; }
    public UniqueIdentity MessageId { get => Data.MessageId; }

    /// <summary>
    /// Headers that were stored with the message.
    /// </summary>
    public Headers Headers { get; set; } = new Headers();

    IHeaders IBaseIncomingContext<TQueueData>.Headers { get => Headers; }
}

public interface IBaseOutgoingContext<TQueueData> : IBaseContext
    where TQueueData : QueueData
{
    public UserHeaders? UserHeaders { get; set; }
    public UtcDateTime? NotBefore { get; set; }
    public int Priority { get; set; }
    public Type Type { get; }
}

public abstract class BaseOutgoingContext<TQueueData>
    : BaseContext
    , IBaseOutgoingContext<TQueueData>
    where TQueueData : QueueData
{
    public UserHeaders? UserHeaders { get; set; }
    public UtcDateTime? NotBefore { get; set; }
    public int Priority { get; set; }
    public required Type Type { get; set; }
}





