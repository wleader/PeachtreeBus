using Microsoft.Extensions.Logging;
using PeachtreeBus.Queues;
using System;
using System.Threading.Tasks;

namespace PeachtreeBus.Example.Pipeline
{
    internal class QueuePipelineExampleStep : IQueuePipelineStep
    {
        private readonly ILogger<QueuePipelineExampleStep> _log;

        public QueuePipelineExampleStep(ILogger<QueuePipelineExampleStep> log)
        {
            _log = log;
        }

        public int Priority => 1;

        public async Task Invoke(QueueContext context, Func<QueueContext, Task> next)
        {
            _log.LogInformation("This code runs before Queue Message handlers.");

            await next.Invoke(context);

            _log.LogInformation("This code runs after Queue Message handlers.");
        }
    }
}
