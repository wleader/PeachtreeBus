using PeachtreeBus.Example.Data;
using PeachtreeBus.Example.Messages;
using System.Threading.Tasks;

namespace PeachtreeBus.Example.Handlers
{
    public class SagaCompletedHandler : IHandleMessage<SampleSagaComplete>
    {
        private readonly ILog _log;
        private readonly IExampleDataAccess _dataAccess;

        public SagaCompletedHandler(ILog<SagaCompletedHandler> log, IExampleDataAccess dataAccess)
        {
            _log = log;
            _dataAccess = dataAccess;
        }

        public Task Handle(MessageContext context, SampleSagaComplete message)
        {
            _log.Info("Distributed Saga Complete!");
            return _dataAccess.Audit("Example Saga Completed.");
        }
    }
}
