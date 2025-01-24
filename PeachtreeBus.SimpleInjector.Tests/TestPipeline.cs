using PeachtreeBus.Queues;
using PeachtreeBus.Subscriptions;
using System.Diagnostics.CodeAnalysis;

namespace PeachtreeBus.SimpleInjector.Tests;

[ExcludeFromCodeCoverage] // this is just something for the extensions to find.
public class TestSubscribedPipelineStep : ISubscribedPipelineStep
{
    public int Priority => 0;

    public Task Invoke(SubscribedContext context, Func<SubscribedContext, Task> next)
    {
        return next(context);
    }
}

[ExcludeFromCodeCoverage] // this is just something for the extensions to find.
public class TestQueuePipelineStep : IQueuePipelineStep
{
    public int Priority => 0;

    public Task Invoke(QueueContext context, Func<QueueContext, Task> next)
    {
        return next(context);
    }
}
