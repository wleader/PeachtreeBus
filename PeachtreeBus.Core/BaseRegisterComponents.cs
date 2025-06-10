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

namespace PeachtreeBus;

public abstract class BaseRegisterComponents
{
    public void Register(IBusConfiguration busConfiguration, List<Assembly>? assemblies = null)
    {
        RegisterLogging();
        RegisterSpecialized();
        RegisterRequired(busConfiguration);
        FindAndRegister(assemblies);
        RegisterConditional(busConfiguration);
    }

    protected virtual void RegisterLogging() { }
    protected abstract void RegisterInstance<T>(T instance) where T : class;
    protected abstract void RegisterSpecialized();
    protected abstract void RegisterSingleton<TInterface, TImplementation>();
    protected abstract void RegisterScoped<TInterface, TImplementation>();
    protected abstract void RegisterScoped(Type interfaceType, IEnumerable<Type> implementations);

    private void RegisterRequired(IBusConfiguration busConfiguration)
    {
        RegisterInstance(busConfiguration);
        RegisterSingleton<ITaskCounter, TaskCounter>();
        RegisterScoped<ITaskManager, TaskManager>();
        RegisterSingleton<ISystemClock, SystemClock>();
        RegisterSingleton<IMeters, Meters>();
        RegisterSingleton<IAlwaysRunTracker, AlwaysRunTracker>();
        RegisterScoped<IShareObjectsBetweenScopes, ShareObjectsBetweenScopes>();
        RegisterSingleton<IDapperTypesHandler, DapperTypesHandler>();
        RegisterScoped<IBusDataAccess, DapperDataAccess>();
        RegisterScoped<ISqlConnectionFactory, SqlConnectionFactory>();
        RegisterSingleton<IProvideDbConnectionString, ProvideDbConnectionString>();
        RegisterSingleton<IRunStartupTasks, RunStarupTasks>();
        RegisterScoped<IStarters, Starters>();
        RegisterSingleton<IUpdateSubscriptionsTracker, UpdateSubscriptionsTracker>();
        RegisterScoped<IUpdateSubscriptionsTask, UpdateSubscriptionsTask>();
        RegisterScoped<IUpdateSubscriptionsStarter, UpdateSubscriptionsStarter>();
        RegisterScoped<IUpdateSubscriptionsRunner, UpdateSubscriptionsRunner>();
        RegisterSingleton<ICleanSubscriptionsTracker, CleanSubscriptionsTracker>();
        RegisterScoped<ICleanSubscriptionsTask, CleanSubscriptionsTask>();
        RegisterScoped<ICleanSubscriptionsStarter, CleanSubscriptionsStarter>();
        RegisterScoped<ICleanSubscriptionsRunner, CleanSubscriptionsRunner>();
        RegisterSingleton<ICleanSubscribedPendingTracker, CleanSubscribedPendingTracker>();
        RegisterScoped<ICleanSubscribedPendingTask, CleanSubscribedPendingTask>();
        RegisterScoped<ICleanSubscribedPendingStarter, CleanSubscribedPendingStarter>();
        RegisterScoped<ICleanSubscribedPendingRunner, CleanSubscribedPendingRunner>();
        RegisterSingleton<ICleanSubscribedCompletedTracker, CleanSubscribedCompletedTracker>();
        RegisterScoped<ICleanSubscribedCompletedTask, CleanSubscribedCompletedTask>();
        RegisterScoped<ICleanSubscribedCompletedStarter, CleanSubscribedCompletedStarter>();
        RegisterScoped<ICleanSubscribedCompletedRunner, CleanSubscribedCompletedRunner>();
        RegisterSingleton<ICleanSubscribedFailedTracker, CleanSubscribedFailedTracker>();
        RegisterScoped<ICleanSubscribedFailedTask, CleanSubscribedFailedTask>();
        RegisterScoped<ICleanSubscribedFailedStarter, CleanSubscribedFailedStarter>();
        RegisterScoped<ICleanSubscribedFailedRunner, CleanSubscribedFailedRunner>();
        RegisterSingleton<ICleanQueuedCompletedTracker, CleanQueuedCompletedTracker>();
        RegisterScoped<ICleanQueuedCompletedTask, CleanQueuedCompletedTask>();
        RegisterScoped<ICleanQueuedCompletedStarter, CleanQueuedCompletedStarter>();
        RegisterScoped<ICleanQueuedCompletedRunner, CleanQueuedCompletedRunner>();
        RegisterSingleton<ICleanQueuedFailedTracker, CleanQueuedFailedTracker>();
        RegisterScoped<ICleanQueuedFailedTask, CleanQueuedFailedTask>();
        RegisterScoped<ICleanQueuedFailedStarter, CleanQueuedFailedStarter>();
        RegisterScoped<ICleanQueuedFailedRunner, CleanQueuedFailedRunner>();
        RegisterScoped<IProcessSubscribedTask, ProcessSubscribedTask>();
        RegisterScoped<IProcessSubscribedStarter, ProcessSubscribedStarter>();
        RegisterScoped<IProcessSubscribedRunner, ProcessSubscribedRunner>();
        RegisterScoped<IProcessQueuedTask, ProcessQueuedTask>();
        RegisterScoped<IProcessQueuedStarter, ProcessQueuedStarter>();
        RegisterScoped<IProcessQueuedRunner, ProcessQueuedRunner>();
        RegisterScoped<IQueueWriter, QueueWriter>();
        RegisterScoped<ISubscribedPublisher, SubscribedPublisher>();
        RegisterScoped<IFailedQueueMessageHandlerFactory, FailedQueueMessageHandlerFactory>();
        RegisterScoped<IQueueFailures, QueueFailures>();
        RegisterScoped<IFailedSubscribedMessageHandlerFactory, FailedSubscribedMessageHandlerFactory>();
        RegisterScoped<ISubscribedFailures, SubscribedFailures>();
        RegisterScoped<IPublishPipelineInvoker, PublishPipelineInvoker>();
        RegisterScoped<IPublishPipelineFactory, PublishPipelineFactory>();
        RegisterScoped<IPublishPipeline, PublishPipeline>();
        RegisterScoped<IFindPublishPipelineSteps, FindPublishPipelineSteps>();
        RegisterScoped<IPublishPipelineFinalStep, PublishPipelineFinalStep>();
        RegisterScoped<ISendPipelineInvoker, SendPipelineInvoker>();
        RegisterScoped<ISendPipelineFactory, SendPipelineFactory>();
        RegisterScoped<ISendPipeline, SendPipeline>();
        RegisterScoped<IFindSendPipelineSteps, FindSendPipelineSteps>();
        RegisterScoped<ISendPipelineFinalStep, SendPipelineFinalStep>();
        RegisterSingleton<ISagaMessageMapManager, SagaMessageMapManager>();
        RegisterScoped<IFindQueueHandlers, FindQueueHandlers>();
        RegisterScoped<IFindQueuePipelineSteps, FindQueuedPipelineSteps>();
        RegisterScoped<IQueueReader, QueueReader>();
        RegisterScoped<IQueuePipelineInvoker, QueuePipelineInvoker>();
        RegisterScoped<IQueuePipelineFactory, QueuePipelineFactory>();
        RegisterScoped<IQueuePipeline, QueuePipeline>();
        RegisterScoped<IQueuePipelineFinalStep, QueuePipelineFinalStep>();
        RegisterScoped<ISubscribedReader, SubscribedReader>();
        RegisterScoped<ISubscribedPipelineInvoker, SubscribedPipelineInvoker>();
        RegisterScoped<ISubscribedPipelineFactory, SubscribedPipelineFactory>();
        RegisterScoped<ISubscribedPipeline, SubscribedPipeline>();
        RegisterScoped<ISubscribedPipelineFinalStep, SubscribedPipelineFinalStep>();
        RegisterScoped<IFindSubscribedPipelineSteps, FindSubscribedPipelineSteps>();
        RegisterScoped<IFindSubscribedHandlers, FindSubscribedHandlers>();
    }

