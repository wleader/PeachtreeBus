using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using PeachtreeBus.Example.Data;
using PeachtreeBus.Example.Services;
using PeachtreeBus.SimpleInjector;
using PeachtreeBus.Telemetry;
using SimpleInjector;
using SimpleInjector.Lifestyles;
using System;
using System.Diagnostics.CodeAnalysis;

[assembly: ExcludeFromCodeCoverage(Justification = "This is example code.")]

namespace PeachtreeBus.Example.SimpleInjector
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

            var busConfiguration = CreateBusConfiguration.Create(connectionString);

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
