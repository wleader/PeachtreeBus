using Microsoft.Extensions.Logging;
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
    public class SubscriptionCleanupThread(
        IBusDataAccess dataAccess,
        ILogger<SubscriptionCleanupThread> log,
        IProvideShutdownSignal shutdown,
        ISubscriptionCleanupWork cleaner,
        ISystemClock clock)
        : BaseThread("Subscription Cleaner", 500, log, dataAccess, shutdown)
        , ISubscriptionCleanupThread
    {
        private readonly ISubscriptionCleanupWork _cleaner = cleaner;
        private readonly ISystemClock _clock = clock;
        public DateTime LastCleaned { get; set; } = DateTime.MinValue;

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
