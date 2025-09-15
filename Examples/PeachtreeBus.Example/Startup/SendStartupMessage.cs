using PeachtreeBus.DatabaseSharing;
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
        ISharedDatabase database)
        : IRunOnStartup
    {
        private readonly IQueueWriter _queueWriter = queueWriter;
        private readonly ISharedDatabase _database = database;

        private static readonly QueueName _queueName = new("SampleQueue");

        public Task Run() => SendSampleSagaStarts(10);

        private async Task SendSampleSagaStarts(int count)
        {
            // Sends a few Saga Start messages to kick off the processing of messages in the example program.
            if (count < 1) return;
            _database.BeginTransaction();
            for(var i = 0; i < count; i++)
            {
                await _queueWriter.WriteMessage(_queueName, new SampleSagaStart { AppId = Guid.NewGuid() });
            }
            _database.CommitTransaction();
        }
    }
}
