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

public interface IRegistrationProvider
{
    void RegisterInstance<T>(T instance) where T : class;
    void RegisterLogging();
    void RegisterScoped<TInterface, TImplementation>();
    void RegisterScoped(Type interfaceType, List<Type> implementations);
    void RegisterSingleton<TInterface, TImplementation>();
    void RegisterSpecialized();
}

public class RegisterComponents(IRegistrationProvider provider)
{
    public void Register(IBusConfiguration busConfiguration, List<Assembly>? assemblies = null)
    {
        provider.RegisterInstance(busConfiguration);
        provider.RegisterLogging();
        provider.RegisterSpecialized();
        RegisterRequired();
        FindAndRegister(assemblies);
        RegisterConditional(busConfiguration);
    }

    private void RegisterRequired()
    {
        provider.RegisterSingleton<ITaskCounter, TaskCounter>();
        provider.RegisterScoped<ITaskManager, TaskManager>();
        provider.RegisterSingleton<ISystemClock, SystemClock>();
        provider.RegisterSingleton<IMeters, Meters>();
        provider.RegisterSingleton<IAlwaysRunTracker, AlwaysRunTracker>();
        provider.RegisterScoped<IShareObjectsBetweenScopes, ShareObjectsBetweenScopes>();
        provider.RegisterSingleton<IDapperTypesHandler, DapperTypesHandler>();
        provider.RegisterScoped<IBusDataAccess, DapperDataAccess>();
        provider.RegisterScoped<IDapperMethods, DapperMethods>();
        provider.RegisterScoped<ISqlConnectionFactory, SqlConnectionFactory>();
        provider.RegisterSingleton<IProvideDbConnectionString, ProvideDbConnectionString>();
        provider.RegisterSingleton<IRunStartupTasks, RunStarupTasks>();
        provider.RegisterScoped<IStarters, Starters>();
        provider.RegisterSingleton<IUpdateSubscriptionsTracker, UpdateSubscriptionsTracker>();
        provider.RegisterScoped<IUpdateSubscriptionsTask, UpdateSubscriptionsTask>();
        provider.RegisterScoped<IUpdateSubscriptionsStarter, UpdateSubscriptionsStarter>();
        provider.RegisterScoped<IUpdateSubscriptionsRunner, UpdateSubscriptionsRunner>();
        provider.RegisterSingleton<ICleanSubscriptionsTracker, CleanSubscriptionsTracker>();
        provider.RegisterScoped<ICleanSubscriptionsTask, CleanSubscriptionsTask>();
        provider.RegisterScoped<ICleanSubscriptionsStarter, CleanSubscriptionsStarter>();
        provider.RegisterScoped<ICleanSubscriptionsRunner, CleanSubscriptionsRunner>();
        provider.RegisterSingleton<ICleanSubscribedPendingTracker, CleanSubscribedPendingTracker>();
        provider.RegisterScoped<ICleanSubscribedPendingTask, CleanSubscribedPendingTask>();
        provider.RegisterScoped<ICleanSubscribedPendingStarter, CleanSubscribedPendingStarter>();
        provider.RegisterScoped<ICleanSubscribedPendingRunner, CleanSubscribedPendingRunner>();
        provider.RegisterSingleton<ICleanSubscribedCompletedTracker, CleanSubscribedCompletedTracker>();
        provider.RegisterScoped<ICleanSubscribedCompletedTask, CleanSubscribedCompletedTask>();
        provider.RegisterScoped<ICleanSubscribedCompletedStarter, CleanSubscribedCompletedStarter>();
        provider.RegisterScoped<ICleanSubscribedCompletedRunner, CleanSubscribedCompletedRunner>();
        provider.RegisterSingleton<ICleanSubscribedFailedTracker, CleanSubscribedFailedTracker>();
        provider.RegisterScoped<ICleanSubscribedFailedTask, CleanSubscribedFailedTask>();
        provider.RegisterScoped<ICleanSubscribedFailedStarter, CleanSubscribedFailedStarter>();
        provider.RegisterScoped<ICleanSubscribedFailedRunner, CleanSubscribedFailedRunner>();
        provider.RegisterSingleton<ICleanQueuedCompletedTracker, CleanQueuedCompletedTracker>();
        provider.RegisterScoped<ICleanQueuedCompletedTask, CleanQueuedCompletedTask>();
        provider.RegisterScoped<ICleanQueuedCompletedStarter, CleanQueuedCompletedStarter>();
        provider.RegisterScoped<ICleanQueuedCompletedRunner, CleanQueuedCompletedRunner>();
        provider.RegisterSingleton<ICleanQueuedFailedTracker, CleanQueuedFailedTracker>();
        provider.RegisterScoped<ICleanQueuedFailedTask, CleanQueuedFailedTask>();
        provider.RegisterScoped<ICleanQueuedFailedStarter, CleanQueuedFailedStarter>();
        provider.RegisterScoped<ICleanQueuedFailedRunner, CleanQueuedFailedRunner>();
        provider.RegisterScoped<IProcessSubscribedTask, ProcessSubscribedTask>();
        provider.RegisterScoped<IProcessSubscribedStarter, ProcessSubscribedStarter>();
        provider.RegisterScoped<IProcessSubscribedRunner, ProcessSubscribedRunner>();
        provider.RegisterScoped<IProcessQueuedTask, ProcessQueuedTask>();
        provider.RegisterScoped<IProcessQueuedStarter, ProcessQueuedStarter>();
        provider.RegisterScoped<IProcessQueuedRunner, ProcessQueuedRunner>();
        provider.RegisterScoped<IQueueWriter, QueueWriter>();
        provider.RegisterScoped<ISubscribedPublisher, SubscribedPublisher>();
        provider.RegisterScoped<IQueueFailures, QueueFailures>();
        provider.RegisterScoped<ISubscribedFailures, SubscribedFailures>();
        provider.RegisterScoped<IPublishPipelineInvoker, PublishPipelineInvoker>();
        provider.RegisterScoped<IPublishPipelineFactory, PublishPipelineFactory>();
        provider.RegisterScoped<IPublishPipeline, PublishPipeline>();
        provider.RegisterScoped<IFindPublishPipelineSteps, FindPublishPipelineSteps>();
        provider.RegisterScoped<IPublishPipelineFinalStep, PublishPipelineFinalStep>();
        provider.RegisterScoped<ISendPipelineInvoker, SendPipelineInvoker>();
        provider.RegisterScoped<ISendPipelineFactory, SendPipelineFactory>();
        provider.RegisterScoped<ISendPipeline, SendPipeline>();
        provider.RegisterScoped<IFindSendPipelineSteps, FindSendPipelineSteps>();
        provider.RegisterScoped<ISendPipelineFinalStep, SendPipelineFinalStep>();
        provider.RegisterSingleton<ISagaMessageMapManager, SagaMessageMapManager>();
        provider.RegisterScoped<IFindQueueHandlers, FindQueueHandlers>();
        provider.RegisterScoped<IFindQueuePipelineSteps, FindQueuedPipelineSteps>();
        provider.RegisterScoped<IQueueReader, QueueReader>();
        provider.RegisterScoped<IQueuePipelineInvoker, QueuePipelineInvoker>();
        provider.RegisterScoped<IQueuePipelineFactory, QueuePipelineFactory>();
        provider.RegisterScoped<IQueuePipeline, QueuePipeline>();
        provider.RegisterScoped<IQueuePipelineFinalStep, QueuePipelineFinalStep>();
        provider.RegisterScoped<ISubscribedReader, SubscribedReader>();
        provider.RegisterScoped<ISubscribedPipelineInvoker, SubscribedPipelineInvoker>();
        provider.RegisterScoped<ISubscribedPipelineFactory, SubscribedPipelineFactory>();
        provider.RegisterScoped<ISubscribedPipeline, SubscribedPipeline>();
        provider.RegisterScoped<ISubscribedPipelineFinalStep, SubscribedPipelineFinalStep>();
        provider.RegisterScoped<IFindSubscribedPipelineSteps, FindSubscribedPipelineSteps>();
        provider.RegisterScoped<IFindSubscribedHandlers, FindSubscribedHandlers>();
    }

