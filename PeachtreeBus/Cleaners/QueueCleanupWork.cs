﻿using PeachtreeBus.Interfaces;

namespace PeachtreeBus.Cleaners
{
    /// <summary>
    /// A unit of work for the IQueueCleanupThread.
    /// </summary>
    public interface IQueueCleanupWork : IUnitOfWork { }

    /// <summary>
    /// Default implmentation of IQueueCleanupWork
    /// </summary>
    public class QueueCleanupWork(
        IQueueCleanerConfiguration config,
        ISystemClock clock,
        IQueueCleaner cleaner)
        : BaseCleanupWork(config, clock, cleaner)
        , IQueueCleanupWork
    { }
}
