using PeachtreeBus.Interfaces;
using System;
using System.Threading.Tasks;

namespace PeachtreeBus.Cleaners
{
    /// <summary>
    /// Describes code that can clean messages (Queues or Subscribed)
    /// </summary>
    public interface IBaseCleaner
    {
        /// <summary>
        /// Removes completed messages from the data store.
        /// </summary>
        /// <param name="olderthan">Messages completed before this time will be eligible for cleanup.</param>
        /// <param name="maxCount">Maximum number of message to clean. Keeps DB transactions small.</param>
        /// <returns>The number of messages removed from the data store.</returns>
        Task<long> CleanCompleted(DateTime olderthan, int maxCount);

        /// <summary>
        /// Removes failed messages from the data store.
        /// </summary>
        /// <param name="olderthan">Messages failed before this time will be eligible for cleanup.</param>
        /// <param name="maxCount">Maximum number of message to clean. Keeps DB transactions small.</param>
        /// <returns>The number of messages removed from the data store.</returns>
        Task<long> CleanFailed(DateTime olderthan, int maxCount);
    }

    /// <summary>
    /// A unit of work to be called from a looping thread that
    /// uses a IBaseCleaner to clean messages.
    /// </summary>
    /// <remarks>
    /// Constructor
    /// </remarks>
    /// <param name="config">Provides configuration.</param>
    /// <param name="clock">Provides acess to the clock.</param>
    /// <param name="cleaner">Performs the cleanup.</param>
    public class BaseCleanupWork(
        IBaseCleanupConfiguration config,
        ISystemClock clock,
        IBaseCleaner cleaner)
        : IUnitOfWork
    {
        protected readonly IBaseCleanupConfiguration _config = config;
        protected readonly ISystemClock _clock = clock;
        private readonly IBaseCleaner _cleaner = cleaner;

        /// <summary>
        /// Tracks the last time a cleanup occured.
        /// Used to keep the cleanup from running too often.
        /// </summary>
        public DateTime NextClean { get; private set; } = DateTime.MinValue;

        /// <summary>
        /// Called from a looping thread.
        /// </summary>
        /// <returns>True if there is more cleanup to do.</returns>
        public async Task<bool> DoWork()
        {
            // if we not scheduled to clean yet, return false
            // so the loop will rollback and sleep.
            if (_clock.UtcNow < NextClean) return false;

            long deleted = 0;
            var olderthan = _clock.UtcNow.Subtract(_config.AgeLimit);

            if (_config.CleanCompleted)
            {
                deleted += await _cleaner.CleanCompleted(olderthan, _config.MaxDeleteCount);
                // we deleted the max amount, we want to runt he loop again and delete more
                // in another transaction.
                // so do not update the NextClean time.
                // Return true because we want to commit the transaction.
                if (deleted >= _config.MaxDeleteCount) return true;
            }

            if (_config.CleanFailed)
            {
                deleted += await _cleaner.CleanFailed(olderthan, _config.MaxDeleteCount - (int)deleted);
                // we deleted the max amount, we want to runt he loop again and delete more
                // in another transaction.
                // so do not update the NextClean time.
                // Return true because we want to commit the transaction.
                if (deleted >= _config.MaxDeleteCount) return true;
            }

            if (deleted < _config.MaxDeleteCount)
            {
                // cause the cleanup process to go to sleep for a while.
                NextClean = _clock.UtcNow.Add(_config.Interval);
            }

            // if we deleted something, then commit the transaction,
            // otherwise rollback and sleep.
            return deleted > 0;
        }
    }
}
