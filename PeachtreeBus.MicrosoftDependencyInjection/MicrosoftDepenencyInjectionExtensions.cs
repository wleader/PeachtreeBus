using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.Collections.Generic;
using System.Reflection;

namespace PeachtreeBus.MicrosoftDependencyInjection;

public static class MicrosoftDependencyInjectionExtensions
{
    public static HostApplicationBuilder HostPeachtreeBus(this HostApplicationBuilder builder)
    {
        // this is the background service that exposes the bus as an IHostedService
        builder.Services.AddHostedService<PeachtreeBusHostedService>();
        return builder;
    }

    public static IServiceCollection AddPeachtreeBus(
        this IServiceCollection builder,
        IBusConfiguration busConfiguration,
        IRegisterBusDataAccess registerDataAccess,
        List<Assembly>? assemblies = null)
    {
        var provider = new MSDIRegistrationProvider(builder);
        var components = new RegisterComponents(provider);
        components.Register(busConfiguration, assemblies);
        registerDataAccess.Register(provider);
        return builder;
    }
}
