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

namespace PeachtreeBus.DependencyInjection.Tests;

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

    public abstract IWrappedScope BuildScope();

    public abstract void Then_GetServiceFails<TService>();

    public abstract void AddInstance<TInterface>(TContainer container, TInterface instance);

    protected Action<TContainer>? AddToContainer { get; set; }

    [TestMethod]
    public void When_GetIWrappedScope_Then_ResultIsSelf()
    {
        using var scope = BuildScope();
        var actual = scope.GetInstance<IWrappedScope>();
        Assert.AreSame(scope, actual);
    }

    [TestMethod]
    public void When_GetInstances_Then_ResultIsNotNull()
    {
        using var scope = BuildScope();
        Assert.IsNotNull(scope.GetInstance<IBusConfiguration>());
        Assert.IsNotNull(scope.GetInstance<ITaskCounter>());
        Assert.IsNotNull(scope.GetInstance<ITaskManager>());
        Assert.IsNotNull(scope.GetInstance<ISystemClock>());
        Assert.IsNotNull(scope.GetInstance<IMeters>());
        Assert.IsNotNull(scope.GetInstance<IAlwaysRunTracker>());
        Assert.IsNotNull(scope.GetInstance<IShareObjectsBetweenScopes>());
        Assert.IsNotNull(scope.GetInstance<IDapperTypesHandler>());
        Assert.IsNotNull(scope.GetInstance<IBusDataAccess>());
        Assert.IsNotNull(scope.GetInstance<ISqlConnectionFactory>());
        Assert.IsNotNull(scope.GetInstance<IProvideDbConnectionString>());
        Assert.IsNotNull(scope.GetInstance<IRunStartupTasks>());
        Assert.IsNotNull(scope.GetInstance<IStarters>());
        Assert.IsNotNull(scope.GetInstance<IUpdateSubscriptionsTracker>());
        Assert.IsNotNull(scope.GetInstance<IUpdateSubscriptionsTask>());
        Assert.IsNotNull(scope.GetInstance<IUpdateSubscriptionsStarter>());
        Assert.IsNotNull(scope.GetInstance<IUpdateSubscriptionsRunner>());
        Assert.IsNotNull(scope.GetInstance<ICleanSubscriptionsTracker>());
        Assert.IsNotNull(scope.GetInstance<ICleanSubscriptionsTask>());
        Assert.IsNotNull(scope.GetInstance<ICleanSubscriptionsStarter>());
        Assert.IsNotNull(scope.GetInstance<ICleanSubscriptionsRunner>());
        Assert.IsNotNull(scope.GetInstance<ICleanSubscribedPendingTracker>());
        Assert.IsNotNull(scope.GetInstance<ICleanSubscribedPendingTask>());
        Assert.IsNotNull(scope.GetInstance<ICleanSubscribedPendingStarter>());
        Assert.IsNotNull(scope.GetInstance<ICleanSubscribedPendingRunner>());
        Assert.IsNotNull(scope.GetInstance<ICleanSubscribedCompletedTracker>());
        Assert.IsNotNull(scope.GetInstance<ICleanSubscribedCompletedTask>());
        Assert.IsNotNull(scope.GetInstance<ICleanSubscribedCompletedStarter>());
        Assert.IsNotNull(scope.GetInstance<ICleanSubscribedCompletedRunner>());
        Assert.IsNotNull(scope.GetInstance<ICleanSubscribedFailedTracker>());
        Assert.IsNotNull(scope.GetInstance<ICleanSubscribedFailedTask>());
        Assert.IsNotNull(scope.GetInstance<ICleanSubscribedFailedStarter>());
        Assert.IsNotNull(scope.GetInstance<ICleanSubscribedFailedRunner>());
        Assert.IsNotNull(scope.GetInstance<ICleanQueuedCompletedTracker>());
        Assert.IsNotNull(scope.GetInstance<ICleanQueuedCompletedTask>());
        Assert.IsNotNull(scope.GetInstance<ICleanQueuedCompletedStarter>());
        Assert.IsNotNull(scope.GetInstance<ICleanQueuedCompletedRunner>());
        Assert.IsNotNull(scope.GetInstance<ICleanQueuedFailedTracker>());
        Assert.IsNotNull(scope.GetInstance<ICleanQueuedFailedTask>());
        Assert.IsNotNull(scope.GetInstance<ICleanQueuedFailedStarter>());
        Assert.IsNotNull(scope.GetInstance<ICleanQueuedFailedRunner>());
        Assert.IsNotNull(scope.GetInstance<IProcessSubscribedTask>());
        Assert.IsNotNull(scope.GetInstance<IProcessSubscribedStarter>());
        Assert.IsNotNull(scope.GetInstance<IProcessSubscribedRunner>());
        Assert.IsNotNull(scope.GetInstance<IProcessQueuedTask>());
        Assert.IsNotNull(scope.GetInstance<IProcessQueuedStarter>());
        Assert.IsNotNull(scope.GetInstance<IProcessQueuedRunner>());
        Assert.IsNotNull(scope.GetInstance<IQueueWriter>());
        Assert.IsNotNull(scope.GetInstance<ISubscribedPublisher>());
        Assert.IsNotNull(scope.GetInstance<IQueueFailures>());
        Assert.IsNotNull(scope.GetInstance<ISubscribedFailures>());
        Assert.IsNotNull(scope.GetInstance<IPublishPipelineInvoker>());
        Assert.IsNotNull(scope.GetInstance<IPublishPipelineFactory>());
        Assert.IsNotNull(scope.GetInstance<IPublishPipeline>());
        Assert.IsNotNull(scope.GetInstance<IFindPublishPipelineSteps>());
        Assert.IsNotNull(scope.GetInstance<IPublishPipelineFinalStep>());
        Assert.IsNotNull(scope.GetInstance<ISendPipelineInvoker>());
        Assert.IsNotNull(scope.GetInstance<ISendPipelineFactory>());
        Assert.IsNotNull(scope.GetInstance<ISendPipeline>());
        Assert.IsNotNull(scope.GetInstance<IFindSendPipelineSteps>());
        Assert.IsNotNull(scope.GetInstance<ISendPipelineFinalStep>());
        Assert.IsNotNull(scope.GetInstance<ISagaMessageMapManager>());
        Assert.IsNotNull(scope.GetInstance<IFindQueueHandlers>());
        Assert.IsNotNull(scope.GetInstance<IFindQueuePipelineSteps>());
        Assert.IsNotNull(scope.GetInstance<IQueueReader>());
        Assert.IsNotNull(scope.GetInstance<IQueuePipelineInvoker>());
        Assert.IsNotNull(scope.GetInstance<IQueuePipelineFactory>());
        Assert.IsNotNull(scope.GetInstance<IQueuePipeline>());
        Assert.IsNotNull(scope.GetInstance<IQueuePipelineFinalStep>());
        Assert.IsNotNull(scope.GetInstance<ISubscribedReader>());
        Assert.IsNotNull(scope.GetInstance<ISubscribedPipelineInvoker>());
        Assert.IsNotNull(scope.GetInstance<ISubscribedPipelineFactory>());
        Assert.IsNotNull(scope.GetInstance<ISubscribedPipeline>());
        Assert.IsNotNull(scope.GetInstance<ISubscribedPipelineFinalStep>());
        Assert.IsNotNull(scope.GetInstance<IFindSubscribedPipelineSteps>());
        Assert.IsNotNull(scope.GetInstance<IFindSubscribedHandlers>());
        Assert.IsNotNull(scope.GetInstance<ISerializer>());
        Assert.IsNotNull(scope.GetInstance<IHandleFailedQueueMessages>());
        Assert.IsNotNull(scope.GetInstance<IQueueRetryStrategy>());
        Assert.IsNotNull(scope.GetInstance<IHandleFailedSubscribedMessages>());
        Assert.IsNotNull(scope.GetInstance<ISubscribedRetryStrategy>());
        Assert.IsNotNull(scope.GetInstance<IClassNameService>());
    }

    private void Then_ServiceIs<TInterface, TExpected>() where TInterface : class
    {
        using var scope = BuildScope();
        var actual = scope.GetInstance<TInterface>();
        Assert.AreEqual(typeof(TExpected), actual.GetType());
    }

    private void Then_ServiceIs<TInterface>(object expected) where TInterface : class
    {
        using var scope = BuildScope();
        var actual = scope.GetInstance<TInterface>();
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
        using var scope = BuildScope();

        void Then_TypeIsReturned<TInterface, TImplmentation>() where TInterface : class
        {
            var actual = scope.GetAllInstances<TInterface>();
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
        using var scope = BuildScope();

        void Then_NoTypesReturned<TInterface>() where TInterface : class
        {
            var actual = scope.GetAllInstances<TInterface>();
            Assert.IsNotNull(actual);
            Assert.IsFalse(actual.Any());
        }

        Then_NoTypesReturned<IRunOnStartup>();
        Then_NoTypesReturned<ISubscribedPipelineStep>();
        Then_NoTypesReturned<IQueuePipelineStep>();
        Then_NoTypesReturned<ISendPipelineStep>();
        Then_NoTypesReturned<IPublishPipelineStep>();
    }

    protected abstract void Then_GetHandlersFails<THandler>(IWrappedScope scope) where THandler : class;

    [TestMethod]
    public void Given_NoFindableTypes_When_GetHandlers_Then_NoTypes()
    {
        TestAssemblies = [];
        using var scope = BuildScope();

        Then_GetHandlersFails<IHandleSubscribedMessage<SubscribedMessage>>(scope);
        Then_GetHandlersFails<IHandleQueueMessage<QueueMessage>>(scope);
    }
}
