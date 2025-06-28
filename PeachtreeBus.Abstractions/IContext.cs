using PeachtreeBus.ClassNames;
using PeachtreeBus.Data;
using System;

namespace PeachtreeBus;

/// <summary>
/// Context information that is common to all context types.
/// </summary>
public interface IContext
{
    /// <summary>
    /// A reference to the IServiceProvider for the scope of the current message.
    /// </summary>
    IServiceProvider? ServiceProvider { get; }

    /// <summary>
    /// The current message being handled, or sent.
    /// </summary>
    object Message { get; }

    /// <summary>
    /// The Type Name of the message.
    /// </summary>
    ClassName MessageClass { get; }
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

    /// <summary>
    /// A time to wait until before processing the message.
    /// </summary>
    UtcDateTime NotBefore { get; }

    /// <summary>
    /// A Unique Identifier for the message.
    /// </summary>
    UniqueIdentity MessageId { get; }

    /// <summary>
    /// The Priority the message was sent with.
    /// </summary>
    int MessagePriority { get; }

    /// <summary>
    /// Any additional headers for the message that are povided by
    /// the user of the messaging library.
    /// </summary>
    IReadOnlyUserHeaders UserHeaders { get; }
}

/// <summary>
/// Context information for outgoing messages.
/// </summary>
public interface IOutgoingContext : IContext
{
    /// <summary>
    /// A time to wait until before processing the message.
    /// </summary>
    UtcDateTime NotBefore { get; set; }

    /// <summary>
    /// A priority for the message.
    /// Higher values are processed sooner.
    /// </summary>
    int MessagePriority { get; set; }

    /// <summary>
    /// Indicates that the outgoing message is the start of a new conversation.
    /// When False, the outgoing message is linked to any incoming message
    /// in any activity traces.
    /// </summary>
    bool StartNewConversation { get; set; }

    /// <summary>
    /// Provides a means for the user to attach any additional headers
    /// to the outgoing message.
    /// </summary>
    UserHeaders UserHeaders { get; }
}
