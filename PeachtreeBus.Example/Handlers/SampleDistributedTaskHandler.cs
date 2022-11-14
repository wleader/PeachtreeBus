using Microsoft.Extensions.Logging;
using PeachtreeBus.Example.Data;
using PeachtreeBus.Example.Messages;
using PeachtreeBus.Queues;
using System;
using System.Threading.Tasks;

namespace PeachtreeBus.Example.Handlers
{
    /// <summary>
    /// An example handler for the SampleDistributedTaskRequest message
    /// </summary>
    public class SampleDistributedTaskHandler : IHandleQueueMessage<SampleDistributedTaskRequest>
    {
        private readonly ILogger _log;
        private readonly IExampleDataAccess _dataAccess;
        private readonly IQueueWriter _queueWriter;

        public SampleDistributedTaskHandler(
            ILogger<SampleDistributedTaskHandler> log,
            IExampleDataAccess dataAccess,
            IQueueWriter queueWriter)
        {
            _log = log;
            _dataAccess = dataAccess;
            _queueWriter = queueWriter;
        }

        public async Task Handle(QueueContext context, SampleDistributedTaskRequest message)
        {
            // This handler does math on two values.
            // Not a realistic example, in that you wouldn't go through all the trouble of sending
            // a message to add numbers. The point is to demontrate assigning a task to be completed
            // by code which may be running in a different process or even on a different machine.

            _log.ProcessingDistributedTask();
            if (message.Operation != "+")
                throw new ApplicationException($"I only know how to add!. I don't know how to {message.Operation}.");

            // compute the response.
            var response = new SampleDistributedTaskResponse
            {
                AppId = message.AppId,
                A = message.A,
                B = message.B,
                Operation = message.Operation,
                Result = message.A + message.B
            };
            // send a response message
            await _queueWriter.WriteMessage(context.SourceQueue, response);

            // demonstrate interacting with our application data.
            _log.DistributedTaskResult(response.A, response.Operation, response.B, response.Result);
            await _dataAccess.Audit($"Distributed Task Result: {response.A} {response.Operation} {response.B} = {response.Result}");
        }
    }
}
