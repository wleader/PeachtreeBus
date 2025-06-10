using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.Collections.Generic;
using System.Reflection;

namespace PeachtreeBus.MicrosoftDependencyInjection;

public static partial class MicrosoftDepenencyInjectionExtensions
{
    public static HostApplicationBuilder HostPeachtreeBus(this HostApplicationBuilder builder)
    {
        // this is the background service that exposes the bus as an IHostedService
        builder.Services.AddHostedService<PeachtreeBusHostedService>();
        return builder;
    }

    public static IServiceCollection AddPeachtreeBus(this IServiceCollection builder, IBusConfiguration busConfiguration, List<Assembly>? assemblies = null)
    {
        var registerComponents = new MSDIRegisterComponents(builder);
        registerComponents.Register(busConfiguration, assemblies);
        return builder;
    }
}
