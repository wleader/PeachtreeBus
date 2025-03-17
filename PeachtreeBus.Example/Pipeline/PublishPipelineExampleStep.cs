using Microsoft.Extensions.Logging;
using PeachtreeBus.Subscriptions;
using System;
using System.Threading.Tasks;

namespace PeachtreeBus.Example.Pipeline;

public class PublishPipelineExampleStep(
    ILogger<PublishPipelineExampleStep> log)
    : IPublishPipelineStep
{
    private readonly ILogger<PublishPipelineExampleStep> _log = log;

    public int Priority => 1;

    public async Task Invoke(IPublishContext context, Func<IPublishContext, Task> next)
    {
        _log.LogInformation("This code runs before publishing a subscribed message.");

        await next.Invoke(context);

        _log.LogInformation("This code runs after publishing a subscribed message.");
    }
}
