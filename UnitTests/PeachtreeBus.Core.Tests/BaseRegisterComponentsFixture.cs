using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using PeachtreeBus.Data;
using PeachtreeBus.DatabaseSharing;
using PeachtreeBus.Queues;
using PeachtreeBus.Sagas;
using PeachtreeBus.ScannableAssembly;
using PeachtreeBus.Serialization;
using PeachtreeBus.Subscriptions;
using PeachtreeBus.Tasks;
using PeachtreeBus.Telemetry;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace PeachtreeBus.Core.Tests;

[TestClass]
public class BaseRegisterComponentsFixture
{
    private RegisterComponents _registerComponents = default!;
    private readonly Mock<IRegistrationProvider> _registrationProvider = new();

    private readonly BusConfiguration _basicConfiguration = new()
    {
        ConnectionString = "CONNECTIONSTRING",
        Schema = new("PBus"),
    };


    [TestInitialize]
    public void Initialize()
    {
        _registrationProvider.Reset();

        _registerComponents = new(
            _registrationProvider.Object);
    }

    private void VerifySingletonRegistered<T1, T2>(Times? times = null) =>
        _registrationProvider.Verify(p => p.RegisterSingleton<T1, T2>(), times ?? Times.Once());

    private void VerifyScopedRegistered<T1, T2>(Times? times = null) =>
        _registrationProvider.Verify(p => p.RegisterScoped<T1, T2>(), times ?? Times.Once());

    [TestMethod]
    public void Given_BusConfiguration_When_Register_Then_BusConfigurationRegistered()
    {
        _registerComponents.Register(_basicConfiguration, []);
        _registrationProvider.Verify(r => r.RegisterInstance<IBusConfiguration>(_basicConfiguration), Times.Once);
    }

    [TestMethod]
    public void When_Register_Then_LoggingRegistered()
    {
        _registerComponents.Register(_basicConfiguration, []);
        _registrationProvider.Verify(r => r.RegisterLogging(), Times.Once());
    }

    [TestMethod]
    public void When_Register_Then_SpecializedRegistered()
    {
        _registerComponents.Register(_basicConfiguration, []);
        _registrationProvider.Verify(r => r.RegisterSpecialized(), Times.Once());
    }

