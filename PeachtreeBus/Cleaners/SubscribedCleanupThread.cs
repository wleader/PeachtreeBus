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

        public SubscribedCleanupThread(ILog<SubscribedCleanupThread> log, 
            IBusDataAccess dataAccess,
            IProvideShutdownSignal shutdown,
            ISubscribedCleanupWork cleaner) 
            : base("SubscriptionCleaner", 500, log, dataAccess, shutdown)
        {
            _cleaner = cleaner;
        }

        public override Task<bool> DoUnitOfWork()
        {
           return _cleaner.DoWork();
        }
    }
}
