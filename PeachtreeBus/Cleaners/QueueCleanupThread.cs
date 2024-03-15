using Microsoft.Extensions.Logging;
using PeachtreeBus.Data;
using System.Threading.Tasks;

namespace PeachtreeBus.Cleaners
{
    /// <summary>
    /// A background thread that cleans a queue.
    /// </summary>
    public interface IQueueCleanupThread : IThread { }

    /// <summary>
    /// A default implementation of IQueueCleanupThread.
    /// Calls an IQueueCleanupWork in a loop.
    /// </summary>
    public class QueueCleanupThread(
        ILogger<QueueCleanupThread> log,
        IBusDataAccess dataAccess,
        IProvideShutdownSignal shutdown,
        IQueueCleanupWork cleaner)
        : BaseThread("QueueCleaner", 500, log, dataAccess, shutdown)
        , IQueueCleanupThread
    {
        private readonly IQueueCleanupWork _cleaner = cleaner;

        public override async Task<bool> DoUnitOfWork()
        {
            return await _cleaner.DoWork();
        }
    }
}
