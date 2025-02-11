using PeachtreeBus.Queues;
using PeachtreeBus.Subscriptions;
using System.Linq;
using System.Reflection;

namespace PeachtreeBus.SimpleInjector.Tests;

[TestClass]
public class SimpleInjectorFindPipelinesFixture : SimpleInjectorExtensionFixtureBase
{
    [TestMethod]
    public void Given_QueuePipelines_When_FindPipelines_Then_PipelinesFound()
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
        var findPipelines = scope.GetInstance<IFindQueuePipelineSteps>();

        var actual = findPipelines.FindSteps().ToList();
        Assert.AreEqual(1, actual.Count);
        Assert.AreEqual(typeof(TestQueuePipelineStep), actual[0].GetType());
    }

    [TestMethod]
    public void Given_SubscribedPipelines_When_FindPipelines_Then_PipelinesFound()
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
        var findPipelines = scope.GetInstance<IFindSubscribedPipelineSteps>();

        var actual = findPipelines.FindSteps().ToList();
        Assert.AreEqual(1, actual.Count);
        Assert.AreEqual(typeof(TestSubscribedPipelineStep), actual[0].GetType());
    }

    [TestMethod]
    public void Given_NoQueuePipelines_When_FindPipelines_Then_PipelinesFound()
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
        var findPipelines = scope.GetInstance<IFindQueuePipelineSteps>();

        var actual = findPipelines.FindSteps().ToList();
        Assert.AreEqual(0, actual.Count);
    }

    [TestMethod]
    public void Given_NoSubscribedPipelines_When_FindPipelines_Then_PipelinesFound()
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
        var findPipelines = scope.GetInstance<IFindSubscribedPipelineSteps>();

        var actual = findPipelines.FindSteps().ToList();
        Assert.AreEqual(0, actual.Count);
    }
}
