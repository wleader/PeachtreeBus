using PeachtreeBus.Data;
using PeachtreeBus.Interfaces;
using System;
using System.Threading.Tasks;

namespace PeachtreeBus.Cleaners
{
    /// <summary>
    /// Describes a thread that cleans up expired subscriptions.
    /// </summary>
    public interface ISubscriptionCleanupThread : IThread { }

    /// <summary>
    /// A thread that cleans up expired subscriptions.
    /// </summary>
    public class SubscriptionCleanupThread : BaseThread, ISubscriptionCleanupThread
    {
        private readonly ISubscriptionCleanupWork _cleaner;
        private readonly ISystemClock _clock;
        public DateTime LastCleaned { get; set; } = DateTime.MinValue;

        public SubscriptionCleanupThread(IBusDataAccess dataAccess,
            ILog<SubscriptionCleanupThread> log,
            IProvideShutdownSignal shutdown,
            ISubscriptionCleanupWork cleaner,
            ISystemClock clock)
            : base("Subscription Cleaner", 500, log, dataAccess, shutdown)
        {
            _cleaner = cleaner;
            _clock = clock;
        }

        public override async Task<bool> DoUnitOfWork()
        {
            // don't clean too often
            if (LastCleaned.AddSeconds(15) > _clock.UtcNow) return false;

            await _cleaner.DoWork();
            LastCleaned = _clock.UtcNow;
            return true;
        }
    }
}
