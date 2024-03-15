using System;
using System.Collections.Generic;
using System.Linq;

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
        public Guid SubscriberId { get; private set; } = subscriberId;
        public IList<string> Categories { get; private set; } = categories.ToList();
        public TimeSpan Lifespan { get; private set; } = lifespan;
    }
}
