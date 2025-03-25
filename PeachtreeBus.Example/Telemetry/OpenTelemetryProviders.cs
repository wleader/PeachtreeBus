using OpenTelemetry;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using System;

namespace PeachtreeBus.Example.Telemetry;

public class OpenTelemetryProviders : IDisposable
{
    private readonly TracerProvider _traceProvider;
    private readonly MeterProvider _meterProvider;

    private static readonly string[] PeachtreeBusSources =
        [
            "PeachtreeBus",
            "PeachTreeBus.*"
        ];

    public OpenTelemetryProviders(string serviceName)
    {
        // setup a tracer
        _traceProvider = Sdk.CreateTracerProviderBuilder()
            .ConfigureResource(r => r.AddService(serviceName))
            .AddSource(PeachtreeBusSources)
            .AddConsoleExporter()
            .AddOtlpExporter()
            .Build();

        _meterProvider = Sdk.CreateMeterProviderBuilder()
            .ConfigureResource(r => r.AddService(serviceName))
            .AddMeter(PeachtreeBusSources)
            .AddConsoleExporter()
            .AddOtlpExporter()
            .Build();
    }
    public void Dispose()
    {
        _meterProvider.Dispose();
        _traceProvider.Dispose();
        GC.SuppressFinalize(this);
    }
}
