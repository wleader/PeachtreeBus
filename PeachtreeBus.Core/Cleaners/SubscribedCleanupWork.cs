namespace PeachtreeBus.Cleaners
{
    /// <summary>
    /// Describes A unit of work that cleans up subscribed messages
    /// </summary>
    public interface ISubscribedCleanupWork : IUnitOfWork { }

    /// <summary>
    /// A unit of work that cleans up subscribed messages
    /// </summary>
    public class SubscribedCleanupWork(
        IBusConfiguration config,
        ISystemClock clock,
        ISubscribedCleaner cleaner)
        : BaseCleanupWork(config.SubscriptionConfiguration, clock, cleaner)
        , ISubscribedCleanupWork
    { }
}
