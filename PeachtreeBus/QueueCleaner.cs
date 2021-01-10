using PeachtreeBus.Data;
using System;
using System.Threading.Tasks;

namespace PeachtreeBus
{
    public interface IConfigureQueueCleaner
    {
        string[] QueueNames { get; }
    }

    public class ConfigureQueueCleaner : IConfigureQueueCleaner
    {
        public string[] QueueNames { get; private set; }

        public ConfigureQueueCleaner(params string[] queueNames)
        {
            QueueNames = queueNames;
        }
    }

    /// <summary>
    /// A task that is run on a regular interval, and moves completed and failed messages from the queue.
    /// </summary>
    public class QueueCleaner : IRunOnIntervalTask
    {
        private readonly IBusDataAccess _dataAccess;
        private readonly ILog<QueueCleaner> _log;
        private readonly IConfigureQueueCleaner _configureQueueCleaner;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="dataAccess">Provides access to the queue data store.</param>
        /// <param name="log"></param>
        public QueueCleaner(
            IBusDataAccess dataAccess,
            ILog<QueueCleaner> log,
            IConfigureQueueCleaner configureQueueCleaner)
        {
            _log = log;
            _dataAccess = dataAccess;
            _configureQueueCleaner = configureQueueCleaner;
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

            foreach (var queueName in _configureQueueCleaner.QueueNames)
            {
                _dataAccess.BeginTransaction();
                try
                {
                    // this could be improved to clean again instantly and only "sleep"
                    // when the message count returned is 0
                    // by setting the SuccessWaitMs.

                    var messageCount = _dataAccess.CleanQueueMessages(queueName);
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
            }
            return Task.CompletedTask;
        }
    }
}
