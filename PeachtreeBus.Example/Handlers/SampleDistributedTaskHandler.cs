using PeachtreeBus.Example.Data;
using PeachtreeBus.Example.Messages;
using System;
using System.Threading.Tasks;

namespace PeachtreeBus.Example.Handlers
{
    public class SampleDistributedTaskHandler : IHandleMessage<SampleDistributedTaskRequest>
    {
        private readonly ILog _log;
        private readonly IExampleDataAccess _dataAccess;

        public SampleDistributedTaskHandler(ILog<SampleDistributedTaskHandler> log, IExampleDataAccess dataAccess)
        {
            _log = log;
            _dataAccess = dataAccess;
        }

        public Task Handle(MessageContext context, SampleDistributedTaskRequest message)
        {
            _log.Info("Processing Distributed Task.");
            if (message.Operation != "+")
                throw new ApplicationException($"I only know how to add!. I don't know how to {message.Operation}.");

            var response = new SampleDistributedTaskResponse
            {
                AppId = message.AppId,
                A = message.A,
                B = message.B,
                Operation = message.Operation,
                Result = message.A + message.B
            };
            context.Send(response);

            var auditMessge = $"Distrbuted Task Result {response.A} { response.Operation} {response.B} =  {response.Result}";
            _log.Info(auditMessge);
            return _dataAccess.Audit(auditMessge);

        }
    }
}
