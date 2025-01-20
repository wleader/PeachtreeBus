using PeachtreeBus.Interfaces;

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
        ISubscribedCleanupConfiguration config,
        ISystemClock clock,
        ISubscribedCleaner cleaner)
        : BaseCleanupWork(config, clock, cleaner)
        , ISubscribedCleanupWork
    { }
}
