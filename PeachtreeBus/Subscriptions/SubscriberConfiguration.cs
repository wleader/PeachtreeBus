using System;
using System.Collections.Generic;

namespace PeachtreeBus.Subscriptions
{
    /// <summary>
    /// Defines the subscriptions for a given process.
    /// </summary>
    public interface ISubscriberConfiguration
    {
        SubscriberId SubscriberId { get; }
        IList<Category> Categories { get; }
        TimeSpan Lifespan { get; }
    }

    /// <summary>
    /// default implementation of ISubscriberConfiguration
    /// </summary>
    public class SubscriberConfiguration(
        SubscriberId subscriberId,
        TimeSpan lifespan,
        params Category[] categories)
        : ISubscriberConfiguration
    {
        public SubscriberId SubscriberId { get; } = subscriberId;
        public IList<Category> Categories { get; } = [.. categories];
        public TimeSpan Lifespan { get; } = lifespan;
    }
}
