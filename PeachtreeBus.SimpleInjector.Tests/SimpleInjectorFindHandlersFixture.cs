using PeachtreeBus.Queues;
using PeachtreeBus.Subscriptions;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using SIActivationException = SimpleInjector.ActivationException;

namespace PeachtreeBus.SimpleInjector.Tests;

[TestClass]
public class SimpleInjectorFindHandlersFixture : SimpleInjectorExtensionFixtureBase
{
    public class FindableQueuedMessage : IQueueMessage;

    [ExcludeFromCodeCoverage]
    public class FindableQueueMessageHandler : IHandleQueueMessage<FindableQueuedMessage>
    {
        public Task Handle(QueueContext context, FindableQueuedMessage message) => throw new System.NotImplementedException();
    }

    public class FindableSubscribedMessage : ISubscribedMessage;

    [ExcludeFromCodeCoverage]
    public class FindableSubscribedMessageHandler : IHandleSubscribedMessage<FindableSubscribedMessage>
    {
        public Task Handle(SubscribedContext context, FindableSubscribedMessage message) => throw new System.NotImplementedException();
    }

    [TestMethod]
    public void Given_QueueHandlers_When_FindHandlers_Then_HandlersFound()
    {
        var config = new BusConfiguration()
        {
            ConnectionString = "Server=(local);Database=PeachtreeBusExample",
            Schema = new("PeachTreeBus"),
            QueueConfiguration = new()
            {
                QueueName = new("QueueName"),
            },
        };

        _container.UsePeachtreeBus(config, _loggerFactory, [Assembly.GetExecutingAssembly()]);
        _container.Verify();

        var scopeFactory = _container.GetInstance<IWrappedScopeFactory>();
        using var scope = scopeFactory.Create();
        var findHandlers = scope.GetInstance<IFindQueueHandlers>();

        var actual = findHandlers.FindHandlers<FindableQueuedMessage>().ToList();
        Assert.AreEqual(1, actual.Count);
        Assert.AreEqual(typeof(FindableQueueMessageHandler), actual[0].GetType());
    }

    [TestMethod]
    public void Given_NoQueueHandlers_When_FindHandlers_Then_HandlersFound()
    {
        var config = new BusConfiguration()
        {
            ConnectionString = "Server=(local);Database=PeachtreeBusExample",
            Schema = new("PeachTreeBus"),
            QueueConfiguration = new()
            {
                QueueName = new("QueueName"),
            },
        };

        _container.UsePeachtreeBus(config, _loggerFactory, []);
        _container.Verify();

        var scopeFactory = _container.GetInstance<IWrappedScopeFactory>();
        using var scope = scopeFactory.Create();
        var findHandlers = scope.GetInstance<IFindQueueHandlers>();

        Assert.ThrowsException<SIActivationException>(() =>
            _ = findHandlers.FindHandlers<FindableQueuedMessage>());
    }

    [TestMethod]
    public void Given_SubscribedHandlers_When_FindHandlers_Then_HandlersFound()
    {
        var config = new BusConfiguration()
        {
            ConnectionString = "Server=(local);Database=PeachtreeBusExample",
            Schema = new("PeachTreeBus"),
            SubscriptionConfiguration = new()
            {
                Categories = [],
                SubscriberId = SubscriberId.New()
            }
        };

        _container.UsePeachtreeBus(config, _loggerFactory, [Assembly.GetExecutingAssembly()]);
        _container.Verify();

        var scopeFactory = _container.GetInstance<IWrappedScopeFactory>();
        using var scope = scopeFactory.Create();
        var findHandlers = scope.GetInstance<IFindSubscribedHandlers>();

        var actual = findHandlers.FindHandlers<FindableSubscribedMessage>().ToList();
        Assert.AreEqual(1, actual.Count);
        Assert.AreEqual(typeof(FindableSubscribedMessageHandler), actual[0].GetType());
    }

    [TestMethod]
    public void Given_NoSubscribedHandlers_When_FindHandlers_Then_HandlersFound()
    {
        var config = new BusConfiguration()
        {
            ConnectionString = "Server=(local);Database=PeachtreeBusExample",
            Schema = new("PeachTreeBus"),
            SubscriptionConfiguration = new()
            {
                Categories = [],
                SubscriberId = SubscriberId.New()
            }
        };

        _container.UsePeachtreeBus(config, _loggerFactory, []);
        _container.Verify();

        var scopeFactory = _container.GetInstance<IWrappedScopeFactory>();
        using var scope = scopeFactory.Create();
        var findHandlers = scope.GetInstance<IFindSubscribedHandlers>();

        Assert.ThrowsException<SIActivationException>(() =>
            _ = findHandlers.FindHandlers<FindableSubscribedMessage>());
    }
}
