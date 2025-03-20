using Moq;
using PeachtreeBus.Queues;
using PeachtreeBus.Subscriptions;
using System;

namespace PeachtreeBus.SimpleInjector.Tests;

[TestClass]
public class SimpleInjectorExtensionsFixture : SimpleInjectorExtensionFixtureBase
{
    [TestMethod]
    public void Given_BasicConfiguration_When_Verify_Then_Runs()
    {
        var config = new BusConfiguration()
        {
            ConnectionString = "Server=(local);Database=PeachtreeBusExample",
            Schema = new("PeachTreeBus"),
        };

        _container.UsePeachtreeBus(config, _loggerFactory, _assemblies);
        _container.Verify();
        _container.RunPeachtreeBus();
    }

    [TestMethod]
    public void Given_Subscriptions_When_Verify_Then_Runs()
    {
        var config = new BusConfiguration()
        {
            ConnectionString = "Server=(local);Database=PeachtreeBusExample",
            Schema = new("PeachTreeBus"),
            SubscriptionConfiguration = new()
            {
                SubscriberId = SubscriberId.New(),
                Topics = [new("Topic1"), new("Topic2")]
            },
        };

        _container.UsePeachtreeBus(config, _loggerFactory, _assemblies);
        _container.Verify();
        _container.RunPeachtreeBus();
    }

    [TestMethod]
    public void Given_Queues_When_Verify_Then_Runs()
    {
        var config = new BusConfiguration()
        {
            ConnectionString = "Server=(local);Database=PeachtreeBusExample",
            Schema = new("PeachTreeBus"),
            QueueConfiguration = new()
            {
                QueueName = new("QueueName"),
            }
        };

        _container.UsePeachtreeBus(config, _loggerFactory, _assemblies);
        _container.Verify();
        _container.RunPeachtreeBus();
    }

    [TestMethod]
    public void Given_UseDefaultQueueRetryStrategyFalse_And_NoStrategyRegistered_When_Verify_Then_Throws()
    {
        var config = new BusConfiguration()
        {
            ConnectionString = "Server=(local);Database=PeachtreeBusExample",
            Schema = new("PeachTreeBus"),
            QueueConfiguration = new()
            {
                QueueName = new("QueueName"),
                UseDefaultRetryStrategy = false,
            },
        };

        _container.UsePeachtreeBus(config, _loggerFactory, _assemblies);
        Assert.ThrowsException<InvalidOperationException>(_container.Verify);
    }

    [TestMethod]
    public void Given_UseDefaultQueueRetryStrategyFalse_And_StrategyRegistered_When_Verify_Then_NoThrows()
    {
        var config = new BusConfiguration()
        {
            ConnectionString = "Server=(local);Database=PeachtreeBusExample",
            Schema = new("PeachTreeBus"),
            QueueConfiguration = new()
            {
                QueueName = new("QueueName"),
                UseDefaultRetryStrategy = false,
            },
        };

        _container.UsePeachtreeBus(config, _loggerFactory, _assemblies);

        var strategy = new Mock<IQueueRetryStrategy>();
        _container.RegisterInstance(strategy.Object);
        _container.Verify();
    }

    [TestMethod]
    public void Given_UseDefaultQueueRetryStrategyTrue_When_Verify_Then_DefaultStrategyIsUsed()
    {
        var config = new BusConfiguration()
        {
            ConnectionString = "Server=(local);Database=PeachtreeBusExample",
            Schema = new("PeachTreeBus"),
            QueueConfiguration = new()
            { QueueName = new("QueueName") },
        };

        _container.UsePeachtreeBus(config, _loggerFactory, _assemblies);
        _container.Verify();
        var strategey = _container.GetInstance<IQueueRetryStrategy>();
        Assert.AreEqual(typeof(DefaultQueueRetryStrategy), strategey.GetType());
    }

    [TestMethod]
    public void Given_UseDefaultRetryStrategyFalse_And_NoStrategyRegistered_When_Verify_Then_Throws()
    {
        var config = new BusConfiguration()
        {
            ConnectionString = "Server=(local);Database=PeachtreeBusExample",
            Schema = new("PeachTreeBus"),
            SubscriptionConfiguration = new()
            {
                Topics = [],
                SubscriberId = new(),
                UseDefaultRetryStrategy = false,
            }
        };

        _container.UsePeachtreeBus(config, _loggerFactory, _assemblies);
        Assert.ThrowsException<InvalidOperationException>(_container.Verify);
    }

    [TestMethod]
    public void Given_UseDefaultSubscribedRetryStrategyFalse_And_StrategyRegistered_When_Verify_Then_NoThrows()
    {
        var config = new BusConfiguration()
        {
            ConnectionString = "Server=(local);Database=PeachtreeBusExample",
            Schema = new("PeachTreeBus"),
            SubscriptionConfiguration = new()
            {
                Topics = [],
                SubscriberId = new(),
                UseDefaultRetryStrategy = false,
            }
        };

        _container.UsePeachtreeBus(config, _loggerFactory, _assemblies);

        var strategy = new Mock<ISubscribedRetryStrategy>();
        _container.RegisterInstance(strategy.Object);
        _container.Verify();
    }

