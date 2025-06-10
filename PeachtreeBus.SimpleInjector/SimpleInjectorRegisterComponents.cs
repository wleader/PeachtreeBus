using Microsoft.Extensions.Logging;
using PeachtreeBus.ClassNames;
using PeachtreeBus.DatabaseSharing;
using SimpleInjector;
using System;
using System.Collections.Generic;
using System.Linq;

namespace PeachtreeBus.SimpleInjector;

public class SimpleInjectorRegisterComponents(
    Container container,
    ILoggerFactory loggerFactory) : BaseRegisterComponents
{
    protected override void RegisterLogging()
    {
        container.RegisterInstance(loggerFactory);
        container.RegisterSingleton(typeof(ILogger<>), typeof(Logger<>));
    }

    protected override void RegisterSpecialized()
    {
        container.Register(typeof(IWrappedScopeFactory), () => new SimpleInjectorScopeFactory(container), Lifestyle.Singleton);
        container.Register(typeof(IWrappedScope), typeof(SimpleInjectorScope), Lifestyle.Scoped);

        var sharedDbProducer = Lifestyle.Scoped.CreateProducer<ISharedDatabase>(typeof(SharedDatabase), container);
        container.Register(typeof(ISharedDatabase),
            () => container.GetInstance<IShareObjectsBetweenScopes>().SharedDatabase ?? sharedDbProducer.GetInstance(),
            Lifestyle.Scoped);

        container.Register(typeof(IClassNameService), typeof(ClassNameService), Lifestyle.Singleton);
        container.RegisterDecorator(typeof(IClassNameService), typeof(CachedClassNameService));
    }

    protected override void RegisterInstance<T>(T instance) =>
        container.RegisterInstance<T>(instance);

    protected override void RegisterSingleton<TInterface, TImplementation>() =>
        container.Register(typeof(TInterface), typeof(TImplementation), Lifestyle.Singleton);

    protected override void RegisterScoped<TInterface, TImplementation>() =>
        container.Register(typeof(TInterface), typeof(TImplementation), Lifestyle.Scoped);

    protected override void RegisterScoped(Type interfaceType, IEnumerable<Type> implementations)
    {
        container.Collection.Register(interfaceType, implementations, Lifestyle.Scoped);
        RegisterIfNeeded(implementations);
    }

    private void RegisterIfNeeded(IEnumerable<Type> implementations)
    {
        var current = container.GetCurrentRegistrations().Select(ip => ip.ImplementationType);
        var needsRegistration = implementations.Except(current);
        foreach (var item in needsRegistration)
        {
            container.Register(item, item, Lifestyle.Scoped);
        }
    }
}
