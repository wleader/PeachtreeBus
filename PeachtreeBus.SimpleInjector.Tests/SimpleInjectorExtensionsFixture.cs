using Microsoft.Extensions.Logging;
using Moq;
using PeachtreeBus.Data;
using PeachtreeBus.DatabaseSharing;
using PeachtreeBus.Queues;
using PeachtreeBus.Subscriptions;
using SimpleInjector;
using SimpleInjector.Lifestyles;
using System;

namespace PeachtreeBus.SimpleInjector.Tests;

[TestClass]
public class SimpleInjectorExtensionsFixture
{
    private Container _container = default!;
    private static readonly SchemaName _schemaName = new("PeachtreeBus");
    private static readonly QueueName _queueName = new("QueueName");

    private ILoggerFactory _loggerFactory = default!;
    private readonly Mock<IProvideDbConnectionString> _provideDBConnectionString = new();
    private Mock<IProvideShutdownSignal> _provideShutdownSignal = default!;
    private static readonly SubscriberId subscriberId = new(Guid.Parse("38dff5e2-b66d-4e01-a5ae-e7fb236708bb"));

    [TestInitialize]
    public void Intialize()
    {
        _container = new Container();
        _container.Options.AllowOverridingRegistrations = true;
        _container.Options.DefaultScopedLifestyle = new AsyncScopedLifestyle();

        // users of peachtree bus must provide their own logging
        _loggerFactory = LoggerFactory.Create(builder =>
        {
            builder.AddSimpleConsole();
        });
        _container.RegisterInstance(_loggerFactory);
        _container.RegisterSingleton(typeof(ILogger<>), typeof(Logger<>));

        // users must provide their own way of configuring the connection string.
        _container.RegisterInstance(_provideDBConnectionString.Object);

        // users must provide their own shutdown signal.
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
    public void When_UsePeachtreeBus_Then_ContainerIsValid()
    {
        _container.UsePeachtreeBus(_schemaName);
        _container.Verify();
    }

    [TestMethod]
    public void When_UsePeachtreeBusSubscriptions_Then_ContainerIsvalid()
    {
        // base components are needed for subscribed message handling.
        _container.UsePeachtreeBus(_schemaName);

        // retry strategies are required for subscribed message handling.
        _container.UsePeachtreeBusDefaultRetryStrategy();

        _container.UsePeachtreeBusSubscriptions(new SubscriberConfiguration(
            subscriberId, TimeSpan.FromSeconds(60), new Category("Announcements")));

        _container.Verify();
    }

    [TestMethod]
    public void When_CleanupSubscribed_Then_ContainerIsvalid()
    {
        // base components are needed for subscribed message handling.
        _container.UsePeachtreeBus(_schemaName);

        _container.CleanupSubscribed(10, true, false, TimeSpan.FromDays(1), TimeSpan.FromMinutes(1));

        _container.Verify();
    }

    [TestMethod]
    public void When_CleanupSubscriptions_Then_ContainerIsvalid()
    {
        // base components are needed for subscribed message handling.
        _container.UsePeachtreeBus(_schemaName);

        _container.CleanupSubscriptions();

        _container.Verify();
    }

    [TestMethod]
    public void When_CleanupQueue_Then_ContainerIsvalid()
    {
        // base components are needed for subscribed message handling.
        _container.UsePeachtreeBus(_schemaName);

        _container.CleanupQueue(_queueName, 10, true, false, TimeSpan.FromDays(1), TimeSpan.FromMinutes(1));

        _container.Verify();
    }

    [TestMethod]
    public void When_UsePeachtreeBusQueue_Then_ContainerIsvalid()
    {
        // base components are needed for queue message handling.
        _container.UsePeachtreeBus(_schemaName);

        // retry strategies are required for queue message handling.
        _container.UsePeachtreeBusDefaultRetryStrategy();

        _container.UsePeachtreeBusQueue(_queueName);

        _container.Verify();
    }

    [TestMethod]
    public void When_RunPeachtreeBus_Then_Runs()
    {
        // base components are needed for queue message handling.
        _container.UsePeachtreeBus(_schemaName);
        _container.UsePeachtreeBusDefaultErrorHandlers();
        _container.Verify();
        _container.RunPeachtreeBus();
    }


    [TestMethod]
    public void When_RunPeachtreeBusStartupTasks_Then_Runs()
    {
        _container.UsePeachtreeBus(_schemaName);
        _container.Verify();
        _container.RunPeachtreeBusStartupTasks();
    }

    [TestMethod]
    public void Given_UseQueue_When_Run_Then_Runs()
    {
        _container.UsePeachtreeBus(_schemaName);
        _container.UsePeachtreeBusDefaultErrorHandlers();
        _container.UsePeachtreeBusQueue(_queueName);
        _container.UsePeachtreeBusDefaultRetryStrategy();
        _container.Verify();
        _container.RunPeachtreeBus();
    }
}