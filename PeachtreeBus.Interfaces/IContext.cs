using PeachtreeBus.Data;

namespace PeachtreeBus;

/// <summary>
/// Context information that is common to all context types.
/// </summary>
public interface IContext
{
    public IWrappedScope? Scope { get; }
    public object Message { get; }
}

/// <summary>
/// Context information for incoming messages
/// </summary>
public interface IIncomingContext : IContext
{
    /// <summary>
    /// The message headers.
    /// </summary>
    public IHeaders Headers { get; }

    /// <summary>
    /// When the message was 'sent'.
    /// </summary>
    public UtcDateTime EnqueuedTime { get; }

    /// <summary>
    /// A Unique Identifier for the message.
    /// </summary>
    public UniqueIdentity MessageId { get; }

    /// <summary>
    /// The Priority the message was sent with.
    /// </summary>
    public int MessagePriority { get; }
}

