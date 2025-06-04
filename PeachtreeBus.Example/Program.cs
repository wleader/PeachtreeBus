using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using PeachtreeBus.Example.Data;
using PeachtreeBus.Example.Services;
using PeachtreeBus.Example.Subsciptions;
using PeachtreeBus.SimpleInjector;
using PeachtreeBus.Subscriptions;
using PeachtreeBus.Telemetry;
using SimpleInjector;
using SimpleInjector.Lifestyles;
using System;
using System.Diagnostics.CodeAnalysis;

[assembly: ExcludeFromCodeCoverage(Justification = "This is example code.")]

namespace PeachtreeBus.Example
{
    /// <summary>
    /// A program that demonstrates using PeachtreeBus with SimpleInjector.
    /// </summary>
    class Program
    {
        /// <summary>
        /// our IOC Container.
        /// </summary>
        private static readonly Container _container = new();

        static void Main()
        {
            // setup a scoped lifestyle.
            _container.Options.DefaultScopedLifestyle = new AsyncScopedLifestyle();

            // get configuration from appsettings.json
            var configurationBuilder = new ConfigurationBuilder();
            configurationBuilder.AddJsonFile("appsettings.json");
            var configuration = configurationBuilder.Build();
            _container.RegisterSingleton<IConfiguration>(() => configuration);

            // read our connection string from the appsettings configuration.
            var connectionString = configuration.GetConnectionString("PeachtreeBus")
                ?? throw new ApplicationException("A PeachtreeBus connection string is not configured.");

            // log to the console window.
            using ILoggerFactory loggerFactory = LoggerFactory.Create(builder =>
            {
                builder.AddSimpleConsole();
            });


            var busConfiguration = new BusConfiguration()
            {
                // requried. This must always be configured.
                Schema = new("PeachtreeBus"),

                // required. What SQL Server database to use.
                ConnectionString = connectionString,

                // if configured this process should recieve and process messages from this queue.
                QueueConfiguration = new()
                {
                    QueueName = new("SampleQueue"),

                    // Determines if the default IHandleFailedQueueMessages will be registerd.
                    // The default handler does nothing.
                    // If false, you must register your own implementation of IHandleFailedQueueMessages with the container.
                    UseDefaultFailedHandler = true,

                    // Determines if the default IQueueRetryStrategy will be registered.
                    // The Default strategy retries up to 5 times, waiting 5 seconds longer after each failure.
                    // if false you must register your own implementation of IQueueRetryStrategy
                    UseDefaultRetryStrategy = true,

                    // determines which messages to automatically cleaned up.
                    CleanFailed = true,
                    CleanCompleted = true,
                    // determines how old message have to be to be cleaned.
                    CleanFailedAge = TimeSpan.FromDays(7),
                    CleanCompleteAge = TimeSpan.FromDays(1),
                    // how often to perform the cleanup.
                    CleanInterval = TimeSpan.FromMinutes(5),

                },

                // If configured this causes the process to search for and process subscribed messages.
                SubscriptionConfiguration = new()
                {
                    // In a real application, each instance of the process would have a different ID.
                    // this can be random, or managed as needed. 
                    SubscriberId = new SubscriberId(Guid.Parse("E00E876C-A9F4-46C4-B0E7-2B27C525FA98")),

                    // Causes the process to put into the subscriptions table what categories of
                    // published messages it wants to recieve.
                    // If Empty, then the process will subscribe to nothing, and no messages will
                    // be published to the subscriber.
                    Topics = [Topics.Announcements],

                    // When adding or updating the subscriptions table, this determines how long the subscription
                    // is considered valid for. If the subscription is updated, it will be removed after this amount of time.
                    Lifespan = TimeSpan.FromDays(1),

                    // Determines if the default IHandleFailedSubscribedMessages will be registerd.
                    // The default handler does nothing.
                    // If false, you must register your own implementation of IHandleFailedSubscribedMessages with the container.
                    UseDefaultFailedHandler = true,

                    // Determines if the default ISubscribedRetryStrategy will be registered.
                    // The Default strategy retries up to 5 times, waiting 5 seconds longer after each failure.
                    // if false you must register your own implementation of ISubscribedRetryStrategy
                    UseDefaultRetryStrategy = true,

                    // determines which messages to automatically cleaned up.
                    CleanFailed = true,
                    CleanCompleted = true,
                    // determines how old message have to be to be cleaned.
                    CleanFailedAge = TimeSpan.FromDays(7),
                    CleanCompleteAge = TimeSpan.FromDays(1),
                    // how often to perform the cleanup.
                    CleanInterval = TimeSpan.FromMinutes(5),
                },

                PublishConfiguration = new()
                {
                    // When publishing a message to subscribers, this determins how long the message
                    // can stay in the pending messages before it is considered abandoned.
                    Lifespan = TimeSpan.FromDays(1),
                },
            };

            // This will:
            // Register common types required to run the bus,
            // configure which DB Schema, and which Queue will be used.
            // Register Startup tasks found in loaded assemblies.
            _container.UsePeachtreeBus(busConfiguration, loggerFactory);

            // Register some services that are needed by PeachtreeBus
            // These are things you may want to replace in your application.
            // signal shutdown when the process is exiting.
            _container.RegisterSingleton(typeof(IProvideShutdownSignal), typeof(ProcessExitShutdownSignal));

            // Register things needed by the handlers.
            // register the data access used by the sample application.
            _container.Register(typeof(IExampleDataAccess), typeof(ExampleDataAccess), Lifestyle.Scoped);

            // sanity check that everything that the IOC container needs to create
            // objects has been registered.
            _container.Verify();

            // optionally turn on and configure telemetry.
            using var _ = new Telemetry.OpenTelemetryProviders("PeachtreeBus-Example",
                tracerSources: [ActivitySources.Messaging.Name],
                traceExportOptions: options =>
                {
                    //options.Endpoint = new("https://server.domain.com/v1/meters");
                    //options.Protocol = OpenTelemetry.Exporter.OtlpExportProtocol.HttpProtobuf;
                },
                meterSources: [ActivitySources.Messaging.Name],
                meterExportOptions: options =>
                {
                    //options.Endpoint = new("https://server.domain.com/v1/meters");
                    //options.Protocol = OpenTelemetry.Exporter.OtlpExportProtocol.HttpProtobuf;
                });

            // run!
            // this will run different looping threads based on the above code
            // Since UsePeachtreeBusQueue was called, the message queue will be serviced, etc.
            _container.RunPeachtreeBus();
        }
    }
}
