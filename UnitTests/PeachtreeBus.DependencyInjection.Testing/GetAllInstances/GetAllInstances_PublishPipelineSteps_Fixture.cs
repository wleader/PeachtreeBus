using PeachtreeBus.Subscriptions;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;

namespace PeachtreeBus.DependencyInjection.Testing.GetAllInstances;

[ExcludeFromCodeCoverage(Justification = "Non-Shipping Test Code")]
public class PublishPipelineStep1 : IPublishPipelineStep
{
    public int Priority => throw new NotImplementedException();

    public Task Invoke(IPublishContext context, Func<IPublishContext, Task> next)
    {
        throw new NotImplementedException();
    }
}

[ExcludeFromCodeCoverage(Justification = "Non-Shipping Test Code")]
public class PublishPipelineStep2 : IPublishPipelineStep
{
    public int Priority => throw new NotImplementedException();

    public Task Invoke(IPublishContext context, Func<IPublishContext, Task> next)
    {
        throw new NotImplementedException();
    }
}

public class GetAllInstances_PublishPipelineSteps_Fixture<TContainer>(ContainerBuilder<TContainer> containerBuilder)
    : GetAllInstances_Base_Fixture<IPublishPipelineStep, TContainer>(containerBuilder)
{
    protected override IEnumerable<Type> GetTypesToRegister() =>
    [
        typeof(PublishPipelineStep1),
        typeof(PublishPipelineStep2),
    ];
}

