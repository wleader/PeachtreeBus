﻿namespace PeachtreeBus.Subscriptions;

public interface ISubscribedContext : IBaseContext<SubscribedMessage>
{
    public SubscriberId SubscriberId { get; }
}

/// <summary>
/// Stores contextual data about the subscription message being handled,
/// that may be useful to application code.
/// </summary>
public class SubscribedContext : BaseContext<SubscribedMessage>, ISubscribedContext
{
    /// <summary>
    /// The Subscriber that the message was sent to.
    /// </summary>
    public SubscriberId SubscriberId { get => MessageData.SubscriberId; }
}
