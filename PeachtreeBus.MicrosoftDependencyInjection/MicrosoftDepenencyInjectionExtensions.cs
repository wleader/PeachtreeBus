using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using PeachtreeBus.ClassNames;
using PeachtreeBus.Data;
using PeachtreeBus.DatabaseSharing;
using PeachtreeBus.Pipelines;
using PeachtreeBus.Queues;
using PeachtreeBus.Sagas;
using PeachtreeBus.Serialization;
using PeachtreeBus.Subscriptions;
using PeachtreeBus.Tasks;
using PeachtreeBus.Telemetry;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace PeachtreeBus.MicrosoftDependencyInjection;

public static partial class MicrosoftDepenencyInjectionExtensions
{
    private static IBusConfiguration Configuration = default!;
    private static List<Assembly> Assemblies = default!;

    public static HostApplicationBuilder HostPeachtreeBus(this HostApplicationBuilder builder)
    {
        // this is the background service that exposes the bus as an IHostedService
        builder.Services.AddHostedService<PeachtreeBusHostedService>();
        return builder;
    }

    public static IServiceCollection AddPeachtreeBus(this IServiceCollection builder, IBusConfiguration busConfiguration, List<Assembly>? assemblies = null)
    {
        Assemblies = assemblies ?? [.. AppDomain.CurrentDomain.GetAssemblies().ToList()];

        Configuration = busConfiguration;

        return builder
            .RegisterCoreComponents(busConfiguration)
            .RegisterQueuedComponents()
            .RegisterSubscribedComponents()
            .RegisterSerializer()
            .RegisterStartupTasks();
    }

    private static IServiceCollection RegisterCoreComponents(this IServiceCollection builder, IBusConfiguration busConfiguration)
    {
        builder.AddSingleton(busConfiguration);
        builder.AddSingleton<IWrappedScopeFactory, MSDIWrappedScopeFactory>();
        builder.AddScoped(typeof(IWrappedScope), typeof(MSDIWrappedScope));
        builder.AddSingleton(typeof(ITaskCounter), typeof(TaskCounter));
        builder.AddScoped(typeof(ITaskManager), typeof(TaskManager));
        builder.AddScoped(typeof(IStarters), typeof(Starters));
        builder.AddScoped(typeof(ISystemClock), typeof(SystemClock));
        builder.AddSingleton(typeof(IAlwaysRunTracker), typeof(AlwaysRunTracker));
        builder.AddScoped(typeof(IBusDataAccess), typeof(DapperDataAccess));
        builder.AddScoped(typeof(ISqlConnectionFactory), typeof(SqlConnectionFactory));
        builder.AddScoped(typeof(IProvideDbConnectionString), typeof(ProvideDbConnectionString));
        builder.AddScoped(typeof(IDapperTypesHandler), typeof(DapperTypesHandler));
        builder.AddSingleton(typeof(IMeters), typeof(Meters));
        builder.AddSingleton<ClassNameService>();
        builder.AddSingleton<IClassNameService>(sp => new CachedClassNameService(sp.GetRequiredService<ClassNameService>()));

        builder.AddScoped(typeof(IShareObjectsBetweenScopes), typeof(ShareObjectsBetweenScopes));
        // this might be wrong
        builder.AddScoped(sp => sp.GetRequiredService<IShareObjectsBetweenScopes>().SharedDatabase ??=
            new SharedDatabase(sp.GetRequiredService<ISqlConnectionFactory>()));

        builder.AddScoped(typeof(ISharedDatabase), typeof(SharedDatabase));

        return builder;
    }

