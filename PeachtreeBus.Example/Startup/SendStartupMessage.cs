using PeachtreeBus.Data;
using PeachtreeBus.Example.Messages;
using PeachtreeBus.Queues;
using System;
using System.Threading.Tasks;

namespace PeachtreeBus.Example.Startup
{
    /// <summary>
    /// A sample startup task. This will be run once when the process starts.
    /// </summary>
    public class SendStartupMessage(
        IQueueWriter queueWriter,
        IBusDataAccess busDataAccess)
        : IRunOnStartup
    {
        private readonly IQueueWriter _queueWriter = queueWriter;
        private readonly IBusDataAccess _dataAccess = busDataAccess;

        private static readonly QueueName _queueName = new("SampleQueue");

        public async Task Run()
        {
            // Sends a few Saga Start messages to kick off the processing of messages in the example program.
            for (var i = 0; i < 10; i++)
            {
                _dataAccess.BeginTransaction();
                for (var j = 0; j < 10; j++)
                {
                    await _queueWriter.WriteMessage(_queueName, new SampleSagaStart { AppId = Guid.NewGuid() });
                }
                _dataAccess.CommitTransaction();
            }
        }
    }
}
