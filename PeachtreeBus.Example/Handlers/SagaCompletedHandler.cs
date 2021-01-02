using PeachtreeBus.Example.Messages;
using System.Threading.Tasks;

namespace PeachtreeBus.Example.Handlers
{
    public class SagaCompletedHandler : IHandleMessage<SampleSagaComplete>
    {
        private readonly ILog _log;

        public SagaCompletedHandler(ILog<SagaCompletedHandler> log)
        {
            _log = log;
        }

        public Task Handle(MessageContext context, SampleSagaComplete message)
        {
            _log.Info("Distributed Saga Complete!");
            return Task.CompletedTask;
        }
    }
}