    private static IServiceCollection RegisterQueuedComponents(this IServiceCollection builder)
    {
        if (Configuration.QueueConfiguration?.UseDefaultRetryStrategy ?? true)
            builder.AddSingleton(typeof(IQueueRetryStrategy), typeof(DefaultQueueRetryStrategy));

        builder.AddScoped(typeof(ICleanQueuedCompletedStarter), typeof(CleanQueuedCompletedStarter));
        builder.AddScoped(typeof(ICleanQueuedCompletedTracker), typeof(CleanQueuedCompletedTracker));
        builder.AddScoped(typeof(ICleanQueuedFailedStarter), typeof(CleanQueuedFailedStarter));
        builder.AddScoped(typeof(ICleanQueuedFailedTracker), typeof(CleanQueuedFailedTracker));
        builder.AddScoped(typeof(IProcessQueuedStarter), typeof(ProcessQueuedStarter));
        builder.AddScoped(typeof(ICleanQueuedCompletedRunner), typeof(CleanQueuedCompletedRunner));
        builder.AddScoped(typeof(ICleanQueuedCompletedTask), typeof(CleanQueuedCompletedTask));
        builder.AddScoped(typeof(ICleanQueuedFailedRunner), typeof(CleanQueuedFailedRunner));
        builder.AddScoped(typeof(ICleanQueuedFailedTask), typeof(CleanQueuedFailedTask));
        builder.AddScoped(typeof(IQueueWriter), typeof(QueueWriter));
        builder.AddScoped(typeof(ISendPipelineInvoker), typeof(SendPipelineInvoker));
        builder.AddScoped(typeof(ISendPipelineFactory), typeof(SendPipelineFactory));
        builder.AddScoped(typeof(ISendPipeline), typeof(SendPipeline));
        builder.AddScoped(typeof(ISendPipelineFinalStep), typeof(SendPipelineFinalStep));
        builder.AddScoped(typeof(IFindSendPipelineSteps), typeof(FindSendPipelineSteps));
        builder.AddScoped(typeof(IProcessQueuedRunner), typeof(ProcessQueuedRunner));
        builder.AddScoped(typeof(IProcessQueuedTask), typeof(ProcessQueuedTask));
        builder.AddScoped(typeof(IQueueReader), typeof(QueueReader));
        builder.AddScoped(typeof(IQueueFailures), typeof(QueueFailures));
        builder.AddScoped(typeof(IFailedQueueMessageHandlerFactory), typeof(FailedQueueMessageHandlerFactory));
        builder.AddScoped(typeof(IQueuePipelineInvoker), typeof(QueuePipelineInvoker));
        builder.AddScoped(typeof(IQueuePipelineFactory), typeof(QueuePipelineFactory));
        builder.AddScoped(typeof(IQueuePipeline), typeof(QueuePipeline));
        builder.AddScoped(typeof(IQueuePipelineFinalStep), typeof(QueuePipelineFinalStep));
        builder.AddScoped(typeof(IFindQueuePipelineSteps), typeof(FindQueuedPipelineSteps));
        builder.AddScoped(typeof(IFindQueueHandlers), typeof(FindQueueHandlers));
        builder.AddSingleton(typeof(ISagaMessageMapManager), typeof(SagaMessageMapManager));

        builder.FindAndRegisterMessageHandler(typeof(IHandleQueueMessage<>), typeof(IQueueMessage));

        return builder;
    }

