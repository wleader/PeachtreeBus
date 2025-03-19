using Microsoft.Extensions.Logging;
using PeachtreeBus.Cleaners;
using PeachtreeBus.Data;
using PeachtreeBus.DatabaseSharing;
using PeachtreeBus.Errors;
using PeachtreeBus.Queues;
using PeachtreeBus.Sagas;
using PeachtreeBus.Serialization;
using PeachtreeBus.Subscriptions;
using SimpleInjector;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace PeachtreeBus.SimpleInjector;

public static partial class SimpleInjectorExtensions
{
    /// <summary>
    /// Enables Basic PeachtreeBus functionality.
    /// Registers needed services with the Container.
    /// </summary>
    public static Container UsePeachtreeBus(this Container container, IBusConfiguration configuration, ILoggerFactory loggerFactory, List<Assembly>? assemblies = null)
    {
        Assemblies = assemblies ?? [.. AppDomain.CurrentDomain.GetAssemblies().ToList()];

        Configuration = configuration;

        // put the configuration into the container so it can be used later.
        container.RegisterInstance(typeof(IBusConfiguration), configuration);

        container
            .RegisterLogging(loggerFactory)
            .RegisterRequiredComponents()
            .RegisterSerializer()
            .RegisterQueueComponents()
            .RegisterSubscribedComponents()
            .RegisterStartupTasks();

        //// Register tasks that should be run once at startup.
        //container.RegisterPeachtreeBusStartupTasks();

        return container;
    }

    private static IBusConfiguration Configuration = default!;

    private static Container RegisterLogging(this Container container, ILoggerFactory loggerFactory)
    {
        container.RegisterInstance(loggerFactory);
        container.RegisterSingleton(typeof(ILogger<>), typeof(Logger<>));
        return container;
    }

    /// <summary>
    /// Registers things that a user should not need to replace,
    /// And that are required for basic functionality.
    /// </summary>
    private static Container RegisterRequiredComponents(this Container container)
    {
        // detects missing registrations.
        container.Register(typeof(VerifyBaseRequirements), typeof(VerifyBaseRequirements), Lifestyle.Transient);

        // Data access components are needed to:
        // send messages (queue or subscribed)
        // handle message (queue or susbscribed)
        // Subscribed to messages
        // do cleanups
        // pretty much everything, so always register these things.
        container.Register(typeof(IBusDataAccess), typeof(DapperDataAccess), Lifestyle.Scoped);
        container.Register(typeof(ISqlConnection), () => container.GetInstance<ISqlConnectionFactory>().GetConnection(), Lifestyle.Scoped);
        container.Register(typeof(ISqlConnectionFactory), typeof(SqlConnectionFactory), Lifestyle.Scoped);
        container.Register(typeof(IShareObjectsBetweenScopes), typeof(ShareObjectsBetweenScopes), Lifestyle.Scoped);
        container.Register(typeof(IProvideDbConnectionString), typeof(ProvideDbConnectionString), Lifestyle.Singleton);

        var sharedDbProducer = Lifestyle.Scoped.CreateProducer<ISharedDatabase>(typeof(SharedDatabase), container);
        container.Register(typeof(ISharedDatabase),
            () => container.GetInstance<IShareObjectsBetweenScopes>().SharedDatabase ?? sharedDbProducer.GetInstance(),
            Lifestyle.Scoped);

        // All of the worker threads need to operate in a scope,
        // so scope handling is always required.
        container.Register(typeof(IWrappedScopeFactory), () => new SimpleInjectorScopeFactory(container), Lifestyle.Singleton);
        container.Register(typeof(IWrappedScope), typeof(SimpleInjectorScope), Lifestyle.Scoped);

        // enables dotnet perf counters.
        container.RegisterSingleton(typeof(IPerfCounters), PerfCounters.Instance);

        // provide an abstracted access to the system clock 
        // supports unit testable code.
        container.RegisterSingleton(typeof(ISystemClock), typeof(SystemClock));

        // anybody should be able to send messages to a queue,
        // or publish subscribed messages without being a 
        // consumer of either.
        container.Register(typeof(IQueueWriter), typeof(QueueWriter), Lifestyle.Scoped);
        container.Register(typeof(ISubscribedPublisher), typeof(SubscribedPublisher), Lifestyle.Scoped);

        // because sending isn't specific to reading a queue or subscribing, the outgoing pipelines must be registered.
        container.RegisterOutgoingPipelines();

        // failed message handlers
        container.Register(typeof(IFailedQueueMessageHandlerFactory), typeof(FailedQueueMessageHandlerFactory), Lifestyle.Scoped);
        container.Register(typeof(IQueueFailures), typeof(QueueFailures), Lifestyle.Scoped);

        container.Register(typeof(IFailedSubscribedMessageHandlerFactory), typeof(FailedSubscribedMessageHandlerFactory), Lifestyle.Scoped);
        container.Register(typeof(ISubscribedFailures), typeof(SubscribedFailures), Lifestyle.Scoped);

        return container;
    }

