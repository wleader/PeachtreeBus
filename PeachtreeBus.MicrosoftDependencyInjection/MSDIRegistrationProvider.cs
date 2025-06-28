using Microsoft.Extensions.DependencyInjection;
using PeachtreeBus.ClassNames;
using PeachtreeBus.DatabaseSharing;
using System;
using System.Collections.Generic;

namespace PeachtreeBus.MicrosoftDependencyInjection;

public class MSDIRegistrationProvider(IServiceCollection services) : IRegistrationProvider
{
    public void RegisterSpecialized()
    {
        RegisterSingleton<IScopeFactory, MSDIScopeFactory>();
        RegisterScoped<IServiceProviderAccessor, MSDIServiceProviderAccessor>();

        services.AddScoped(sp => sp.GetRequiredService<IShareObjectsBetweenScopes>().SharedDatabase ??=
            new SharedDatabase(sp.GetRequiredService<ISqlConnectionFactory>()));

        services.AddSingleton<ClassNameService>();
        services.AddSingleton<IClassNameService>(sp => new CachedClassNameService(sp.GetRequiredService<ClassNameService>()));
    }

    public void RegisterLogging() { }

    public void RegisterInstance<T>(T instance) where T : class =>
        services.AddSingleton(instance);

    public void RegisterSingleton<TInterface, TImplementation>() =>
        services.AddSingleton(typeof(TInterface), typeof(TImplementation));

    public void RegisterScoped<TInterface, TImplementation>() =>
        services.AddScoped(typeof(TInterface), typeof(TImplementation));

    public void RegisterScoped(Type interfaceType, List<Type> implementations)
    {
        foreach (var implementationType in implementations)
        {
            services.Add(new ServiceDescriptor(interfaceType, implementationType, ServiceLifetime.Scoped));
        }
    }
}
