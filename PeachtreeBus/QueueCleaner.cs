using PeachtreeBus.Data;
using PeachtreeBus;
using System;
using System.Threading.Tasks;

namespace Jukebox2.Bus
{

    /// <summary>
    /// A task that is run on a regular interval, and moves completed and failed messages from the queue.
    /// </summary>
    public class QueueCleaner : IRunOnIntervalTask
    {
        private readonly IBusDataAccess _dataAccess;
        private readonly ILog<QueueCleaner> _log;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="dataAccess">Provides access to the queue data store.</param>
        /// <param name="log"></param>
        public QueueCleaner(
            IBusDataAccess dataAccess,
            ILog<QueueCleaner> log)
        {
            _log = log;
            _dataAccess = dataAccess;
            Name = "QueueCleaner";
            SuccessWaitMs = 30000; // Wait 30 seconds before running again when no error was detected.
            ErrorWaitMs = 30000; // Wait 30 seconds before running again if an error was detected.
        }

        /// <inheritdoc/>
        public string Name { get; private set; }

        /// <inheritdoc/>
        public int SuccessWaitMs { get; private set; }

        /// <inheritdoc/>
        public int ErrorWaitMs { get; private set; }

        public Task Run()
        {
            // Need to findout what happens when the Data Access looses its connection.
            // might need some reconnection functionality?

            _dataAccess.ClearChangeTracker();
            _dataAccess.BeginTransaction();
            try
            {
                // this could be improved to clean again instantly and only "sleep"
                // when the message count returned is 0
                // by setting the SuccessWaitMs.

                var messageCount = _dataAccess.CleanQueueMessages();
                _dataAccess.CommitTransaction();
                if (messageCount > 0)
                { _log.Info($"Cleaned {messageCount} messages."); }
            }
            catch (Exception ex)
            {
                _dataAccess.RollbackTransaction();
                _log.Error(ex);
                throw;
            }
            return Task.CompletedTask;
        }
    }
}
