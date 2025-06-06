﻿using Microsoft.Extensions.Logging;
using PeachtreeBus.Pipelines;
using PeachtreeBus.Queues;
using System;
using System.Threading.Tasks;

namespace PeachtreeBus.Example.Pipeline
{
    internal class QueuePipelineExampleStep(
        ILogger<QueuePipelineExampleStep> log)
        : IQueuePipelineStep
    {
        private readonly ILogger<QueuePipelineExampleStep> _log = log;

        public int Priority => 1;

        public async Task Invoke(IQueueContext context, Func<IQueueContext, Task> next)
        {
            _log.LogInformation("This code runs before Queue Message handlers.");

            await next.Invoke(context);

            _log.LogInformation("This code runs after Queue Message handlers.");
        }
    }
}
