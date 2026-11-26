using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using PeachtreeBus.MicrosoftDependencyInjection;
using PeachtreeBus.Telemetry;

namespace PeachtreeBus.Example;

public abstract class BaseProgram
{
    public async Task Run(string[] args)
    {
        var builder = Host.CreateApplicationBuilder(args);
        builder.ConfigureContainer(
            new DefaultServiceProviderFactory(
                new ServiceProviderOptions
                {
                    ValidateOnBuild = true,
                    ValidateScopes = true
                }));

        // The order that configuration sources are added matters.
        // The last source added wins.
        // Aspire will pass settings like the DB Connection string via Environment Variables.
        builder.Configuration
            .AddJsonFile("appsettings.json") 
            .AddJsonFile("appsettings.overrides.json", true)
            .AddEnvironmentVariables();

        // log to the console window.
        builder.Logging.ClearProviders();
        builder.Logging.AddSimpleConsole();

        // build a bus configuration for PeachtreeBus.
        var connectionString = builder.Configuration.GetConnectionString(ConnectionStringName)
            ?? throw new ApplicationException("A PeachtreeBus connection string is not configured.");
        var busConfiguration = CreateBusConfiguration.Create(connectionString);
        
        // this will register the core PeachtreeBus components with the DI container.
        // It also adds the Microsoft SQL PeachtreeBus components.
        builder.Services.AddPeachtreeBus(busConfiguration, UseDatabase());

        // add application specific services
        RegisterComponents(builder.Services);

        // registers an IHostedService that will run the endpoint.
        builder.HostPeachtreeBus();

        using var host = builder.Build();
        
        // optionally turn on and configure telemetry.
        using var _ = new Telemetry.OpenTelemetryProviders(ServiceName,
            tracerSources: [ActivitySources.Messaging.Name],
            traceExportOptions: _ =>
            {
                //options.Endpoint = new("https://server.domain.com/v1/meters");
                //options.Protocol = OpenTelemetry.Exporter.OtlpExportProtocol.HttpProtobuf;
            },
            meterSources: [Meters.Messaging.Name],
            meterExportOptions: _ =>
            {
                //options.Endpoint = new("https://server.domain.com/v1/meters");
                //options.Protocol = OpenTelemetry.Exporter.OtlpExportProtocol.HttpProtobuf;
            });


        await host.RunAsync();
    }

    protected abstract IRegisterBusDataAccess UseDatabase();

    protected abstract void RegisterComponents(IServiceCollection services);

    protected abstract string ServiceName { get; }
    
    protected abstract string ConnectionStringName { get; }
}