    [TestMethod]
    public void When_Register_Then_RequiredComponentsRegistered()
    {
        _registerComponents.Register(_basicConfiguration, []);

        VerifySingletonRegistered<IMessagingTaskCounter, MessagingTaskCounter>();
        VerifySingletonRegistered<IScheduledTaskCounter, ScheduledTaskCounter>();
        VerifyScopedRegistered<ITaskManager, TaskManager>();
        VerifySingletonRegistered<ISystemClock, SystemClock>();
        VerifySingletonRegistered<IMeters, Meters>();
        VerifySingletonRegistered<IAlwaysRunTracker, AlwaysRunTracker>();
        VerifyScopedRegistered<IShareObjectsBetweenScopes, ShareObjectsBetweenScopes>();
        VerifySingletonRegistered<IDapperTypesHandler, DapperTypesHandler>();
        VerifyScopedRegistered<IBusDataAccess, DapperDataAccess>();
        VerifyScopedRegistered<IDapperMethods, DapperMethods>();
        VerifyScopedRegistered<ISqlConnectionFactory, SqlConnectionFactory>();
        VerifySingletonRegistered<IProvideDbConnectionString, ProvideDbConnectionString>();
        VerifySingletonRegistered<IRunStartupTasks, RunStarupTasks>();
        VerifyScopedRegistered<IStarters, Starters>();
        VerifySingletonRegistered<IUpdateSubscriptionsTracker, UpdateSubscriptionsTracker>();
        VerifyScopedRegistered<IUpdateSubscriptionsTask, UpdateSubscriptionsTask>();
        VerifyScopedRegistered<IUpdateSubscriptionsStarter, UpdateSubscriptionsStarter>();
        VerifyScopedRegistered<IUpdateSubscriptionsRunner, UpdateSubscriptionsRunner>();
        VerifySingletonRegistered<ICleanSubscriptionsTracker, CleanSubscriptionsTracker>();
        VerifyScopedRegistered<ICleanSubscriptionsTask, CleanSubscriptionsTask>();
        VerifyScopedRegistered<ICleanSubscriptionsStarter, CleanSubscriptionsStarter>();
        VerifyScopedRegistered<ICleanSubscriptionsRunner, CleanSubscriptionsRunner>();
        VerifySingletonRegistered<ICleanSubscribedPendingTracker, CleanSubscribedPendingTracker>();
        VerifyScopedRegistered<ICleanSubscribedPendingTask, CleanSubscribedPendingTask>();
        VerifyScopedRegistered<ICleanSubscribedPendingStarter, CleanSubscribedPendingStarter>();
        VerifyScopedRegistered<ICleanSubscribedPendingRunner, CleanSubscribedPendingRunner>();
        VerifySingletonRegistered<ICleanSubscribedCompletedTracker, CleanSubscribedCompletedTracker>();
        VerifyScopedRegistered<ICleanSubscribedCompletedTask, CleanSubscribedCompletedTask>();
        VerifyScopedRegistered<ICleanSubscribedCompletedStarter, CleanSubscribedCompletedStarter>();
        VerifyScopedRegistered<ICleanSubscribedCompletedRunner, CleanSubscribedCompletedRunner>();
        VerifySingletonRegistered<ICleanSubscribedFailedTracker, CleanSubscribedFailedTracker>();
        VerifyScopedRegistered<ICleanSubscribedFailedTask, CleanSubscribedFailedTask>();
        VerifyScopedRegistered<ICleanSubscribedFailedStarter, CleanSubscribedFailedStarter>();
        VerifyScopedRegistered<ICleanSubscribedFailedRunner, CleanSubscribedFailedRunner>();
        VerifySingletonRegistered<ICleanQueuedCompletedTracker, CleanQueuedCompletedTracker>();
        VerifyScopedRegistered<ICleanQueuedCompletedTask, CleanQueuedCompletedTask>();
        VerifyScopedRegistered<ICleanQueuedCompletedStarter, CleanQueuedCompletedStarter>();
        VerifyScopedRegistered<ICleanQueuedCompletedRunner, CleanQueuedCompletedRunner>();
        VerifySingletonRegistered<ICleanQueuedFailedTracker, CleanQueuedFailedTracker>();
        VerifyScopedRegistered<ICleanQueuedFailedTask, CleanQueuedFailedTask>();
        VerifyScopedRegistered<ICleanQueuedFailedStarter, CleanQueuedFailedStarter>();
        VerifyScopedRegistered<ICleanQueuedFailedRunner, CleanQueuedFailedRunner>();
        VerifyScopedRegistered<IProcessSubscribedTask, ProcessSubscribedTask>();
        VerifyScopedRegistered<IProcessSubscribedStarter, ProcessSubscribedStarter>();
        VerifyScopedRegistered<IProcessSubscribedRunner, ProcessSubscribedRunner>();
        VerifyScopedRegistered<IProcessQueuedTask, ProcessQueuedTask>();
        VerifyScopedRegistered<IProcessQueuedStarter, ProcessQueuedStarter>();
        VerifyScopedRegistered<IProcessQueuedRunner, ProcessQueuedRunner>();
        VerifyScopedRegistered<IQueueWriter, QueueWriter>();
        VerifyScopedRegistered<ISubscribedPublisher, SubscribedPublisher>();
        VerifyScopedRegistered<IQueueFailures, QueueFailures>();
        VerifyScopedRegistered<ISubscribedFailures, SubscribedFailures>();
        VerifyScopedRegistered<IPublishPipelineInvoker, PublishPipelineInvoker>();
        VerifyScopedRegistered<IPublishPipelineFactory, PublishPipelineFactory>();
        VerifyScopedRegistered<IPublishPipeline, PublishPipeline>();
        VerifyScopedRegistered<IPublishPipelineFinalStep, PublishPipelineFinalStep>();
        VerifyScopedRegistered<ISendPipelineInvoker, SendPipelineInvoker>();
        VerifyScopedRegistered<ISendPipelineFactory, SendPipelineFactory>();
        VerifyScopedRegistered<ISendPipeline, SendPipeline>();
        VerifyScopedRegistered<ISendPipelineFinalStep, SendPipelineFinalStep>();
        VerifySingletonRegistered<ISagaMessageMapManager, SagaMessageMapManager>();
        VerifyScopedRegistered<IQueueReader, QueueReader>();
        VerifyScopedRegistered<IQueuePipelineInvoker, QueuePipelineInvoker>();
        VerifyScopedRegistered<IQueuePipelineFactory, QueuePipelineFactory>();
        VerifyScopedRegistered<IQueuePipeline, QueuePipeline>();
        VerifyScopedRegistered<IQueuePipelineFinalStep, QueuePipelineFinalStep>();
        VerifyScopedRegistered<ISubscribedReader, SubscribedReader>();
        VerifyScopedRegistered<ISubscribedPipelineInvoker, SubscribedPipelineInvoker>();
        VerifyScopedRegistered<ISubscribedPipelineFactory, SubscribedPipelineFactory>();
        VerifyScopedRegistered<ISubscribedPipeline, SubscribedPipeline>();
        VerifyScopedRegistered<ISubscribedPipelineFinalStep, SubscribedPipelineFinalStep>();
    }

    private static BusConfiguration CreateBusConfiguration(bool useDefault)
    {
        return new BusConfiguration()
        {
            ConnectionString = "CONNECTIONSTRING",
            Schema = new("PBus"),
            UseDefaultSerialization = useDefault,
            QueueConfiguration = new()
            {
                QueueName = new("QueueName"),
                UseDefaultFailedHandler = useDefault,
                UseDefaultRetryStrategy = useDefault,
            },
            SubscriptionConfiguration = new()
            {
                SubscriberId = new(),
                Topics = [],
                UseDefaultRetryStrategy = useDefault,
                UseDefaultFailedHandler = useDefault,
            }
        };
    }