    private void RegisterConditional(IBusConfiguration busConfiguration)
    {
        if (busConfiguration.UseDefaultSerialization)
            provider.RegisterSingleton<ISerializer, DefaultSerializer>();

        if (busConfiguration.QueueConfiguration?.UseDefaultFailedHandler ?? true)
            provider.RegisterScoped<IHandleFailedQueueMessages, DefaultFailedQueueMessageHandler>();

        if (busConfiguration.QueueConfiguration?.UseDefaultRetryStrategy ?? true)
            provider.RegisterScoped<IQueueRetryStrategy, DefaultQueueRetryStrategy>();

        if (busConfiguration.SubscriptionConfiguration?.UseDefaultFailedHandler ?? true)
            provider.RegisterScoped<IHandleFailedSubscribedMessages, DefaultFailedSubscribedMessageHandler>();

        if (busConfiguration.SubscriptionConfiguration?.UseDefaultRetryStrategy ?? true)
            provider.RegisterScoped<ISubscribedRetryStrategy, DefaultSubscribedRetryStrategy>();
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
        provider.RegisterScoped(typeof(TInterface), matches);
    }

    private void FindAndRegisterMessageHandler(Type handlerInterface, Type messageInterface, List<Assembly> assemblies)
    {
        var messageTypes = Find(messageInterface, assemblies);
        foreach (var mt in messageTypes)
        {
            var genericMessageHandlerType = handlerInterface.MakeGenericType(mt);
            var concreteMessageHandlerTypes = Find(genericMessageHandlerType, assemblies);
            provider.RegisterScoped(genericMessageHandlerType, concreteMessageHandlerTypes);
        }
    }

    private static List<Type> Find(Type interfaceType, List<Assembly> assemblies)
    {
        return [.. assemblies
            .Distinct()
            .Where(a => !a.IsDynamic)
            .SelectMany(a => a.GetTypes())
            .Where(t => t.IsAssignableTo(interfaceType) && t.IsClass && !t.IsAbstract)];
    }
}
