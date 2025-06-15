using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using PeachtreeBus.DependencyInjection.Tests;
using System;
using System.Linq;

namespace PeachtreeBus.MicrosoftDependencyInjection.Tests;

[TestClass]
public class MSDI_RegisterComponentsFixture : BaseRegisterComponentsFixture<IServiceCollection>
{
    public override IWrappedScope BuildScope()
    {
        var serviceCollection = new ServiceCollection();
        serviceCollection.AddPeachtreeBus(BusConfiguration, TestAssemblies);
        serviceCollection.AddLogging(b => b.AddSimpleConsole());

        AddToContainer?.Invoke(serviceCollection);

        var provider = serviceCollection.BuildServiceProvider();

        var factory = provider.GetRequiredService<IWrappedScopeFactory>();
        return factory.Create();
    }

    public override void Then_GetServiceFails<TService>()
    {
        using var scope = BuildScope();
        Assert.ThrowsExactly<InvalidOperationException>(() => scope.GetService<TService>());
    }

    public override void AddInstance<TInterface>(IServiceCollection container, TInterface instance)
    {
        container.AddSingleton(typeof(TInterface), instance!);
    }

    protected override void Then_GetHandlersFails<THandler>(IWrappedScope scope) where THandler : class
    {
        var actual = scope.GetAllInstances<THandler>();
        Assert.IsNotNull(actual);
        Assert.IsFalse(actual.Any());
    }
}
