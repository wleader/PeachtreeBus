using Microsoft.Extensions.Logging;
using PeachtreeBus.Data;
using System.Threading.Tasks;

namespace PeachtreeBus.Cleaners
{
    /// <summary>
    /// Describes a thread that cleans up subscribed messages.
    /// </summary>
    public interface ISubscribedCleanupThread : IThread { }

    /// <summary>
    /// A thread that cleans up subscribed messages.
    /// </summary>
    public class SubscribedCleanupThread : BaseThread, ISubscribedCleanupThread
    {
        private readonly ISubscribedCleanupWork _cleaner;

        public SubscribedCleanupThread(ILogger<SubscribedCleanupThread> log, 
            IBusDataAccess dataAccess,
            IProvideShutdownSignal shutdown,
            ISubscribedCleanupWork cleaner) 
            : base("SubscriptionCleaner", 500, log, dataAccess, shutdown)
        {
            _cleaner = cleaner;
        }

        public override async Task<bool> DoUnitOfWork()
        {
           return await _cleaner.DoWork();
        }
    }
}
