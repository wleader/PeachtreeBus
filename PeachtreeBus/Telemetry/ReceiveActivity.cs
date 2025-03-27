using PeachtreeBus.Queues;
using System;
using System.Diagnostics;

namespace PeachtreeBus.Telemetry;

public class ReceiveActivity(QueueContext context, DateTime started) : IDisposable
{
    private readonly Activity? _activity =
        ActivitySources.Messaging.StartActivity(
            $"receive {context.SourceQueue}",
            ActivityKind.Client,
            null, // parent context
            startTime: started)
            ?.AddMessagingSystem()
            ?.AddMessagingOperation("receive")
            ?.AddMessagingClientId()
            ?.AddQueueContext(context);

    public void Dispose()
    {
        _activity?.Dispose();
        GC.SuppressFinalize(this);
    }
}
