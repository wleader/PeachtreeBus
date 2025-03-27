using PeachtreeBus.Data;

namespace PeachtreeBus;

/// <summary>
/// Context information that is common to all context types.
/// </summary>
public interface IContext
{
    IWrappedScope? Scope { get; }
    object Message { get; }
    UserHeaders Headers { get; }
}

/// <summary>
/// Context information for incoming messages
/// </summary>
public interface IIncomingContext : IContext
{
    /// <summary>
    /// When the message was 'sent'.
    /// </summary>
    UtcDateTime EnqueuedTime { get; }

    UtcDateTime NotBefore { get; }

    /// <summary>
    /// A Unique Identifier for the message.
    /// </summary>
    UniqueIdentity MessageId { get; }

    /// <summary>
    /// The Priority the message was sent with.
    /// </summary>
    int MessagePriority { get; }

    string MessageClass { get; }
}

public interface IOutgoingContext : IContext
{
    UtcDateTime NotBefore { get; set; }
    int MessagePriority { get; set; }
}
