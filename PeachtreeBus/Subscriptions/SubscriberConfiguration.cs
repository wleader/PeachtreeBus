using System;
using System.Collections.Generic;

namespace PeachtreeBus.Subscriptions
{
    /// <summary>
    /// Defines the subscriptions for a given process.
    /// </summary>
    public interface ISubscriberConfiguration
    {
        Guid SubscriberId { get; }
        IList<string> Categories { get; }
        TimeSpan Lifespan { get; }
    }

    /// <summary>
    /// default implementation of ISubscriberConfiguration
    /// </summary>
    public class SubscriberConfiguration(
        Guid subscriberId,
        TimeSpan lifespan,
        params string[] categories)
        : ISubscriberConfiguration
    {
        public Guid SubscriberId { get; } = subscriberId;
        public IList<string> Categories { get; } = [.. categories];
        public TimeSpan Lifespan { get; } = lifespan;
    }
}
