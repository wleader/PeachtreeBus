using PeachtreeBus.Data;

namespace PeachtreeBus;

public interface IBaseContext
{
    public IWrappedScope? Scope { get; internal set; }
    public IHeaders Headers { get; }
    public object Message { get; }
}


public abstract class BaseContext : IBaseContext
{
    /// <summary>
    /// The message itself.
    /// </summary>
    public required object Message { get; set; } = default!;

    /// <summary>
    /// A unique Id for the message.
    /// </summary>
    public required UniqueIdentity MessageId { get; set; }

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
    public Headers Headers { get; set; } = new Headers();

    IHeaders IBaseContext.Headers { get => Headers; }
}
