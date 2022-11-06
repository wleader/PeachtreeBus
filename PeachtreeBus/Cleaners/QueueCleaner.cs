using PeachtreeBus.Data;
using PeachtreeBus.Queues;
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
    public class QueueCleaner : IQueueCleaner
    {
        private readonly IBusDataAccess _dataAccess;
        private readonly IQueueCleanerConfiguration _qconfig;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="qconfig">Controls with Queue will be cleaned.</param>
        /// <param name="dataAccess">Provides access to the data store.</param>
        public QueueCleaner(IQueueCleanerConfiguration qconfig,
            IBusDataAccess dataAccess)
        {
            _qconfig = qconfig;
            _dataAccess = dataAccess;
        }

        /// <summary>
        /// Cleans completed messages for the configured queue.
        /// </summary>
        /// <param name="olderthan">Time since the message completed to be eligible for cleanup.</param>
        /// <param name="maxCount">Maximum number of completed message to cleanup. Keeps DB transaction size small.</param>
        /// <returns>The number of messages removed from the data store.</returns>
        public async Task<long> CleanCompleted(DateTime olderthan, int maxCount)
        {
            return await _dataAccess.CleanQueueCompleted(_qconfig.QueueName, olderthan, maxCount);
        }

        /// <summary>
        /// Cleans failed messages for the configured queue.
        /// </summary>
        /// <param name="olderthan">Time since the message failed to be eligble for cleanup.</param>
        /// <param name="maxCount">Maximum number of failed message to cleanup. Keeps DB transaction size small.</param>
        /// <returns>The number of messages removed from the data store.</returns>
        public async Task<long> CleanFailed(DateTime olderthan, int maxCount)
        {
            return await _dataAccess.CleanQueueFailed(_qconfig.QueueName, olderthan, maxCount);
        }
    }
}
