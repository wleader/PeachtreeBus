using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using PeachtreeBus.DependencyInjection.Testing;
using System;
using System.Linq;
using Moq;
using PeachtreeBus.Data;

namespace PeachtreeBus.MicrosoftDependencyInjection.Tests;

[TestClass]
public class MSDI_RegisterComponentsFixture : BaseRegisterComponentsFixture<IServiceCollection>
{
    protected override IServiceProviderAccessor BuildAccessor()
    {
        var serviceCollection = new ServiceCollection();
        serviceCollection.AddPeachtreeBus(BusConfiguration, TestAssemblies);
        serviceCollection.AddLogging(b => b.AddSimpleConsole());
        
        // This will probably change.
        // Assumption, there will be some extension method in 
        // the data access assemblies for adding an IBusDataAccess
        var busDataAcess = new Mock<IBusDataAccess>();
        serviceCollection.AddSingleton(busDataAcess.Object);

        AddToContainer?.Invoke(serviceCollection);

        var provider = serviceCollection.BuildServiceProvider();

        var factory = provider.GetRequiredService<IScopeFactory>();
        return factory.Create();
    }

    protected override void Then_GetServiceFails<TService>()
        where TService : class
    {
        using var accessor = BuildAccessor();
        Assert.ThrowsExactly<InvalidOperationException>(() => accessor.GetRequiredService<TService>());
    }

    protected override void AddInstance<TInterface>(IServiceCollection container, TInterface instance)
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
