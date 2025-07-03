using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using PeachtreeBus.DependencyInjection.Testing;
using System;
using System.Linq;

namespace PeachtreeBus.MicrosoftDependencyInjection.Tests;

[TestClass]
public class MSDI_RegisterComponentsFixture : BaseRegisterComponentsFixture<IServiceCollection>
{
    public override IServiceProviderAccessor BuildAccessor()
    {
        var serviceCollection = new ServiceCollection();
        serviceCollection.AddPeachtreeBus(BusConfiguration, TestAssemblies);
        serviceCollection.AddLogging(b => b.AddSimpleConsole());

        AddToContainer?.Invoke(serviceCollection);

        var provider = serviceCollection.BuildServiceProvider();

        var factory = provider.GetRequiredService<IScopeFactory>();
        return factory.Create();
    }

    public override void Then_GetServiceFails<TService>()
    {
        using var accessor = BuildAccessor();
        Assert.ThrowsExactly<InvalidOperationException>(() => accessor.GetRequiredService<TService>());
    }

    public override void AddInstance<TInterface>(IServiceCollection container, TInterface instance)
    {
        container.AddSingleton(typeof(TInterface), instance!);
    }

    protected override void Then_GetHandlersReturnsEmpty<THandler>(IServiceProviderAccessor accessor) where THandler : class
    {
        var actual = accessor.GetServices<THandler>();
        Assert.IsNotNull(actual);
        Assert.IsFalse(actual.Any());
    }
}
