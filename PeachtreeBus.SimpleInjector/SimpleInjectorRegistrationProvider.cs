using Microsoft.Extensions.Logging;
using PeachtreeBus.ClassNames;
using PeachtreeBus.DatabaseSharing;
using SimpleInjector;
using System;
using System.Collections.Generic;
using System.Linq;

namespace PeachtreeBus.SimpleInjector;

public class SimpleInjectorRegistrationProvider(
    Container container,
    ILoggerFactory loggerFactory) : IRegistrationProvider
{
    public void RegisterLogging()
    {
        container.RegisterInstance(loggerFactory);
        container.RegisterSingleton(typeof(ILogger<>), typeof(Logger<>));
    }

    public void RegisterSpecialized()
    {
        container.Register(typeof(IScopeFactory), () => new SimpleInjectorScopeFactory(container), Lifestyle.Singleton);
        container.Register(typeof(IServiceProviderAccessor), typeof(SimpleInjectorServiceProviderAccessor), Lifestyle.Scoped);

        var sharedDbProducer = Lifestyle.Scoped.CreateProducer<ISharedDatabase>(typeof(SharedDatabase), container);
        container.Register(typeof(ISharedDatabase),
            () => container.GetInstance<IShareObjectsBetweenScopes>().SharedDatabase ?? sharedDbProducer.GetInstance(),
            Lifestyle.Scoped);

        container.Register(typeof(IClassNameService), typeof(ClassNameService), Lifestyle.Singleton);
        container.RegisterDecorator(typeof(IClassNameService), typeof(CachedClassNameService));
    }

    public void RegisterInstance<T>(T instance) where T : class =>
        container.RegisterInstance(instance);

    public void RegisterSingleton<TInterface, TImplementation>() =>
        container.Register(typeof(TInterface), typeof(TImplementation), Lifestyle.Singleton);

    public void RegisterScoped<TInterface, TImplementation>() =>
        container.Register(typeof(TInterface), typeof(TImplementation), Lifestyle.Scoped);

    public void RegisterScoped(Type interfaceType, List<Type> implementations)
    {
        container.Collection.Register(interfaceType, implementations, Lifestyle.Scoped);
        RegisterIfNeeded(implementations);
    }

    private void RegisterIfNeeded(List<Type> implementations)
    {
        var current = container.GetCurrentRegistrations().Select(ip => ip.ImplementationType);
        var needsRegistration = implementations.Except(current);
        foreach (var item in needsRegistration)
        {
            container.Register(item, item, Lifestyle.Scoped);
        }
    }
}
