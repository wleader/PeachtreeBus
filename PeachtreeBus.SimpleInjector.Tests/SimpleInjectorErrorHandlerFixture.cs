using Moq;
using PeachtreeBus.Queues;
using PeachtreeBus.Subscriptions;
using SimpleInjector;
using System;

namespace PeachtreeBus.SimpleInjector.Tests;

[TestClass]
public class SimpleInjectorErrorHandlerFixture : SimpleInjectorExtensionFixtureBase
{
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