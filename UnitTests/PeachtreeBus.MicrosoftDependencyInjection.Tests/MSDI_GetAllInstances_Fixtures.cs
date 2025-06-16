using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using PeachtreeBus.DependencyInjection.Testing.GetAllInstances;
using System;
using System.Collections.Generic;

namespace PeachtreeBus.MicrosoftDependencyInjection.Tests;

public class MSDI_ContainerBuilder : ContainerBuilder<IServiceCollection>
{
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

    public override void AddRegistrations<TInterface>(IServiceCollection container, IEnumerable<Type> concreteTypes)
    {
        foreach (var t in concreteTypes)
        {
            container.Add(new ServiceDescriptor(typeof(TInterface), t, ServiceLifetime.Scoped));
        }
    }
}

[TestClass]
public class MSDI_GetAllInstances_QueueHandlers_Fixture()
    : GetAllInstances_QueueHandlers_Fixture<IServiceCollection>(new MSDI_ContainerBuilder());

[TestClass]
public class MSDI_GetAllInstances_SubscribedHandlers_Fixture()
    : GetAllInstances_SubscribedHandlers_Fixture<IServiceCollection>(new MSDI_ContainerBuilder());

[TestClass]
public class MSDI_GetAllInstances_QueuePipelineSteps_Fixture()
    : GetAllInstances_QueuePipelineSteps_Fixture<IServiceCollection>(new MSDI_ContainerBuilder());

[TestClass]
public class MSDI_GetAllInstances_SendPipelineSteps_Fixture()
    : GetAllInstances_SendPipelineSteps_Fixture<IServiceCollection>(new MSDI_ContainerBuilder());

[TestClass]
public class MSDI_GetAllInstances_PublishPipelineSteps_Fixture()
    : GetAllInstances_PublishPipelineSteps_Fixture<IServiceCollection>(new MSDI_ContainerBuilder());

[TestClass]
public class MSDI_GetAllInstances_SubscribedPipelineSteps_Fixture()
    : GetAllInstances_SubscribedPipelineSteps_Fixture<IServiceCollection>(new MSDI_ContainerBuilder());

