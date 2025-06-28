using PeachtreeBus.Subscriptions;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;

namespace PeachtreeBus.DependencyInjection.Testing.GetEnumerableOf;

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
public class GetEnumerableOfISubscribedPipelineStep_Fixture<TContainer>(ContainerBuilder<TContainer> containerBuilder)
    : GetEnumerableOfService_FixtureBase<ISubscribedPipelineStep, TContainer>(containerBuilder)
{
    protected override IEnumerable<Type> GetTypesToRegister() =>
    [
        typeof(SubscribedPipelineStep1),
        typeof(SubscribedPipelineStep2),
    ];
}

