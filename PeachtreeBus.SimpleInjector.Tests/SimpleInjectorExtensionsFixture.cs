using Microsoft.Extensions.Logging;
using Moq;
using PeachtreeBus.DatabaseSharing;
using PeachtreeBus.Subscriptions;
using SimpleInjector;
using SimpleInjector.Lifestyles;
using System.Collections.Generic;
using System.Reflection;

namespace PeachtreeBus.SimpleInjector.Tests;

[TestClass]
public class SimpleInjectorExtensionsFixture
{
    private Container _container = default!;
    private ILoggerFactory _loggerFactory = default!;
    private readonly Mock<IProvideDbConnectionString> _provideDBConnectionString = new();
    private Mock<IProvideShutdownSignal> _provideShutdownSignal = default!;
    private List<Assembly> _assemblies = default!;

    [TestInitialize]
    public void Intialize()
    {
        _assemblies = [Assembly.GetExecutingAssembly()];

        _container = new Container();
        _container.Options.AllowOverridingRegistrations = true;
        _container.Options.DefaultScopedLifestyle = new AsyncScopedLifestyle();

        // users of peachtree bus must provide their own logging
        _loggerFactory = LoggerFactory.Create(builder =>
        {
            builder.AddSimpleConsole();
        });

        // users must provide their own way of configuring the connection string.
        _container.RegisterInstance(_provideDBConnectionString.Object);

        // users must provide their own shutdown signal.
        // provide one that immediatly shuts down so the tests complete.
        _provideShutdownSignal = new();
        _provideShutdownSignal.SetupGet(p => p.ShouldShutdown)
            .Returns(() => true);
        _container.RegisterInstance(_provideShutdownSignal.Object);
    }

    [TestCleanup]
    public void Cleanup()
    {
        _loggerFactory.Dispose();
    }

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
                Categories = [new("Category1"), new("Category2")]
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
            { QueueName = new("QueueName") },
        };

        _container.UsePeachtreeBus(config, _loggerFactory, _assemblies);
        _container.Verify();
        _container.RunPeachtreeBus();
    }

    [TestMethod]
    public void Todo()
    {
        // Tests custom Queue retry strategy
        // Tests custom Subscribed retry strategy
        // Tests custom Queue failed message handler
        // Tests custom Subscribed failed message handler
        // Tests with queue cleaning on and off
        // Tests with subscribed cleaning on and off.
        // Tests with subscription cleaning on and off.
        // Tests that find handlers
        // Tests that find Startup Tasks 
        // Tests that run startup tasks
        // Tests that check pipelines are found.
        Assert.Inconclusive("Tests Missing");
    }
}