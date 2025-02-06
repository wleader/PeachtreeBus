using PeachtreeBus.Data;
using System;
using System.Threading.Tasks;

namespace PeachtreeBus.Cleaners
{
    /// <summary>
    /// A Cleaner that cleans queue messages.
    /// </summary>
    public interface IQueueCleaner : IBaseCleaner { }

    /// <summary>
    /// A Default implementation of IQueueCleaner
    /// </summary>
    /// <remarks>
    /// Constructor
    /// </remarks>
    /// <param name="config">Controls with Queue will be cleaned.</param>
    /// <param name="dataAccess">Provides access to the data store.</param>
    public class QueueCleaner(
        IBusConfiguration config,
        IBusDataAccess dataAccess)
        : IQueueCleaner
    {
        private readonly IBusDataAccess _dataAccess = dataAccess;
        private readonly QueueConfiguration? _config = config.QueueConfiguration;
        /// <summary>
        /// Cleans completed messages for the configured queue.
        /// </summary>
        /// <param name="olderthan">Time since the message completed to be eligible for cleanup.</param>
        /// <param name="maxCount">Maximum number of completed message to cleanup. Keeps DB transaction size small.</param>
        /// <returns>The number of messages removed from the data store.</returns>
        public async Task<long> CleanCompleted(DateTime olderthan, int maxCount)
        {
            if (_config is null) return 0;
            return await _dataAccess.CleanQueueCompleted(_config.QueueName, olderthan, maxCount);
        }

        /// <summary>
        /// Cleans failed messages for the configured queue.
        /// </summary>
        /// <param name="olderthan">Time since the message failed to be eligble for cleanup.</param>
        /// <param name="maxCount">Maximum number of failed message to cleanup. Keeps DB transaction size small.</param>
        /// <returns>The number of messages removed from the data store.</returns>
        public async Task<long> CleanFailed(DateTime olderthan, int maxCount)
        {
            if (_config is null) return 0;
            return await _dataAccess.CleanQueueFailed(_config.QueueName, olderthan, maxCount);
        }
    }
}