    [TestMethod]
    public void Given_UseDefaultsTrue_When_Regsiter_Then_DefaultsRegistered()
    {
        _registerComponents.Register(CreateBusConfiguration(true), []);

        VerifySingletonRegistered<ISerializer, DefaultSerializer>();
        VerifyScopedRegistered<IHandleFailedQueueMessages, DefaultFailedQueueMessageHandler>();
        VerifyScopedRegistered<IQueueRetryStrategy, DefaultQueueRetryStrategy>();
        VerifyScopedRegistered<IHandleFailedSubscribedMessages, DefaultFailedSubscribedMessageHandler>();
        VerifyScopedRegistered<ISubscribedRetryStrategy, DefaultSubscribedRetryStrategy>();
    }

    [TestMethod]
    public void Given_UseDefaultsFalse_When_Regsiter_Then_DefaultsRegistered()
    {
        _registerComponents.Register(CreateBusConfiguration(false), []);

        VerifySingletonRegistered<ISerializer, DefaultSerializer>(Times.Never());
        VerifyScopedRegistered<IHandleFailedQueueMessages, DefaultFailedQueueMessageHandler>(Times.Never());
        VerifyScopedRegistered<IQueueRetryStrategy, DefaultQueueRetryStrategy>(Times.Never());
        VerifyScopedRegistered<IHandleFailedSubscribedMessages, DefaultFailedSubscribedMessageHandler>(Times.Never());
        VerifyScopedRegistered<ISubscribedRetryStrategy, DefaultSubscribedRetryStrategy>(Times.Never());
    }

    private void VerifyScopedCollection<TService>(List<Type> implementations)
    {
        _registrationProvider.Verify(p => p.RegisterScoped(
            typeof(TService),
            It.Is<List<Type>>(l => !(l.Except(implementations).Any() || implementations.Except(l).Any()))),
            Times.Once);
    }

    private void VerifyScopedCollectionNever<TService>()
    {
        _registrationProvider.Verify(p => p.RegisterScoped(
            typeof(TService),
            It.IsAny<List<Type>>()),
            Times.Never);
    }

    [TestMethod]
    public void Given_EmptyAssembliesList_When_Register_Then_FindableTypesAreNotRegistered()
    {
        _registerComponents.Register(_basicConfiguration, []);

        VerifyScopedCollection<IPublishPipelineStep>([]);
        VerifyScopedCollection<ISendPipelineStep>([]);
        VerifyScopedCollection<ISubscribedPipelineStep>([]);
        VerifyScopedCollection<IQueuePipelineStep>([]);
        VerifyScopedCollection<IRunOnStartup>([]);

        VerifyScopedCollectionNever<IHandleQueueMessage<QueueMessage1>>();
        VerifyScopedCollectionNever<IHandleQueueMessage<QueueMessage2>>();
        VerifyScopedCollectionNever<IHandleQueueMessage<SagaMessage1>>();
        VerifyScopedCollectionNever<IHandleQueueMessage<SagaMessage2>>();
        VerifyScopedCollectionNever<IHandleSubscribedMessage<SubscribedMessage1>>();
        VerifyScopedCollectionNever<IHandleSubscribedMessage<SubscribedMessage2>>();
    }

    [TestMethod]
    public void Given_AssembliesList_When_Register_Then_FindableTypesAreRegistered()
    {
        List<Assembly> _assembliesToScan = [typeof(QueueMessage1).Assembly];

        _registerComponents.Register(_basicConfiguration, _assembliesToScan);

        VerifyScopedCollection<IPublishPipelineStep>([typeof(PublishPipelineStep1), typeof(PublishPipelineStep2)]);
        VerifyScopedCollection<ISendPipelineStep>([typeof(SendPipelineStep1), typeof(SendPipelineStep2)]);
        VerifyScopedCollection<ISubscribedPipelineStep>([typeof(SubscribedPipelineStep1), typeof(SubscribedPipelineStep2)]);
        VerifyScopedCollection<IQueuePipelineStep>([typeof(QueuePipelineStep1), typeof(QueuePipelineStep2)]);
        VerifyScopedCollection<IRunOnStartup>([typeof(RunOnStartup1), typeof(RunOnStartup2)]);

        VerifyScopedCollection<IHandleQueueMessage<QueueMessage1>>([typeof(HandleQueueMessage1A), typeof(HandleQueueMessage1B)]);
        VerifyScopedCollection<IHandleQueueMessage<QueueMessage2>>([typeof(HandleQueueMessage2A), typeof(HandleQueueMessage2B)]);
        VerifyScopedCollection<IHandleQueueMessage<SagaMessage1>>([typeof(Saga1), typeof(Saga2)]);
        VerifyScopedCollection<IHandleQueueMessage<SagaMessage2>>([typeof(Saga1), typeof(Saga2)]);
        VerifyScopedCollection<IHandleSubscribedMessage<SubscribedMessage1>>([typeof(HandleSubscribedMessage1A), typeof(HandleSubscribedMessage1B)]);
        VerifyScopedCollection<IHandleSubscribedMessage<SubscribedMessage2>>([typeof(HandleSubscribedMessage2A), typeof(HandleSubscribedMessage2B)]);
    }
}
