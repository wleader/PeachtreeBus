using Microsoft.Extensions.DependencyInjection;
using PeachtreeBus.ClassNames;
using PeachtreeBus.DatabaseSharing;
using System;
using System.Collections.Generic;

namespace PeachtreeBus.MicrosoftDependencyInjection;

public class MSDIRegisterComponents(IServiceCollection services) : BaseRegisterComponents
{
    protected override void RegisterSpecialized()
    {
        RegisterSingleton<IWrappedScopeFactory, MSDIWrappedScopeFactory>();
        RegisterScoped<IWrappedScope, MSDIWrappedScope>();

        services.AddScoped(sp => sp.GetRequiredService<IShareObjectsBetweenScopes>().SharedDatabase ??=
            new SharedDatabase(sp.GetRequiredService<ISqlConnectionFactory>()));

        services.AddSingleton<ClassNameService>();
        services.AddSingleton<IClassNameService>(sp => new CachedClassNameService(sp.GetRequiredService<ClassNameService>()));
    }

    protected override void RegisterLogging() { }

    protected override void RegisterInstance<T>(T instance) where T : class =>
        services.AddSingleton(instance);

    protected override void RegisterSingleton<TInterface, TImplementation>() =>
        services.AddSingleton(typeof(TInterface), typeof(TImplementation));

    protected override void RegisterScoped<TInterface, TImplementation>() =>
        services.AddScoped(typeof(TInterface), typeof(TImplementation));

    protected override void RegisterScoped(Type interfaceType, IEnumerable<Type> implementations)
    {
        foreach (var implementationType in implementations)
        {
            services.Add(new ServiceDescriptor(interfaceType, implementationType, ServiceLifetime.Scoped));
        }
    }
}
