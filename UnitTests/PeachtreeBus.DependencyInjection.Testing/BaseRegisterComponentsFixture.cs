using Moq;
using PeachtreeBus.ClassNames;
using PeachtreeBus.Data;
using PeachtreeBus.DatabaseSharing;
using PeachtreeBus.Errors;
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

namespace PeachtreeBus.DependencyInjection.Testing;

public abstract class BaseRegisterComponentsFixture<TContainer>
{
    protected IBusConfiguration BusConfiguration = default!;

    [TestInitialize]
    public void Initialize()
    {
        BusConfiguration = new BusConfiguration()
        {
            ConnectionString = "ConnectionString",
            Schema = new("PBus")
        };
    }

    protected List<Assembly> TestAssemblies = [Assembly.GetExecutingAssembly()];

    public abstract IServiceProviderAccessor BuildAccessor();

    public abstract void Then_GetServiceFails<TService>();

    public abstract void AddInstance<TInterface>(TContainer container, TInterface instance);

    protected Action<TContainer>? AddToContainer { get; set; }

    [TestMethod]
    public void When_GetIWrappedScope_Then_ResultIsSelf()
    {
        using var accessor = BuildAccessor();
        var actual = accessor.GetService<IServiceProviderAccessor>();
        Assert.AreSame(accessor, actual);
    }

    [TestMethod]
    public void When_GetService_Then_ResultIsNotNull()
    {
        using var accessor = BuildAccessor();

        void AssertService<T>() => 
            Assert.IsNotNull(accessor.GetService<T>(), $"Type {typeof(T).Name} not registered.");

        AssertService<IMessagingSession>();
        AssertService<IBusContextAccessor>();
        AssertService<IBusConfiguration>();
        AssertService<IMessagingTaskCounter>();
        AssertService<IScheduledTaskCounter>();
        AssertService<ITaskManager>();
        AssertService<ICurrentTasks>();
        AssertService<IDelayFactory>();
        AssertService<ISystemClock>();
        AssertService<IMeters>();
        AssertService<IAlwaysRunTracker>();
        AssertService<IShareObjectsBetweenScopes>();
        AssertService<IDapperTypesHandler>();
        AssertService<IBusDataAccess>();
        AssertService<IDapperMethods>();
        AssertService<ISqlConnectionFactory>();
        AssertService<IProvideDbConnectionString>();
        AssertService<IRunStartupTasks>();
        AssertService<IStarters>();
        AssertService<IUpdateSubscriptionsTracker>();
        AssertService<IUpdateSubscriptionsTask>();
        AssertService<IUpdateSubscriptionsStarter>();
        AssertService<IUpdateSubscriptionsRunner>();
        AssertService<ICleanSubscriptionsTracker>();
        AssertService<ICleanSubscriptionsTask>();
        AssertService<ICleanSubscriptionsStarter>();
        AssertService<ICleanSubscriptionsRunner>();
        AssertService<ICleanSubscribedPendingTracker>();
        AssertService<ICleanSubscribedPendingTask>();
        AssertService<ICleanSubscribedPendingStarter>();
        AssertService<ICleanSubscribedPendingRunner>();
        AssertService<ICleanSubscribedCompletedTracker>();
        AssertService<ICleanSubscribedCompletedTask>();
        AssertService<ICleanSubscribedCompletedStarter>();
        AssertService<ICleanSubscribedCompletedRunner>();
        AssertService<ICleanSubscribedFailedTracker>();
        AssertService<ICleanSubscribedFailedTask>();
        AssertService<ICleanSubscribedFailedStarter>();
        AssertService<ICleanSubscribedFailedRunner>();
        AssertService<ICleanQueuedCompletedTracker>();
        AssertService<ICleanQueuedCompletedTask>();
        AssertService<ICleanQueuedCompletedStarter>();
        AssertService<ICleanQueuedCompletedRunner>();
        AssertService<ICleanQueuedFailedTracker>();
        AssertService<ICleanQueuedFailedTask>();
        AssertService<ICleanQueuedFailedStarter>();
        AssertService<ICleanQueuedFailedRunner>();
        AssertService<IProcessSubscribedTask>();
        AssertService<IProcessSubscribedStarter>();
        AssertService<IProcessSubscribedRunner>();
        AssertService<IProcessQueuedTask>();
        AssertService<IProcessQueuedStarter>();
        AssertService<IProcessQueuedRunner>();
        AssertService<IQueueWriter>();
        AssertService<ISubscribedPublisher>();
        AssertService<IQueueFailures>();
        AssertService<ISubscribedFailures>();
        AssertService<IPublishPipelineInvoker>();
        AssertService<IPublishPipelineFactory>();
        AssertService<IPublishPipeline>();
        AssertService<IPublishPipelineFinalStep>();
        AssertService<ISendPipelineInvoker>();
        AssertService<ISendPipelineFactory>();
        AssertService<ISendPipeline>();
        AssertService<ISendPipelineFinalStep>();
        AssertService<ISagaMessageMapManager>();
        AssertService<IQueueReader>();
        AssertService<IQueuePipelineInvoker>();
        AssertService<IQueuePipelineFactory>();
        AssertService<IQueuePipeline>();
        AssertService<IQueuePipelineFinalStep>();
        AssertService<ISubscribedReader>();
        AssertService<ISubscribedPipelineInvoker>();
        AssertService<ISubscribedPipelineFactory>();
        AssertService<ISubscribedPipeline>();
        AssertService<ISubscribedPipelineFinalStep>();
        AssertService<ISerializer>();
        AssertService<IHandleFailedQueueMessages>();
        AssertService<IQueueRetryStrategy>();
        AssertService<IHandleFailedSubscribedMessages>();
        AssertService<ISubscribedRetryStrategy>();
        AssertService<IClassNameService>();
        AssertService<IAlwaysOneEstimator>();
        AssertService<IProcessQueuedEstimator>();
        AssertService<IProcessSubscribedEstimator>();
        AssertService<ICircuitBreakerProvider>();
        AssertService<ICircuitBreakerConfigurationProvider>();
    }

