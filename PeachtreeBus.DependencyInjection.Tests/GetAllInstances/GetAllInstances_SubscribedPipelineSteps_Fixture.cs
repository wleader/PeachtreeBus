using PeachtreeBus.Subscriptions;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;

namespace PeachtreeBus.DependencyInjection.Tests.GetAllInstances;

[ExcludeFromCodeCoverage(Justification = "Non-Shipping Test Code")]
public class SubscribedPipelineStep1 : ISubscribedPipelineStep
{
    public int Priority => throw new NotImplementedException();

    public Task Invoke(ISubscribedContext context, Func<ISubscribedContext, Task> next)
    {
        throw new NotImplementedException();
    }
}

[ExcludeFromCodeCoverage(Justification = "Non-Shipping Test Code")]
public class SubscribedPipelineStep2 : ISubscribedPipelineStep
{
    public int Priority => throw new NotImplementedException();

    public Task Invoke(ISubscribedContext context, Func<ISubscribedContext, Task> next)
    {
        throw new NotImplementedException();
    }
}

[TestClass]
public class GetAllInstances_SubscribedPipelineSteps_Fixture<TContainer>(ContainerBuilder<TContainer> containerBuilder)
    : GetAllInstances_Base_Fixture<ISubscribedPipelineStep, TContainer>(containerBuilder)
{
    protected override IEnumerable<Type> GetTypesToRegister() =>
    [
        typeof(SubscribedPipelineStep1),
        typeof(SubscribedPipelineStep2),
    ];
}

