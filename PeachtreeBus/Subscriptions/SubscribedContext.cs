namespace PeachtreeBus.Subscriptions;

/// <summary>
/// Stores contextual data about the subscription message being handled,
/// that may be useful to application code.
/// </summary>
public class SubscribedContext : IncomingContext<SubscribedMessage>, ISubscribedContext
{
    /// <summary>
    /// The Subscriber that the message was sent to.
    /// </summary>
    public SubscriberId SubscriberId { get => Data.SubscriberId; }
}