    private static Container RegisterOutgoingPipelines(this Container container)
    {
        container.Register(typeof(IPublishPipelineInvoker), typeof(PublishPipelineInvoker), Lifestyle.Scoped);
        container.Register(typeof(IPublishPipelineFactory), typeof(PublishPipelineFactory), Lifestyle.Scoped);
        container.Register(typeof(IPublishPipeline), typeof(PublishPipeline), Lifestyle.Scoped);
        container.Register(typeof(IFindPublishPipelineSteps), typeof(FindPublishPipelineSteps), Lifestyle.Scoped);
        container.Register(typeof(IPublishPipelineFinalStep), typeof(PublishPipelineFinalStep), Lifestyle.Scoped);

        // register pipeline steps
        container.FindAndRegisterScopedTypes<IPublishPipelineStep>();


        container.Register(typeof(ISendPipelineInvoker), typeof(SendPipelineInvoker), Lifestyle.Scoped);
        container.Register(typeof(ISendPipelineFactory), typeof(SendPipelineFactory), Lifestyle.Scoped);
        container.Register(typeof(ISendPipeline), typeof(SendPipeline), Lifestyle.Scoped);
        container.Register(typeof(IFindSendPipelineSteps), typeof(FindSendPipelineSteps), Lifestyle.Scoped);
        container.Register(typeof(ISendPipelineFinalStep), typeof(SendPipelineFinalStep), Lifestyle.Scoped);

        // register pipeline steps
        container.FindAndRegisterScopedTypes<ISendPipelineStep>();

        return container;
    }

    private static Container RegisterSerializer(this Container container)
    {
        // a serializer is needed to convert objects such as messages and saga data to and
        // from strings that can be stored in a single database column.
        // this can be replaced in the container if desired, but one is required for the bus 
        // code to function.
        if (Configuration.UseDefaultSerialization)
            container.RegisterSingleton(typeof(ISerializer), typeof(DefaultSerializer));

        return container;
    }

    private static Container RegisterQueueComponents(this Container container)
    {
        if (Configuration.QueueConfiguration is null) return container;

        if (Configuration.QueueConfiguration.UseDefaultFailedHandler)
            container.Register(typeof(IHandleFailedQueueMessages), typeof(DefaultFailedQueueMessageHandler), Lifestyle.Scoped);

        if (Configuration.QueueConfiguration.UseDefaultRetryStrategy)
            container.Register(typeof(IQueueRetryStrategy), typeof(DefaultQueueRetryStrategy), Lifestyle.Singleton);

        // detects missing registrations.
        container.Register(typeof(VerifyQueueRequirements), typeof(VerifyQueueRequirements), Lifestyle.Transient);

        // register message handlers and sagas.
        // this finds types that impliment IHandleMessage<>.
        container.FindAndRegisterMessageHandler(typeof(IHandleQueueMessage<>), typeof(IQueueMessage));

        // register pipeline steps
        container.FindAndRegisterScopedTypes<IQueuePipelineStep>();

        // register classes needed to service the message queue.
        container.RegisterSingleton(typeof(ISagaMessageMapManager), typeof(SagaMessageMapManager));
        container.Register(typeof(IFindQueueHandlers), typeof(FindQueueHandlers), Lifestyle.Scoped);
        container.Register(typeof(IFindQueuePipelineSteps), typeof(FindQueuedPipelineSteps), Lifestyle.Scoped);
        container.Register(typeof(IQueueThread), typeof(QueueThread), Lifestyle.Scoped);
        container.Register(typeof(IQueueWork), typeof(QueueWork), Lifestyle.Scoped);
        container.Register(typeof(IQueueReader), typeof(QueueReader), Lifestyle.Scoped);
        container.Register(typeof(IQueuePipelineInvoker), typeof(QueuePipelineInvoker), Lifestyle.Scoped);
        container.Register(typeof(IQueuePipelineFactory), typeof(QueuePipelineFactory), Lifestyle.Scoped);
        container.Register(typeof(IQueuePipeline), typeof(QueuePipeline), Lifestyle.Scoped);
        container.Register(typeof(IQueuePipelineFinalStep), typeof(QueuePipelineFinalStep), Lifestyle.Scoped);

        container.Register<IQueueCleanupThread, QueueCleanupThread>(Lifestyle.Scoped);
        container.Register<IQueueCleanupWork, QueueCleanupWork>(Lifestyle.Scoped);
        container.Register<IQueueCleaner, QueueCleaner>(Lifestyle.Scoped);

        return container;
    }

