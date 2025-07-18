﻿using PeachtreeBus.Queues;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;

namespace PeachtreeBus.DependencyInjection.Testing.GetEnumerableOf;

[ExcludeFromCodeCoverage(Justification = "Non-Shipping Test Code")]
public class QueuePipelineStep1 : IQueuePipelineStep
{
    public int Priority => throw new NotImplementedException();

    public Task Invoke(IQueueContext context, Func<IQueueContext, Task> next)
    {
        throw new NotImplementedException();
    }
}

[ExcludeFromCodeCoverage(Justification = "Non-Shipping Test Code")]
public class QueuePipelineStep2 : IQueuePipelineStep
{
    public int Priority => throw new NotImplementedException();

    public Task Invoke(IQueueContext context, Func<IQueueContext, Task> next)
    {
        throw new NotImplementedException();
    }
}

public class GetEnumerableOfIQueuePipelineStep_Fixture<TContainer>(ContainerBuilder<TContainer> containerBuilder)
    : GetEnumerableOfService_FixtureBase<IQueuePipelineStep, TContainer>(containerBuilder)
{
    protected override IEnumerable<Type> GetTypesToRegister() =>
    [
        typeof(QueuePipelineStep1),
        typeof(QueuePipelineStep2),
    ];
}

