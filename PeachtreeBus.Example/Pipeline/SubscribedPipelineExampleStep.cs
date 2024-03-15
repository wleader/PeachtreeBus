using Microsoft.Extensions.Logging;
using PeachtreeBus.Subscriptions;
using System;
using System.Threading.Tasks;

namespace PeachtreeBus.Example.Pipeline
{
    internal class SubscribedPipelineExampleStep(
        ILogger<SubscribedPipelineExampleStep> log)
        : ISubscribedPipelineStep
    {
        private readonly ILogger<SubscribedPipelineExampleStep> _log = log;

        public int Priority => 1;

        public async Task Invoke(SubscribedContext context, Func<SubscribedContext, Task> next)
        {
            _log.LogInformation("This code runs before Subscribed Message handlers.");

            await next(context);

            _log.LogInformation("This code runs after Subscribed Message handlers.");
        }
    }
}
