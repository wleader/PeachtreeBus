using PeachtreeBus.Queues;
using System;
using System.Diagnostics;

namespace PeachtreeBus.Telemetry;

public class SendActivity(ISendContext context) : IDisposable
{
    private readonly Activity? _activity =
        ActivitySources.Messaging.StartActivity(
            "send " + context.Destination.ToString(),
            ActivityKind.Producer)
            ?.AddOutgoingContext(context)
            ?.AddDestination(context.Destination);

    public void Dispose()
    {
        _activity?.Dispose();
        GC.SuppressFinalize(this);
    }
}
