using PeachtreeBus.Queues;
using PeachtreeBus.Subscriptions;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;

namespace PeachtreeBus.SimpleInjector.Tests;

[ExcludeFromCodeCoverage] // this is just something for the extensions to find.
public class TestSubscribedPipelineStep : ISubscribedPipelineStep
{
    public int Priority => 0;

    public Task Invoke(ISubscribedContext context, Func<ISubscribedContext, Task> next)
    {
        return next(context);
    }
}

[ExcludeFromCodeCoverage] // this is just something for the extensions to find.
public class TestQueuePipelineStep : IQueuePipelineStep
{
    public int Priority => 0;

    public Task Invoke(IQueueContext context, Func<IQueueContext, Task> next)
    {
        return next(context);
    }
}
