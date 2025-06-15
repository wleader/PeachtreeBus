using OpenTelemetry;
using OpenTelemetry.Exporter;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using PeachtreeBus.Telemetry;
using System;
using System.Runtime.CompilerServices;

namespace PeachtreeBus.Example.Telemetry;

public class OpenTelemetryProviders(
    string serviceName,
    string[]? tracerSources = null,
    Action<OtlpExporterOptions>? traceExportOptions = null,
    string[]? meterSources = null,
    Action<OtlpExporterOptions>? meterExportOptions = null)
    : IDisposable
{
    private readonly TracerProvider _traceProvider = BuildTracerProvider(
        serviceName, tracerSources, traceExportOptions);

    private readonly MeterProvider _meterProvider = BuildMeterProvide(
        serviceName, meterSources, meterExportOptions);

    private static readonly string[] DefaultTracerSources =
    [
        ActivitySources.Messaging.Name,
        //ActivitySources.User.Name,

        // these are chatty.
        //ActivitySources.DataAccess.Name,
        //ActivitySources.Internal.Name,
    ];

    private static readonly string[] DefaultMeterSources =
    [
        ActivitySources.Messaging.Name,

        // these don't have any meters yet
        //ActivitySources.User.Name,
        //ActivitySources.DataAccess.Name,
        //ActivitySources.Internal.Name,
    ];

    private static void DefaultExporterOptions(OtlpExporterOptions options) { }

    private static TracerProvider BuildTracerProvider(
        string serviceName,
        string[]? sources,
        Action<OtlpExporterOptions>? options)
    {
        return Sdk.CreateTracerProviderBuilder()
            .ConfigureResource(r => r.AddService(serviceName))
            .AddSource(sources ?? DefaultTracerSources)
            //.AddConsoleExporter()
            .AddOtlpExporter(options ?? DefaultExporterOptions)
            .Build();
    }

    private static MeterProvider BuildMeterProvide(
        string serviceName,
        string[]? sources,
        Action<OtlpExporterOptions>? options)
    {
        return Sdk.CreateMeterProviderBuilder()
            .ConfigureResource(r => r.AddService(serviceName))
            .AddMeter(sources ?? DefaultMeterSources)
            //.AddConsoleExporter()
            .AddOtlpExporter(options ?? DefaultExporterOptions)
            .Build();
    }

    public void Dispose()
    {
        _meterProvider.Dispose();
        _traceProvider.Dispose();
        GC.SuppressFinalize(this);
    }
}
