using PeachtreeBus.Data;
using System;
using System.Threading.Tasks;

namespace PeachtreeBus.Cleaners
{
    /// <summary>
    /// A Cleaner that cleans subscribed messages.
    /// </summary>
    public interface ISubscribedCleaner : IBaseCleaner { }

    /// <summary>
    /// A Default implmentation of ISubscribedCleaner
    /// </summary>
    public class SubscribedCleaner : ISubscribedCleaner
    {
        private readonly IBusDataAccess _dataAccess;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="dataAccess">Provides access to the data store.</param>
        public SubscribedCleaner(IBusDataAccess dataAccess)
        {
            _dataAccess = dataAccess;
        }

        /// <summary>
        /// Cleans completed subscribed messages
        /// </summary>
        /// <param name="olderthan">Message that compelted before this time will be cleaned.</param>
        /// <param name="maxCount">Maximum number of messages to clean in an operation.
        /// Keeps database transaction small.</param>
        /// <returns></returns>
        public Task<long> CleanCompleted(DateTime olderthan, int maxCount)
        {
            return _dataAccess.CleanSubscribedCompleted(olderthan, maxCount);
        }

        public Task<long> CleanFailed(DateTime olderthan, int maxCount)
        {
            return _dataAccess.CleanSubscribedFailed(olderthan, maxCount);
        }
    }
}
