using Microsoft.Extensions.Logging;
using PeachtreeBus.DependencyInjection.Testing;
using SimpleInjector;
using SimpleInjector.Lifestyles;
using System;
using System.Linq;

namespace PeachtreeBus.SimpleInjector.Tests;

[TestClass]
public class SimpleInjector_RegisterComponentsFixture : BaseRegisterComponentsFixture<Container>
{
    protected Container _container = default!;

    private void BuildContainer()
    {
        _container = new Container();
        _container.Options.DefaultScopedLifestyle = new AsyncScopedLifestyle();
        ILoggerFactory loggerFactory = LoggerFactory.Create(builder =>
        {
            builder.AddSimpleConsole();
        });

        _container.UsePeachtreeBus(BusConfiguration, loggerFactory, TestAssemblies);

        AddToContainer?.Invoke(_container);
    }

    public override IServiceProviderAccessor BuildAccessor()
    {
        BuildContainer();
        var factory = _container.GetRequiredService<IScopeFactory>();
        return factory.Create();
    }

    public override void Then_GetServiceFails<TService>()
    {
        BuildContainer();
        Assert.ThrowsExactly<InvalidOperationException>(_container.Verify);
    }

    public override void AddInstance<TInterface>(Container container, TInterface instance)
    {
        container.RegisterSingleton(typeof(TInterface), () => instance!);
    }

    protected override void Then_GetHandlersReturnsEmpty<THandler>(IServiceProviderAccessor accessor) where THandler : class
    {
        var actual = accessor.GetServices<THandler>();
        Assert.IsNotNull(actual);
        Assert.IsFalse(actual.Any());
    }
}
