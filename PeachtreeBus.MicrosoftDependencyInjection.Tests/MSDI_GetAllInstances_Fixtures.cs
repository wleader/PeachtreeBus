using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using PeachtreeBus.DependencyInjection.Tests.GetAllInstances;

namespace PeachtreeBus.MicrosoftDependencyInjection.Tests;

public class MSDI_ContainerBuilder : ContainerBuilder<IServiceCollection>
{
    public override void AddRegistrations<TInterface>(IServiceCollection container, IEnumerable<Type> concreteTypes)
    {
        foreach (var t in concreteTypes)
        {
            container.Add(new ServiceDescriptor(typeof(TInterface), t, ServiceLifetime.Scoped));
        }
    }

    public override IWrappedScope CreateScope(Action<IServiceCollection>? addRegistrations = null)
    {
        var builder = Host.CreateApplicationBuilder([]);
        builder.ConfigureContainer(
            new DefaultServiceProviderFactory(
                new ServiceProviderOptions
                {
                    ValidateOnBuild = true,
                    ValidateScopes = true
                }));

        builder.Services.AddSingleton<IWrappedScopeFactory, MSDIWrappedScopeFactory>();
        builder.Services.AddScoped(typeof(IWrappedScope), typeof(MSDIWrappedScope));

        addRegistrations?.Invoke(builder.Services);

        var provider = builder.Services.BuildServiceProvider();
        var factory = provider.GetRequiredService<IWrappedScopeFactory>();
        return factory.Create();
    }
}

[TestClass]
public class MSDI_GetAllInstances_QueueHandlers_Fixture()
    : GetAllInstances_QueueHandlers_Fixture<IServiceCollection>(new MSDI_ContainerBuilder());

[TestClass]
public class MSDI_GetAllInstances_SubscribedHandlers_Fixture()
    : GetAllInstances_SubscribedHandlers_Fixture<IServiceCollection>(new MSDI_ContainerBuilder());

