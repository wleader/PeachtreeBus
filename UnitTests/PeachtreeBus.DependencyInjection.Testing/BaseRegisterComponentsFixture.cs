using Moq;
using PeachtreeBus.ClassNames;
using PeachtreeBus.Data;
using PeachtreeBus.DatabaseSharing;
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
    public void When_GetInstances_Then_ResultIsNotNull()
    {
        using var accessor = BuildAccessor();
        Assert.IsNotNull(accessor.GetService<IBusConfiguration>());
        Assert.IsNotNull(accessor.GetService<ITaskCounter>());
        Assert.IsNotNull(accessor.GetService<ITaskManager>());
        Assert.IsNotNull(accessor.GetService<ISystemClock>());
        Assert.IsNotNull(accessor.GetService<IMeters>());
        Assert.IsNotNull(accessor.GetService<IAlwaysRunTracker>());
        Assert.IsNotNull(accessor.GetService<IShareObjectsBetweenScopes>());
        Assert.IsNotNull(accessor.GetService<IDapperTypesHandler>());
        Assert.IsNotNull(accessor.GetService<IBusDataAccess>());
        Assert.IsNotNull(accessor.GetService<IDapperMethods>());
        Assert.IsNotNull(accessor.GetService<ISqlConnectionFactory>());
        Assert.IsNotNull(accessor.GetService<IProvideDbConnectionString>());
        Assert.IsNotNull(accessor.GetService<IRunStartupTasks>());
        Assert.IsNotNull(accessor.GetService<IStarters>());
        Assert.IsNotNull(accessor.GetService<IUpdateSubscriptionsTracker>());
        Assert.IsNotNull(accessor.GetService<IUpdateSubscriptionsTask>());
        Assert.IsNotNull(accessor.GetService<IUpdateSubscriptionsStarter>());
        Assert.IsNotNull(accessor.GetService<IUpdateSubscriptionsRunner>());
        Assert.IsNotNull(accessor.GetService<ICleanSubscriptionsTracker>());
        Assert.IsNotNull(accessor.GetService<ICleanSubscriptionsTask>());
        Assert.IsNotNull(accessor.GetService<ICleanSubscriptionsStarter>());
        Assert.IsNotNull(accessor.GetService<ICleanSubscriptionsRunner>());
        Assert.IsNotNull(accessor.GetService<ICleanSubscribedPendingTracker>());
        Assert.IsNotNull(accessor.GetService<ICleanSubscribedPendingTask>());
        Assert.IsNotNull(accessor.GetService<ICleanSubscribedPendingStarter>());
        Assert.IsNotNull(accessor.GetService<ICleanSubscribedPendingRunner>());
        Assert.IsNotNull(accessor.GetService<ICleanSubscribedCompletedTracker>());
        Assert.IsNotNull(accessor.GetService<ICleanSubscribedCompletedTask>());
        Assert.IsNotNull(accessor.GetService<ICleanSubscribedCompletedStarter>());
        Assert.IsNotNull(accessor.GetService<ICleanSubscribedCompletedRunner>());
        Assert.IsNotNull(accessor.GetService<ICleanSubscribedFailedTracker>());
        Assert.IsNotNull(accessor.GetService<ICleanSubscribedFailedTask>());
        Assert.IsNotNull(accessor.GetService<ICleanSubscribedFailedStarter>());
        Assert.IsNotNull(accessor.GetService<ICleanSubscribedFailedRunner>());
        Assert.IsNotNull(accessor.GetService<ICleanQueuedCompletedTracker>());
        Assert.IsNotNull(accessor.GetService<ICleanQueuedCompletedTask>());
        Assert.IsNotNull(accessor.GetService<ICleanQueuedCompletedStarter>());
        Assert.IsNotNull(accessor.GetService<ICleanQueuedCompletedRunner>());
        Assert.IsNotNull(accessor.GetService<ICleanQueuedFailedTracker>());
        Assert.IsNotNull(accessor.GetService<ICleanQueuedFailedTask>());
        Assert.IsNotNull(accessor.GetService<ICleanQueuedFailedStarter>());
        Assert.IsNotNull(accessor.GetService<ICleanQueuedFailedRunner>());
        Assert.IsNotNull(accessor.GetService<IProcessSubscribedTask>());
        Assert.IsNotNull(accessor.GetService<IProcessSubscribedStarter>());
        Assert.IsNotNull(accessor.GetService<IProcessSubscribedRunner>());
        Assert.IsNotNull(accessor.GetService<IProcessQueuedTask>());
        Assert.IsNotNull(accessor.GetService<IProcessQueuedStarter>());
        Assert.IsNotNull(accessor.GetService<IProcessQueuedRunner>());
        Assert.IsNotNull(accessor.GetService<IQueueWriter>());
        Assert.IsNotNull(accessor.GetService<ISubscribedPublisher>());
        Assert.IsNotNull(accessor.GetService<IQueueFailures>());
        Assert.IsNotNull(accessor.GetService<ISubscribedFailures>());
        Assert.IsNotNull(accessor.GetService<IPublishPipelineInvoker>());
        Assert.IsNotNull(accessor.GetService<IPublishPipelineFactory>());
        Assert.IsNotNull(accessor.GetService<IPublishPipeline>());
        Assert.IsNotNull(accessor.GetService<IPublishPipelineFinalStep>());
        Assert.IsNotNull(accessor.GetService<ISendPipelineInvoker>());
        Assert.IsNotNull(accessor.GetService<ISendPipelineFactory>());
        Assert.IsNotNull(accessor.GetService<ISendPipeline>());
        Assert.IsNotNull(accessor.GetService<ISendPipelineFinalStep>());
        Assert.IsNotNull(accessor.GetService<ISagaMessageMapManager>());
        Assert.IsNotNull(accessor.GetService<IQueueReader>());
        Assert.IsNotNull(accessor.GetService<IQueuePipelineInvoker>());
        Assert.IsNotNull(accessor.GetService<IQueuePipelineFactory>());
        Assert.IsNotNull(accessor.GetService<IQueuePipeline>());
        Assert.IsNotNull(accessor.GetService<IQueuePipelineFinalStep>());
        Assert.IsNotNull(accessor.GetService<ISubscribedReader>());
        Assert.IsNotNull(accessor.GetService<ISubscribedPipelineInvoker>());
        Assert.IsNotNull(accessor.GetService<ISubscribedPipelineFactory>());
        Assert.IsNotNull(accessor.GetService<ISubscribedPipeline>());
        Assert.IsNotNull(accessor.GetService<ISubscribedPipelineFinalStep>());
        Assert.IsNotNull(accessor.GetService<ISerializer>());
        Assert.IsNotNull(accessor.GetService<IHandleFailedQueueMessages>());
        Assert.IsNotNull(accessor.GetService<IQueueRetryStrategy>());
        Assert.IsNotNull(accessor.GetService<IHandleFailedSubscribedMessages>());
        Assert.IsNotNull(accessor.GetService<ISubscribedRetryStrategy>());
        Assert.IsNotNull(accessor.GetService<IClassNameService>());
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
