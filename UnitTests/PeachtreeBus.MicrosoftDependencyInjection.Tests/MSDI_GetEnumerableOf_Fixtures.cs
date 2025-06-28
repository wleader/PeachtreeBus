using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using PeachtreeBus.DependencyInjection.Testing.GetEnumerableOf;
using System;
using System.Collections.Generic;

namespace PeachtreeBus.MicrosoftDependencyInjection.Tests;

public class MSDI_ContainerBuilder : ContainerBuilder<IServiceCollection>
{
    public override IServiceProviderAccessor CreateScope(Action<IServiceCollection>? addRegistrations = null)
    {
        var builder = Host.CreateApplicationBuilder([]);
        builder.ConfigureContainer(
            new DefaultServiceProviderFactory(
                new ServiceProviderOptions
                {
                    ValidateOnBuild = true,
                    ValidateScopes = true
                }));

        builder.Services.AddSingleton<IScopeFactory, MSDIScopeFactory>();
        builder.Services.AddScoped(typeof(IServiceProviderAccessor), typeof(MSDIServiceProviderAccessor));

        addRegistrations?.Invoke(builder.Services);

        var provider = builder.Services.BuildServiceProvider();
        var factory = provider.GetRequiredService<IScopeFactory>();
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
public class MSDI_GetEnumerableOfIHandleQueueMessage_Fixture()
    : GetEnumerableOfIHandleQueueMessage_Fixture<IServiceCollection>(new MSDI_ContainerBuilder());

[TestClass]
public class MSDI_GetAllInstances_SubscribedHandlers_Fixture()
    : GetEnumerableOfIHandleSubscribedMessage_Fixture<IServiceCollection>(new MSDI_ContainerBuilder());

[TestClass]
public class MSDI_GetEnumerableOfIHandleSubscribedMessage_Fixture()
    : GetEnumerableOfIQueuePipelineStep_Fixture<IServiceCollection>(new MSDI_ContainerBuilder());

[TestClass]
public class MSDI_GetAllInstances_SendPipelineSteps_Fixture()
    : GetEnumerableOfISendPipelineStep_Fixture<IServiceCollection>(new MSDI_ContainerBuilder());

[TestClass]
public class MSDI_GetEnumerableOfISendPipelineStep_Fixture()
    : GetEnumerbaleOfIPublishPipelineStep_Fixture<IServiceCollection>(new MSDI_ContainerBuilder());

[TestClass]
public class MSDI_GetEnumerableOfISubscribedPipelineStep_Fixture()
    : GetEnumerableOfISubscribedPipelineStep_Fixture<IServiceCollection>(new MSDI_ContainerBuilder());

