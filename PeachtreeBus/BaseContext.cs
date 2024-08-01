using System;

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
    public Guid MessageId { get; set; }

    /// <summary>
    /// Exposes the Dependency Injection Scope for the message being handled.
    /// Experimmental. This may be removed in a future update.
    /// </summary>
    public IWrappedScope? Scope { get; set; }
}