    private void Then_ServiceIs<TInterface, TExpected>() where TInterface : class
    {
        using var accessor = BuildAccessor();
        var actual = accessor.GetService<TInterface>();
        Assert.IsNotNull(actual);
        Assert.AreEqual(typeof(TExpected), actual.GetType());
    }

    private void Then_ServiceIs<TInterface>(object expected) where TInterface : class
    {
        using var accessor = BuildAccessor();
        var actual = accessor.GetService<TInterface>();
        Assert.AreSame(expected, actual);
    }

    private void Given_Mock_Then_ServiceIsMock<TInterface>() where TInterface : class
    {
        var mock = new Mock<TInterface>();
        AddToContainer = c =>
        {
            AddInstance(c, mock.Object);
        };
        Then_ServiceIs<TInterface>(mock.Object);
    }

    [TestMethod]
    public void When_GetIClassNameService_Then_IsCachedClassNameService()
    {
        Then_ServiceIs<IClassNameService, CachedClassNameService>();
    }

    private void Given_QueueUseDefaultFailedHandler(bool value)
    {
        BusConfiguration = new BusConfiguration()
        {
            ConnectionString = "Server=(local);Database=PeachtreeBusExample",
            Schema = new("PeachTreeBus"),
            QueueConfiguration = new()
            {
                QueueName = new("QueueName"),
                UseDefaultFailedHandler = value,
            },
        };
    }

    [TestMethod]
    public void Given_QueueUseDefaultFailedHandlerFalse_And_NoHandlerRegistered_When_Verify_Then_Throws()
    {
        Given_QueueUseDefaultFailedHandler(false);
        Then_GetServiceFails<IHandleFailedQueueMessages>();
    }

    [TestMethod]
    public void Given_QueueUseDefaultFailedHandlerFalse_And_HandlerRegistered_When_Verify_Then_Handler()
    {
        Given_QueueUseDefaultFailedHandler(false);
        Given_Mock_Then_ServiceIsMock<IHandleFailedQueueMessages>();
    }

