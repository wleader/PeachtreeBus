using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using PeachtreeBus.DependencyInjection.Tests;
using SimpleInjector;
using SimpleInjector.Lifestyles;
using System;
using ActivationException = SimpleInjector.ActivationException;

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

    public override IWrappedScope BuildScope()
    {
        BuildContainer();
        var factory = _container.GetRequiredService<IWrappedScopeFactory>();
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

    protected override void Then_GetHandlersFails<THandler>(IWrappedScope scope) where THandler : class
    {
        Assert.ThrowsExactly<ActivationException>(() => scope.GetAllInstances<THandler>());
    }
}
