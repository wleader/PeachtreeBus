namespace PeachtreeBus.Subscriptions;

public interface ISubscribedContext : IBaseContext
{
    public SubscriberId SubscriberId { get; }
}

/// <summary>
/// Stores contextual data about the subscription message being handled,
/// that may be useful to application code.
/// </summary>
public class SubscribedContext : BaseContext, ISubscribedContext
{
    /// <summary>
    /// The Subscriber that the message was sent to.
    /// </summary>
    public SubscriberId SubscriberId { get; set; }

    /// <summary>
    /// The message as read from the database.
    /// </summary>
    public SubscribedMessage MessageData { get; set; } = default!;
}