    [TestMethod]
    public void Given_QueueUseDefaultFailedHandlerTrue_When_Verify_Then_DefaultHandlerIsUsed()
    {
        Given_QueueUseDefaultFailedHandler(true);
        Then_ServiceIs<IHandleFailedQueueMessages, DefaultFailedQueueMessageHandler>();
    }

    [TestMethod]
    public void Given_QueueConfigurationNull_When_Verify_Then_DefaultHandlerIsUsed()
    {
        Assert.IsNull(BusConfiguration.QueueConfiguration);
        Then_ServiceIs<IHandleFailedQueueMessages, DefaultFailedQueueMessageHandler>();
    }

    private void Given_SubscribedUseDefaultFailedHandler(bool value)
    {
        BusConfiguration = new BusConfiguration()
        {
            ConnectionString = "Server=(local);Database=PeachtreeBusExample",
            Schema = new("PeachTreeBus"),
            SubscriptionConfiguration = new()
            {
                Topics = [],
                SubscriberId = new(),
                UseDefaultFailedHandler = value,
            }
        };
    }

    [TestMethod]
    public void Given_SubscribedUseDefaultFailedHandlerFalse_And_NoHandlerRegistered_When_Verify_Then_Throws()
    {
        Given_SubscribedUseDefaultFailedHandler(false);
        Then_GetServiceFails<IHandleFailedSubscribedMessages>();
    }

    [TestMethod]
    public void Given_SubscribedUseDefaultFailedHandlerFalse_And_HandlerRegistered_When_Verify_Then_Handler()
    {
        Given_SubscribedUseDefaultFailedHandler(false);
        Given_Mock_Then_ServiceIsMock<IHandleFailedSubscribedMessages>();
    }

    [TestMethod]
    public void Given_SubscribedUseDefaulFailedHandlerTrue_When_Verify_Then_DefaultHandlerIsUsed()
    {
        Given_SubscribedUseDefaultFailedHandler(true);
        Then_ServiceIs<IHandleFailedSubscribedMessages, DefaultFailedSubscribedMessageHandler>();
    }

    [TestMethod]
    public void Given_SubscribedConfigurationNull_When_Verify_Then_DefaultHandlerIsUsed()
    {
        Assert.IsNull(BusConfiguration.SubscriptionConfiguration);
        Then_ServiceIs<IHandleFailedSubscribedMessages, DefaultFailedSubscribedMessageHandler>();
    }


    private void Given_QueueUseDefaultRetryStrategy(bool value)
    {
        BusConfiguration = new BusConfiguration()
        {
            ConnectionString = "Server=(local);Database=PeachtreeBusExample",
            Schema = new("PeachTreeBus"),
            QueueConfiguration = new()
            {
                QueueName = new("QueueName"),
                UseDefaultRetryStrategy = value,
            },
        };
    }

    [TestMethod]
    public void Given_QueueUseDefaultRetryStrategyFalse_And_NoHandlerRegistered_When_Verify_Then_Throws()
    {
        Given_QueueUseDefaultRetryStrategy(false);
        Then_GetServiceFails<IQueueRetryStrategy>();
    }

    [TestMethod]
    public void Given_QueueUseDefaultRetryStrategyFalse_And_HandlerRegistered_When_Verify_Then_RetryStrategy()
    {
        Given_QueueUseDefaultRetryStrategy(false);
        Given_Mock_Then_ServiceIsMock<IQueueRetryStrategy>();
    }

    [TestMethod]
    public void Given_QueueUseDefaultRetryStrategyTrue_When_Verify_Then_DefaultRetryStrategyIsUsed()
    {
        Given_QueueUseDefaultRetryStrategy(true);
        Then_ServiceIs<IQueueRetryStrategy, DefaultQueueRetryStrategy>();
    }

    [TestMethod]
    public void Given_QueueConfigurationNull_When_Verify_Then_DefaultRetryStrategyIsUsed()
    {
        Assert.IsNull(BusConfiguration.QueueConfiguration);
        Then_ServiceIs<IQueueRetryStrategy, DefaultQueueRetryStrategy>();
    }

