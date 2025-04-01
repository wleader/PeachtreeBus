using PeachtreeBus.Pipelines;
using PeachtreeBus.Queues;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;

namespace PeachtreeBus.Tests;

[ExcludeFromCodeCoverage(Justification = "Test Class")]
public class TestFinalStep : PipelineFinalStep<IQueueContext>
{
    public override Task Invoke(IQueueContext context, Func<IQueueContext, Task>? next)
    {
        throw new NotImplementedException();
    }
}
