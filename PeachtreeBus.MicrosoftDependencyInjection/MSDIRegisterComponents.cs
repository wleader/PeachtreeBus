using Microsoft.Extensions.DependencyInjection;
using PeachtreeBus.DatabaseSharing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PeachtreeBus.MicrosoftDependencyInjection;

public class MSDIRegisterComponents(IServiceCollection services) : BaseRegisterComponents
{
    protected override void RegisterSpecialized()
    {
        RegisterSingleton<IWrappedScope, MSDIWrappedScope>();
        RegisterScoped<IWrappedScope, MSDIWrappedScope>();

        services.AddScoped(sp => sp.GetRequiredService<IShareObjectsBetweenScopes>().SharedDatabase ??=
            new SharedDatabase(sp.GetRequiredService<ISqlConnectionFactory>()));
    }

    protected override void RegisterInstance<T>(T instance) where T : class =>
        services.AddSingleton(instance);

    protected override void RegisterSingleton<TInterface, TImplementation>() =>
        services.AddSingleton(typeof(TInterface), typeof(TImplementation));

    protected override void RegisterScoped<TInterface, TImplementation>() =>
        services.AddScoped(typeof(TInterface), typeof(TImplementation));
}