    private static Container RegisterSubscribedComponents(this Container container)
    {
        if (Configuration.SubscriptionConfiguration is null) return container;

        if (Configuration.SubscriptionConfiguration.UseDefaultFailedHandler)
            container.Register(typeof(IHandleFailedSubscribedMessages), typeof(DefaultFailedSubscribedMessageHandler), Lifestyle.Scoped);

        if (Configuration.SubscriptionConfiguration.UseDefaultRetryStrategy)
            container.Register(typeof(ISubscribedRetryStrategy), typeof(DefaultSubscribedRetryStrategy), Lifestyle.Singleton);

        // detects missing registrations.
        container.Register(typeof(VerifiySubscriptionsRequirements), typeof(VerifiySubscriptionsRequirements), Lifestyle.Transient);

        // register our subscription message handlers
        container.FindAndRegisterMessageHandler(typeof(IHandleSubscribedMessage<>), typeof(ISubscribedMessage));

        // register pipeline steps
        container.FindAndRegisterScopedTypes<ISubscribedPipelineStep>();

        // register stuff needed to process subscribed messages.
        container.Register(typeof(ISubscribedThread), typeof(SubscribedThread), Lifestyle.Scoped);
        container.Register(typeof(ISubscribedWork), typeof(SubscribedWork), Lifestyle.Scoped);
        container.Register(typeof(ISubscribedReader), typeof(SubscribedReader), Lifestyle.Scoped);
        container.Register(typeof(IFindSubscribedHandlers), typeof(FindSubscribedHandlers), Lifestyle.Scoped);
        container.Register(typeof(IFindSubscribedPipelineSteps), typeof(FindSubscribedPipelineSteps), Lifestyle.Scoped);
        container.Register(typeof(ISubscriptionUpdateThread), typeof(SubscriptionUpdateThread), Lifestyle.Scoped);
        container.Register(typeof(ISubscriptionUpdateWork), typeof(SubscriptionUpdateWork), Lifestyle.Scoped);
        container.Register(typeof(ISubscribedPipelineInvoker), typeof(SubscribedPipelineInvoker), Lifestyle.Scoped);
        container.Register(typeof(ISubscribedPipelineFactory), typeof(SubscribedPipelineFactory), Lifestyle.Scoped);
        container.Register(typeof(ISubscribedPipeline), typeof(SubscribedPipeline), Lifestyle.Scoped);
        container.Register(typeof(ISubscribedPipelineFinalStep), typeof(SubscribedPipelineFinalStep), Lifestyle.Scoped);

        container.Register<ISubscriptionCleanupThread, SubscriptionCleanupThread>(Lifestyle.Scoped);
        container.Register<ISubscriptionCleanupWork, SubscriptionCleanupWork>(Lifestyle.Scoped);
        container.Register<ISubscribedCleanupThread, SubscribedCleanupThread>(Lifestyle.Scoped);
        container.Register<ISubscribedCleanupWork, SubscribedCleanupWork>(Lifestyle.Scoped);
        container.Register<ISubscribedCleaner, SubscribedCleaner>(Lifestyle.Scoped);

        return container;
    }

    private static List<Assembly> Assemblies = default!;

    private static Container FindAndRegisterMessageHandler(this Container container,
        Type handlerInterface,
        Type messageInterface)
    {
        // find all of the messages.
        var messageTypes = container.GetTypesToRegister(messageInterface, Assemblies);
        foreach (var mt in messageTypes)
        {
            // determine the generic interface for the IHandleMessage<mt>
            var genericMessageHandlerType = handlerInterface.MakeGenericType(mt);
            // find types that impliment IHandleMessage<mt>
            var concreteMessageHandlerTypes = container.GetTypesToRegister(genericMessageHandlerType, Assemblies);
            // collection register them so the Message Processor can find the handlers.
            container.Collection.Register(genericMessageHandlerType, concreteMessageHandlerTypes, Lifestyle.Scoped);
            container.RegisterConcreteTypesIfNeeded(concreteMessageHandlerTypes, Lifestyle.Scoped);
        }
        return container;
    }

    private static Container FindAndRegisterScopedTypes<T>(this Container container)
    {
        var foundTypes = container.GetTypesToRegister(typeof(T), Assemblies);
        container.Collection.Register(typeof(T), foundTypes, Lifestyle.Scoped);
        container.RegisterConcreteTypesIfNeeded(foundTypes, Lifestyle.Scoped);
        return container;
    }

    private static Container RegisterStartupTasks(this Container container)
    {
        if (!Configuration.UseStartupTasks) return container;

        // get a list types that impliment the type.
        // We'll instantiate them later.
        var runOnStartupTypes = container.GetTypesToRegister(typeof(IRunOnStartup), Assemblies);
        // Register the concrete types. This allows the container to do the DI later.
        foreach (var t in runOnStartupTypes) { container.Register(t, t, Lifestyle.Scoped); }

        return container;
    }

    public static Container RunStartupTasks(this Container container)
    {
        if (!Configuration.UseStartupTasks)
            return container;

        List<Task> tasks = [];
        List<IWrappedScope> scopes = [];

        var startupTaskTypes = container.FindTypesThatImplement<IRunOnStartup>();
        var factory = container.GetInstance<IWrappedScopeFactory>();

        foreach (var t in startupTaskTypes)
        {
            var scope = factory.Create();
            scopes.Add(scope);
            var startupTask = (IRunOnStartup)scope.GetInstance(t);
            tasks.Add(startupTask.Run());
        }

        Task.WaitAll([.. tasks]);

        foreach (var s in scopes)
        { s.Dispose(); }

        return container;
    }
}
