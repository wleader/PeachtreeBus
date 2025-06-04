using Microsoft.Extensions.Logging;
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
using SimpleInjector;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

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
        container.Register(typeof(IDapperTypesHandler), typeof(DapperTypesHandler), Lifestyle.Singleton);
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

        // telemetry services.
        container.RegisterSingleton(typeof(IMeters), () => new Meters());

        // provide an abstracted access to the system clock 
        // supports unit testable code.
        container.RegisterSingleton(typeof(ISystemClock), typeof(SystemClock));

        // runs things once at startup.
        container.Register(typeof(IRunStartupTasks), typeof(RunStarupTasks), Lifestyle.Singleton);

        // The task manager manages all repeating tasks.
        container.Register(typeof(ITaskManager), typeof(TaskManager), Lifestyle.Scoped);
        container.Register(typeof(IStarters), typeof(Starters), Lifestyle.Scoped);
        container.Register(typeof(ITaskCounter), typeof(TaskCounter), Lifestyle.Singleton);
        container.Register(typeof(ISleeper), typeof(Sleeper), Lifestyle.Transient);

        container.Register(typeof(IUpdateSubscriptionsTracker), typeof(UpdateSubscriptionsTracker), Lifestyle.Singleton);
        container.Register(typeof(IUpdateSubscriptionsTask), typeof(UpdateSubscriptionsTask), Lifestyle.Scoped);
        container.Register(typeof(IUpdateSubscriptionsStarter), typeof(UpdateSubscriptionsStarter), Lifestyle.Scoped);
        container.Register(typeof(IUpdateSubscriptionsRunner), typeof(UpdateSubscriptionsRunner), Lifestyle.Scoped);

        container.Register(typeof(ICleanSubscriptionsTracker), typeof(CleanSubscriptionsTracker), Lifestyle.Singleton);
        container.Register(typeof(ICleanSubscriptionsTask), typeof(CleanSubscriptionsTask), Lifestyle.Scoped);
        container.Register(typeof(ICleanSubscriptionsStarter), typeof(CleanSubscriptionsStarter), Lifestyle.Scoped);
        container.Register(typeof(ICleanSubscriptionsRunner), typeof(CleanSubscriptionsRunner), Lifestyle.Scoped);

        container.Register(typeof(ICleanSubscribedPendingTracker), typeof(CleanSubscribedPendingTracker), Lifestyle.Singleton);
        container.Register(typeof(ICleanSubscribedPendingTask), typeof(CleanSubscribedPendingTask), Lifestyle.Scoped);
        container.Register(typeof(ICleanSubscribedPendingStarter), typeof(CleanSubscribedPendingStarter), Lifestyle.Scoped);
        container.Register(typeof(ICleanSubscribedPendingRunner), typeof(CleanSubscribedPendingRunner), Lifestyle.Scoped);

        container.Register(typeof(ICleanSubscribedCompletedTracker), typeof(CleanSubscribedCompletedTracker), Lifestyle.Singleton);
        container.Register(typeof(ICleanSubscribedCompletedTask), typeof(CleanSubscribedCompletedTask), Lifestyle.Scoped);
        container.Register(typeof(ICleanSubscribedCompletedStarter), typeof(CleanSubscribedCompletedStarter), Lifestyle.Scoped);
        container.Register(typeof(ICleanSubscribedCompletedRunner), typeof(CleanSubscribedCompletedRunner), Lifestyle.Scoped);

        container.Register(typeof(ICleanSubscribedFailedTracker), typeof(CleanSubscribedFailedTracker), Lifestyle.Singleton);
        container.Register(typeof(ICleanSubscribedFailedTask), typeof(CleanSubscribedFailedTask), Lifestyle.Scoped);
        container.Register(typeof(ICleanSubscribedFailedStarter), typeof(CleanSubscribedFailedStarter), Lifestyle.Scoped);
        container.Register(typeof(ICleanSubscribedFailedRunner), typeof(CleanSubscribedFailedRunner), Lifestyle.Scoped);

        container.Register(typeof(ICleanQueuedCompletedTracker), typeof(CleanQueuedCompletedTracker), Lifestyle.Singleton);
        container.Register(typeof(ICleanQueuedCompletedTask), typeof(CleanQueuedCompletedTask), Lifestyle.Scoped);
        container.Register(typeof(ICleanQueuedCompletedStarter), typeof(CleanQueuedCompletedStarter), Lifestyle.Scoped);
        container.Register(typeof(ICleanQueuedCompletedRunner), typeof(CleanQueuedCompletedRunner), Lifestyle.Scoped);

        container.Register(typeof(ICleanQueuedFailedTracker), typeof(CleanQueuedFailedTracker), Lifestyle.Singleton);
        container.Register(typeof(ICleanQueuedFailedTask), typeof(CleanQueuedFailedTask), Lifestyle.Scoped);
        container.Register(typeof(ICleanQueuedFailedStarter), typeof(CleanQueuedFailedStarter), Lifestyle.Scoped);
        container.Register(typeof(ICleanQueuedFailedRunner), typeof(CleanQueuedFailedRunner), Lifestyle.Scoped);

        container.Register(typeof(IProcessSubscribedTask), typeof(ProcessSubscribedTask), Lifestyle.Scoped);
        container.Register(typeof(IProcessSubscribedStarter), typeof(ProcessSubscribedStarter), Lifestyle.Scoped);
        container.Register(typeof(IProcessSubscribedRunner), typeof(ProcessSubscribedRunner), Lifestyle.Scoped);

        container.Register(typeof(IProcessQueuedTask), typeof(ProcessQueuedTask), Lifestyle.Scoped);
        container.Register(typeof(IProcessQueuedStarter), typeof(ProcessQueuedStarter), Lifestyle.Scoped);
        container.Register(typeof(IProcessQueuedRunner), typeof(ProcessQueuedRunner), Lifestyle.Scoped);

        container.Register(typeof(IAlwaysRunTracker), typeof(AlwaysRunTracker), Lifestyle.Singleton);

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

        // the class name service uses caching to speed up the conversion of class name strings to types.
        container.Register(typeof(IClassNameService), typeof(ClassNameService), Lifestyle.Singleton);
        container.RegisterDecorator(typeof(IClassNameService), typeof(CachedClassNameService));

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
        if (Configuration.QueueConfiguration?.UseDefaultFailedHandler ?? true)
            container.Register(typeof(IHandleFailedQueueMessages), typeof(DefaultFailedQueueMessageHandler), Lifestyle.Scoped);

        if (Configuration.QueueConfiguration?.UseDefaultRetryStrategy ?? true)
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
        container.Register(typeof(IQueueReader), typeof(QueueReader), Lifestyle.Scoped);
        container.Register(typeof(IQueuePipelineInvoker), typeof(QueuePipelineInvoker), Lifestyle.Scoped);
        container.Register(typeof(IQueuePipelineFactory), typeof(QueuePipelineFactory), Lifestyle.Scoped);
        container.Register(typeof(IQueuePipeline), typeof(QueuePipeline), Lifestyle.Scoped);
        container.Register(typeof(IQueuePipelineFinalStep), typeof(QueuePipelineFinalStep), Lifestyle.Scoped);

        return container;
    }

    private static Container RegisterSubscribedComponents(this Container container)
    {
        if (Configuration.SubscriptionConfiguration?.UseDefaultFailedHandler ?? true)
            container.Register(typeof(IHandleFailedSubscribedMessages), typeof(DefaultFailedSubscribedMessageHandler), Lifestyle.Scoped);

        if (Configuration.SubscriptionConfiguration?.UseDefaultRetryStrategy ?? true)
            container.Register(typeof(ISubscribedRetryStrategy), typeof(DefaultSubscribedRetryStrategy), Lifestyle.Singleton);

        // detects missing registrations.
        container.Register(typeof(VerifiySubscriptionsRequirements), typeof(VerifiySubscriptionsRequirements), Lifestyle.Transient);

        // register our subscription message handlers
        container.FindAndRegisterMessageHandler(typeof(IHandleSubscribedMessage<>), typeof(ISubscribedMessage));

        // register pipeline steps
        container.FindAndRegisterScopedTypes<ISubscribedPipelineStep>();

        // register stuff needed to process subscribed messages.
        container.Register(typeof(ISubscribedReader), typeof(SubscribedReader), Lifestyle.Scoped);
        container.Register(typeof(IFindSubscribedHandlers), typeof(FindSubscribedHandlers), Lifestyle.Scoped);
        container.Register(typeof(IFindSubscribedPipelineSteps), typeof(FindSubscribedPipelineSteps), Lifestyle.Scoped);
        container.Register(typeof(ISubscribedPipelineInvoker), typeof(SubscribedPipelineInvoker), Lifestyle.Scoped);
        container.Register(typeof(ISubscribedPipelineFactory), typeof(SubscribedPipelineFactory), Lifestyle.Scoped);
        container.Register(typeof(ISubscribedPipeline), typeof(SubscribedPipeline), Lifestyle.Scoped);
        container.Register(typeof(ISubscribedPipelineFinalStep), typeof(SubscribedPipelineFinalStep), Lifestyle.Scoped);

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
        if (!Configuration.UseStartupTasks)
            return container;

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

        var runner = container.GetInstance<IRunStartupTasks>();
        runner.RunStartupTasks();

        return container;
    }
}