    private static IServiceCollection RegisterSubscribedComponents(this IServiceCollection builder)
    {
        if (Configuration.SubscriptionConfiguration?.UseDefaultRetryStrategy ?? true)
            builder.AddSingleton(typeof(ISubscribedRetryStrategy), typeof(DefaultSubscribedRetryStrategy));

        builder.AddScoped(typeof(IUpdateSubscriptionsRunner), typeof(UpdateSubscriptionsRunner));
        builder.AddScoped(typeof(IUpdateSubscriptionsStarter), typeof(UpdateSubscriptionsStarter));
        builder.AddScoped(typeof(IUpdateSubscriptionsTracker), typeof(UpdateSubscriptionsTracker));
        builder.AddScoped(typeof(ICleanSubscriptionsStarter), typeof(CleanSubscriptionsStarter));
        builder.AddScoped(typeof(ICleanSubscriptionsTracker), typeof(CleanSubscriptionsTracker));
        builder.AddScoped(typeof(ICleanSubscribedPendingStarter), typeof(CleanSubscribedPendingStarter));
        builder.AddScoped(typeof(ICleanSubscribedPendingTracker), typeof(CleanSubscribedPendingTracker));
        builder.AddScoped(typeof(ICleanSubscribedCompletedStarter), typeof(CleanSubscribedCompletedStarter));
        builder.AddScoped(typeof(ICleanSubscribedCompletedTracker), typeof(CleanSubscribedCompletedTracker));
        builder.AddScoped(typeof(ICleanSubscribedFailedStarter), typeof(CleanSubscribedFailedStarter));
        builder.AddScoped(typeof(ICleanSubscribedFailedTracker), typeof(CleanSubscribedFailedTracker));
        builder.AddScoped(typeof(IProcessSubscribedStarter), typeof(ProcessSubscribedStarter));
        builder.AddScoped(typeof(IUpdateSubscriptionsTask), typeof(UpdateSubscriptionsTask));
        builder.AddScoped(typeof(ICleanSubscriptionsRunner), typeof(CleanSubscriptionsRunner));
        builder.AddScoped(typeof(ICleanSubscriptionsTask), typeof(CleanSubscriptionsTask));
        builder.AddScoped(typeof(ICleanSubscribedPendingRunner), typeof(CleanSubscribedPendingRunner));
        builder.AddScoped(typeof(ICleanSubscribedPendingTask), typeof(CleanSubscribedPendingTask));
        builder.AddScoped(typeof(ICleanSubscribedCompletedRunner), typeof(CleanSubscribedCompletedRunner));
        builder.AddScoped(typeof(ICleanSubscribedCompletedTask), typeof(CleanSubscribedCompletedTask));
        builder.AddScoped(typeof(ICleanSubscribedFailedRunner), typeof(CleanSubscribedFailedRunner));
        builder.AddScoped(typeof(ICleanSubscribedFailedTask), typeof(CleanSubscribedFailedTask));
        builder.AddScoped(typeof(IProcessSubscribedRunner), typeof(ProcessSubscribedRunner));
        builder.AddScoped(typeof(IProcessSubscribedTask), typeof(ProcessSubscribedTask));
        builder.AddScoped(typeof(ISubscribedReader), typeof(SubscribedReader));
        builder.AddScoped(typeof(ISubscribedFailures), typeof(SubscribedFailures));
        builder.AddScoped(typeof(IFailedSubscribedMessageHandlerFactory), typeof(FailedSubscribedMessageHandlerFactory));
        builder.AddScoped(typeof(ISubscribedPipelineInvoker), typeof(SubscribedPipelineInvoker));
        builder.AddScoped(typeof(ISubscribedPipelineFactory), typeof(SubscribedPipelineFactory));
        builder.AddScoped(typeof(ISubscribedPipeline), typeof(SubscribedPipeline));
        builder.AddScoped(typeof(ISubscribedPipelineFinalStep), typeof(SubscribedPipelineFinalStep));
        builder.AddScoped(typeof(IFindSubscribedPipelineSteps), typeof(FindSubscribedPipelineSteps));
        builder.AddScoped(typeof(IFindSubscribedHandlers), typeof(FindSubscribedHandlers));
        builder.AddScoped(typeof(ISubscribedPublisher), typeof(SubscribedPublisher));
        builder.AddScoped(typeof(IPublishPipelineInvoker), typeof(PublishPipelineInvoker));
        builder.AddScoped(typeof(IPublishPipelineFactory), typeof(PublishPipelineFactory));
        builder.AddScoped(typeof(IPublishPipeline), typeof(PublishPipeline));
        builder.AddScoped(typeof(IPublishPipelineFinalStep), typeof(PublishPipelineFinalStep));
        builder.AddScoped(typeof(IFindPublishPipelineSteps), typeof(FindPublishPipelineSteps));

        builder.FindAndRegisterMessageHandler(typeof(IHandleSubscribedMessage<>), typeof(ISubscribedMessage));

        return builder;
    }

    private static IServiceCollection RegisterSerializer(this IServiceCollection builder)
    {
        if (Configuration.UseDefaultSerialization)
            builder.AddSingleton<ISerializer, DefaultSerializer>();

        return builder;
    }

    private static IServiceCollection RegisterStartupTasks(this IServiceCollection builder)
    {
        if (!Configuration.UseStartupTasks)
            return builder;

        builder.RegisterMultiple(
            typeof(IRunOnStartup),
            Find(typeof(IRunOnStartup), Assemblies),
            ServiceLifetime.Scoped);

        return builder;
    }

    private static IServiceCollection FindAndRegisterMessageHandler(this IServiceCollection builder,
        Type handlerInterface,
        Type messageInterface)
    {
        var messageTypes = Find(messageInterface, Assemblies);
        foreach (var mt in messageTypes)
        {
            var genericMessageHandlerType = handlerInterface.MakeGenericType(mt);
            var concreteMessageHandlerTypes = Find(genericMessageHandlerType, Assemblies);
            builder.RegisterMultiple(genericMessageHandlerType, concreteMessageHandlerTypes, ServiceLifetime.Scoped);
        }

        return builder;
    }

    private static IServiceCollection RegisterMultiple(this IServiceCollection services, Type serviceType, IEnumerable<Type> implemenetationTypes, ServiceLifetime lifetime)
    {
        foreach (var it in implemenetationTypes)
        {
            services.Add(new ServiceDescriptor(serviceType, it, lifetime));
        }
        return services;
    }

    private static IEnumerable<Type> Find(Type type, IEnumerable<Assembly> assemblies)
    {
        return assemblies
            .Distinct()
            .Where(a => !a.IsDynamic)
            .SelectMany(a => a.GetTypes())
            .Where(t => t.IsAssignableTo(type) && t.IsClass && !t.IsAbstract);
    }
}
