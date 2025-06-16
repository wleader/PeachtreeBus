using PeachtreeBus.Queues;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;

namespace PeachtreeBus.DependencyInjection.Testing.GetAllInstances;

[ExcludeFromCodeCoverage(Justification = "Non-Shipping Test Code")]
public class SendPipelineStep1 : ISendPipelineStep
{
    public int Priority => throw new NotImplementedException();

    public Task Invoke(ISendContext context, Func<ISendContext, Task> next)
    {
        throw new NotImplementedException();
    }
}

[ExcludeFromCodeCoverage(Justification = "Non-Shipping Test Code")]
public class SendPipelineStep2 : ISendPipelineStep
{
    public int Priority => throw new NotImplementedException();

    public Task Invoke(ISendContext context, Func<ISendContext, Task> next)
    {
        throw new NotImplementedException();
    }
}

public class GetAllInstances_SendPipelineSteps_Fixture<TContainer>(ContainerBuilder<TContainer> containerBuilder)
    : GetAllInstances_Base_Fixture<ISendPipelineStep, TContainer>(containerBuilder)
{
    protected override IEnumerable<Type> GetTypesToRegister() =>
    [
        typeof(SendPipelineStep1),
        typeof(SendPipelineStep2),
    ];
}

