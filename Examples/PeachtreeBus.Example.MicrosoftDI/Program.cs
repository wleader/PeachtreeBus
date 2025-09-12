using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using PeachtreeBus.Example;
using PeachtreeBus.Example.Data;
using PeachtreeBus.MicrosoftDependencyInjection;
using PeachtreeBus.Telemetry;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;

[assembly: ExcludeFromCodeCoverage(Justification = "This is example code.")]

namespace Peachtreebus.Example.MicrosoftDI;

public class Program
{
    public static async Task Main(string[] args)
    {
        var configurationBuilder = new ConfigurationBuilder();
        configurationBuilder.AddJsonFile("appsettings.json");
        var configuration = configurationBuilder.Build();

        // read our connection string from the appsettings configuration.
        var connectionString = configuration.GetConnectionString("PeachtreeBus")
            ?? throw new ApplicationException("A PeachtreeBus connection string is not configured.");

        var builder = Host.CreateApplicationBuilder(args);
        builder.ConfigureContainer(
            new DefaultServiceProviderFactory(
                new ServiceProviderOptions
                {
                    ValidateOnBuild = true,
                    ValidateScopes = true
                }));

        // get configuration from appsettings.json
        builder.Configuration.AddConfiguration(configuration);

        // log to the console window.
        builder.Logging.ClearProviders();
        builder.Logging.AddSimpleConsole();

        var busConfiguration = CreateBusConfiguration.Create(connectionString);

        // registers PeachtreeBus components with the container.
        builder.Services.AddPeachtreeBus(busConfiguration);

        // add application specific services
        builder.Services.AddScoped(typeof(IExampleDataAccess), typeof(ExampleDataAccess));

        // registers an IHostedService that will run the endpoint.
        builder.HostPeachtreeBus();

        using var host = builder.Build();


        // optionally turn on and configure telemetry.
        using var _ = new PeachtreeBus.Example.Telemetry.OpenTelemetryProviders("PeachtreeBus-Example-MSDI",
            tracerSources: [ActivitySources.Messaging.Name],
            traceExportOptions: options =>
            {
                //options.Endpoint = new("https://server.domain.com/v1/meters");
                //options.Protocol = OpenTelemetry.Exporter.OtlpExportProtocol.HttpProtobuf;
            },
            meterSources: [Meters.Messaging.Name],
            meterExportOptions: options =>
            {
                //options.Endpoint = new("https://server.domain.com/v1/meters");
                //options.Protocol = OpenTelemetry.Exporter.OtlpExportProtocol.HttpProtobuf;
            });


        await host.RunAsync();//.ConfigureAwait(false);
    }
}
