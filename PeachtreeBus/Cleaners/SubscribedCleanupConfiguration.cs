using System;

namespace PeachtreeBus.Cleaners
{
    /// <summary>
    /// Provides configuration to the Subscribed Cleaner
    /// </summary>
    public interface ISubscribedCleanupConfiguration : IBaseCleanupConfiguration { }

    /// <summary>
    /// A default implementation of ISubscribedCleanupConfiguration
    /// </summary>
    public class SubscribedCleanupConfiguration(
        int maxDeleteCount,
        bool cleanCompleted,
        bool cleanFailed,
        TimeSpan ageLimit,
        TimeSpan interval)
        : BaseCleanupConfiguration(maxDeleteCount, cleanCompleted, cleanFailed, ageLimit, interval)
        , ISubscribedCleanupConfiguration
    { }
}
