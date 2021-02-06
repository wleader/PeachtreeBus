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

        public async Task Run()
        {

            const string QueueName = "SampleQueue";
            
            for (var i = 0; i < 10; i++)
            {
                _dataAccess.BeginTransaction();
                for (var j = 0; j < 10; j++)
                {
                    await _queueWriter.WriteMessage(QueueName, new SampleSagaStart { AppId = Guid.NewGuid() });
                }
                _dataAccess.CommitTransaction();
            }
        }
    }
}
