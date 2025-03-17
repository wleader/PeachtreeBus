using Microsoft.Extensions.Logging;
using PeachtreeBus.Queues;
using System;
using System.Threading.Tasks;

namespace PeachtreeBus.Example.Pipeline;

public class SendPipelineExampleStep(
    ILogger<SendPipelineExampleStep> log)
    : ISendPipelineStep
{
    private readonly ILogger<SendPipelineExampleStep> _log = log;

    public int Priority => 0;

    public async Task Invoke(ISendContext context, Func<ISendContext, Task> next)
    {
        _log.LogInformation("This code runs before a queue message is sent.");
        await next.Invoke(context);
        _log.LogInformation("This code runs after a queue message is sent.");
    }
}
