﻿using PeachtreeBus.Data;
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
    public class QueueCleanupThread : BaseThread, IQueueCleanupThread
    {
        private readonly IQueueCleanupWork _cleaner;

        public QueueCleanupThread(
            ILog<QueueCleanupThread> log,
            IBusDataAccess dataAccess,
            IProvideShutdownSignal shutdown,
            IQueueCleanupWork cleaner)
            : base("QueueCleaner", 500, log, dataAccess, shutdown)
        {
            _cleaner = cleaner;
        }

        public override Task<bool> DoUnitOfWork()
        {
            return _cleaner.DoWork();
        }
    }
}