    private void RegisterConditional(IBusConfiguration busConfiguration)
    {
        if (busConfiguration.UseDefaultSerialization)
            RegisterSingleton<ISerializer, DefaultSerializer>();

        if (busConfiguration.QueueConfiguration?.UseDefaultFailedHandler ?? true)
            RegisterScoped<IHandleFailedQueueMessages, DefaultFailedQueueMessageHandler>();

        if (busConfiguration.QueueConfiguration?.UseDefaultRetryStrategy ?? true)
            RegisterScoped<IQueueRetryStrategy, DefaultQueueRetryStrategy>();

        if (busConfiguration.SubscriptionConfiguration?.UseDefaultFailedHandler ?? true)
            RegisterScoped<IHandleFailedSubscribedMessages, DefaultFailedSubscribedMessageHandler>();

        if (busConfiguration.SubscriptionConfiguration?.UseDefaultRetryStrategy ?? true)
            RegisterScoped<ISubscribedRetryStrategy, DefaultSubscribedRetryStrategy>();
    }

    private void FindAndRegister(List<Assembly>? assemblies = null)
    {
        assemblies ??= [.. AppDomain.CurrentDomain.GetAssemblies()];

        FindAndRegister<IPublishPipelineStep>(assemblies);
        FindAndRegister<ISendPipelineStep>(assemblies);
        FindAndRegister<ISubscribedPipelineStep>(assemblies);
        FindAndRegister<IQueuePipelineStep>(assemblies);
        FindAndRegister<IRunOnStartup>(assemblies);

        FindAndRegisterMessageHandler(typeof(IHandleQueueMessage<>), typeof(IQueueMessage), assemblies); // also finds sagas.
        FindAndRegisterMessageHandler(typeof(IHandleSubscribedMessage<>), typeof(ISubscribedMessage), assemblies);
    }

    private void FindAndRegister<TInterface>(List<Assembly> assemblies)
    {
        var matches = Find(typeof(TInterface), assemblies);
        RegisterScoped(typeof(Type), matches);
    }

    private void FindAndRegisterMessageHandler(Type handlerInterface, Type messageInterface, List<Assembly> assemblies)
    {
        var messageTypes = Find(messageInterface, assemblies);
        foreach (var mt in messageTypes)
        {
            var genericMessageHandlerType = handlerInterface.MakeGenericType(mt);
            var concreteMessageHandlerTypes = Find(genericMessageHandlerType, assemblies);
            RegisterScoped(genericMessageHandlerType, concreteMessageHandlerTypes);
        }
    }

    private static IEnumerable<Type> Find(Type interfaceType, IEnumerable<Assembly> assemblies)
    {
        return assemblies
            .Distinct()
            .Where(a => !a.IsDynamic)
            .SelectMany(a => a.GetTypes())
            .Where(t => t.IsAssignableTo(interfaceType) && t.IsClass && !t.IsAbstract);
    }
}
