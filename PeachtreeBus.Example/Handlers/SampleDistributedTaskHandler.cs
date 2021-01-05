using PeachtreeBus.Example.Messages;
using System;
using System.Threading.Tasks;

namespace PeachtreeBus.Example.Handlers
{
    public class SampleDistributedTaskHandler : IHandleMessage<SampleDistributedTaskRequest>
    {
        private readonly ILog _log;
        public SampleDistributedTaskHandler(ILog<SampleDistributedTaskHandler> log)
        {
            _log = log;
        }

        public Task Handle(MessageContext context, SampleDistributedTaskRequest message)
        {
            _log.Info("Processing Distributed Task.");

            if (message.Operation == "+")
            {
                var response = new SampleDistributedTaskResponse
                {
                    SagaId = message.SagaId,
                    A = message.A,
                    B = message.B,
                    Operation = message.Operation,
                    Result = message.A + message.B
                };
                context.Send(response);

                _log.Info($"Distrbuted Task Result {response.A} { response.Operation} {response.B} =  {response.Result}");
            }
            else
            {
                throw new ApplicationException($"I only know how to add!. I don't know how to {message.Operation}.");
            }

            return Task.CompletedTask;
        }
    }
}
