using PeachtreeBus.Data;
using PeachtreeBus.Example.Messages;
using System;
using System.Threading.Tasks;

namespace PeachtreeBus.Example.Startup
{
    public class SendStartupMessage : IRunOnStartup
    {
        private readonly IQueueWriter _queueWriter;
        private readonly IBusDataAccess _dataAccess;

        public SendStartupMessage(IQueueWriter queueWriter, IBusDataAccess busDataAccess)
        {
            _queueWriter = queueWriter;
            _dataAccess = busDataAccess;
        }

        public Task Run()
        {
            const string QueueName = "SampleQueue";

            _dataAccess.BeginTransaction();
            _queueWriter.WriteMessage(QueueName, new SampleSagaStart { SagaId = new Random().Next(100000) });
            _dataAccess.CommitTransaction();

            return Task.CompletedTask;
        }
    }
}