    private void Given_SubscribedUseDefaultRetryStrategy(bool value)
    {
        BusConfiguration = new BusConfiguration()
        {
            ConnectionString = "Server=(local);Database=PeachtreeBusExample",
            Schema = new("PeachTreeBus"),
            SubscriptionConfiguration = new()
            {
                Topics = [],
                SubscriberId = new(),
                UseDefaultRetryStrategy = value,
            }
        };
    }

    [TestMethod]
    public void Given_SubscribedUseDefaultRetryStrategyFalse_And_NoRetryStrategyRegistered_When_Verify_Then_Throws()
    {
        Given_SubscribedUseDefaultRetryStrategy(false);
        Then_GetServiceFails<ISubscribedRetryStrategy>();
    }

    [TestMethod]
    public void Given_SubscribedUseDefaultRetryStrategyFalse_And_RetryStrategyRegistered_When_Verify_Then_RetryStrategy()
    {
        Given_SubscribedUseDefaultRetryStrategy(false);
        Given_Mock_Then_ServiceIsMock<ISubscribedRetryStrategy>();
    }

    [TestMethod]
    public void Given_SubscribedUseDefaultRetryStrategyTrue_When_Verify_Then_DefaultRetryStrategyIsUsed()
    {
        Given_SubscribedUseDefaultRetryStrategy(true);
        Then_ServiceIs<ISubscribedRetryStrategy, DefaultSubscribedRetryStrategy>();
    }

    [TestMethod]
    public void Given_SubscribedConfigurationNull_When_Verify_Then_DefaultRetryStrategyIsUsed()
    {
        Assert.IsNull(BusConfiguration.SubscriptionConfiguration);
        Then_ServiceIs<ISubscribedRetryStrategy, DefaultSubscribedRetryStrategy>();
    }

    [TestMethod]
    public void Given_FindableTypes_When_GetAllInstances_Then_ContainsFindableType()
    {
        CollectionAssert.Contains(TestAssemblies, Assembly.GetExecutingAssembly());
        using var accessor = BuildAccessor();

        void Then_TypeIsReturned<TInterface, TImplmentation>() where TInterface : class
        {
            var actual = accessor.GetServices<TInterface>();
            Assert.IsNotNull(actual);
            Assert.IsNotNull(actual.FirstOrDefault(x => x.GetType() == typeof(TImplmentation)));
        }

        Then_TypeIsReturned<IRunOnStartup, RunOnStartup>();
        Then_TypeIsReturned<ISubscribedPipelineStep, SubscribedPipelineStep>();
        Then_TypeIsReturned<IQueuePipelineStep, QueuePipelineStep>();
        Then_TypeIsReturned<ISendPipelineStep, SendPipelineStep>();
        Then_TypeIsReturned<IPublishPipelineStep, PublishPipelineStep>();
        Then_TypeIsReturned<IHandleSubscribedMessage<SubscribedMessage>, SubscribedHandler>();
        Then_TypeIsReturned<IHandleQueueMessage<QueueMessage>, QueueHandler>();
    }

    [TestMethod]
    public void Given_NoFindableTypes_When_GetAllInstances_Then_NoTypes()
    {
        TestAssemblies = [];
        using var accessor = BuildAccessor();

        void Then_NoTypesReturned<TInterface>() where TInterface : class
        {
            var actual = accessor.GetServices<TInterface>();
            Assert.IsNotNull(actual);
            Assert.IsFalse(actual.Any());
        }

        Then_NoTypesReturned<IRunOnStartup>();
        Then_NoTypesReturned<ISubscribedPipelineStep>();
        Then_NoTypesReturned<IQueuePipelineStep>();
        Then_NoTypesReturned<ISendPipelineStep>();
        Then_NoTypesReturned<IPublishPipelineStep>();
    }

    protected abstract void Then_GetHandlersReturnsEmpty<THandler>(IServiceProviderAccessor accessor) where THandler : class;

    [TestMethod]
    public void Given_NoFindableTypes_When_GetHandlers_Then_NoTypes()
    {
        TestAssemblies = [];
        using var accessor = BuildAccessor();

        Then_GetHandlersReturnsEmpty<IHandleSubscribedMessage<SubscribedMessage>>(accessor);
        Then_GetHandlersReturnsEmpty<IHandleQueueMessage<QueueMessage>>(accessor);
    }
}