    [TestMethod]
    public void Given_UseDefaultSubscribedRetryStrategyTrue_When_Verify_Then_DefaultStrategyIsUsed()
    {
        var config = new BusConfiguration()
        {
            ConnectionString = "Server=(local);Database=PeachtreeBusExample",
            Schema = new("PeachTreeBus"),
            SubscriptionConfiguration = new()
            {
                Topics = [],
                SubscriberId = new(),
                UseDefaultRetryStrategy = true,
            }
        };

        _container.UsePeachtreeBus(config, _loggerFactory, _assemblies);
        _container.Verify();
        var strategey = _container.GetInstance<ISubscribedRetryStrategy>();
        Assert.AreEqual(typeof(DefaultSubscribedRetryStrategy), strategey.GetType());
    }

    [TestMethod]
    public void Given_UseDefaultQueueFailedHandlerFalse_And_NoHandlerRegistered_When_Verify_Then_Throws()
    {
        var config = new BusConfiguration()
        {
            ConnectionString = "Server=(local);Database=PeachtreeBusExample",
            Schema = new("PeachTreeBus"),
            QueueConfiguration = new()
            {
                QueueName = new("QueueName"),
                UseDefaultFailedHandler = false,
            },
        };

        _container.UsePeachtreeBus(config, _loggerFactory, _assemblies);
        Assert.ThrowsException<InvalidOperationException>(_container.Verify);
    }

    [TestMethod]
    public void Given_UseDefaultFailedHandlerFalse_And_HandlerRegistered_When_Verify_Then_NoThrows()
    {
        var config = new BusConfiguration()
        {
            ConnectionString = "Server=(local);Database=PeachtreeBusExample",
            Schema = new("PeachTreeBus"),
            QueueConfiguration = new()
            {
                QueueName = new("QueueName"),
                UseDefaultFailedHandler = false,
            },
        };

        _container.UsePeachtreeBus(config, _loggerFactory, _assemblies);

        var strategy = new Mock<IHandleFailedQueueMessages>();
        _container.RegisterInstance(strategy.Object);
        _container.Verify();
    }

    [TestMethod]
    public void Given_UseDefaultQueueFailedHandler_When_Verify_Then_DefaultStrategyIsUsed()
    {
        var config = new BusConfiguration()
        {
            ConnectionString = "Server=(local);Database=PeachtreeBusExample",
            Schema = new("PeachTreeBus"),
            QueueConfiguration = new()
            {
                QueueName = new("QueueName"),
                UseDefaultFailedHandler = true,
            },
        };

        _container.UsePeachtreeBus(config, _loggerFactory, _assemblies);
        _container.Verify();
        var strategey = _container.GetRegistration<IHandleFailedQueueMessages>();
        Assert.AreEqual(typeof(DefaultFailedQueueMessageHandler), strategey?.ImplementationType);
    }

    [TestMethod]
    public void Given_UseDefaultSubscribedFailedHandlerFalse_And_NoHandlerRegistered_When_Verify_Then_Throws()
    {
        var config = new BusConfiguration()
        {
            ConnectionString = "Server=(local);Database=PeachtreeBusExample",
            Schema = new("PeachTreeBus"),
            SubscriptionConfiguration = new()
            {
                Topics = [],
                SubscriberId = new(),
                UseDefaultFailedHandler = false,
            }
        };

        _container.UsePeachtreeBus(config, _loggerFactory, _assemblies);
        Assert.ThrowsException<InvalidOperationException>(_container.Verify);
    }

    [TestMethod]
    public void Given_UseDefaultSubscribedFailedHandlerFalse_And_HandlerRegistered_When_Verify_Then_NoThrows()
    {
        var config = new BusConfiguration()
        {
            ConnectionString = "Server=(local);Database=PeachtreeBusExample",
            Schema = new("PeachTreeBus"),
            SubscriptionConfiguration = new()
            {
                Topics = [],
                SubscriberId = new(),
                UseDefaultFailedHandler = false,
            }
        };

        _container.UsePeachtreeBus(config, _loggerFactory, _assemblies);

        var strategy = new Mock<IHandleFailedSubscribedMessages>();
        _container.RegisterInstance(strategy.Object);
        _container.Verify();
    }

    [TestMethod]
    public void Given_UseDefaultSubscribedErrorHandlerTrue_When_Verify_Then_DefaultStrategyIsUsed()
    {
        var config = new BusConfiguration()
        {
            ConnectionString = "Server=(local);Database=PeachtreeBusExample",
            Schema = new("PeachTreeBus"),
            SubscriptionConfiguration = new()
            {
                Topics = [],
                SubscriberId = new(),
                UseDefaultFailedHandler = true,
            }
        };

        _container.UsePeachtreeBus(config, _loggerFactory, _assemblies);
        _container.Verify();
        var strategey = _container.GetRegistration<IHandleFailedSubscribedMessages>();
        Assert.AreEqual(typeof(DefaultFailedSubscribedMessageHandler), strategey?.ImplementationType);
    }
}