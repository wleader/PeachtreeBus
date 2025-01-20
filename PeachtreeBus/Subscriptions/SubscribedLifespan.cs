using System;

namespace PeachtreeBus.Subscriptions
{
    /// <summary>
    /// defines how long a subscribed message is valid for, before 
    /// it will be discarded by the system if the subscriber does not consume it.
    /// Configuration information for SubscriptionPublisher
    /// </summary>
    public interface ISubscribedLifespan
    {
        TimeSpan Duration { get; }
    }

    /// <summary>
    /// Default implmentation of ISubscribedLifespan.
    /// </summary>
    public class SubscribedLifespan(
        TimeSpan duration)
        : ISubscribedLifespan
    {
        public TimeSpan Duration { get; } = duration;
    }
}
