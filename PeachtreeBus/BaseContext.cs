using PeachtreeBus.Data;

namespace PeachtreeBus;

public abstract class BaseContext
{
    /// <summary>
    /// The message itself.
    /// </summary>
    public object Message { get; set; } = default!;

    /// <summary>
    /// A unique Id for the message.
    /// </summary>
    public UniqueIdentity MessageId { get; set; }

    /// <summary>
    /// Exposes the Dependency Injection Scope for the message being handled.
    /// Experimmental. This may be removed in a future update.
    /// </summary>
    public IWrappedScope? Scope { get; set; }

    /// <summary>
    /// The priority value of the message being handled.
    /// </summary>
    public int MessagePriority { get; set; }

    /// <summary>
    /// Headers that were stored with the message.
    /// </summary>
    public Headers Headers { get; set; } = new();
}
