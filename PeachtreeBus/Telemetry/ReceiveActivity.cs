using PeachtreeBus.Queues;
using PeachtreeBus.Subscriptions;
using System;
using System.Diagnostics;

namespace PeachtreeBus.Telemetry;

public class ReceiveActivity : BaseActivity, IDisposable
{
    public ReceiveActivity(QueueContext context, DateTime started)
    {
        _activity = ActivitySources.Messaging.StartActivity(
            $"receive {context.SourceQueue}",
            ActivityKind.Client,
            GetParent(context),
            startTime: started)
            ?.AddMessagingSystem()
            ?.AddMessagingOperation("receive")
            ?.AddMessagingClientId()
            ?.AddQueueContext(context);
    }

    public ReceiveActivity(SubscribedContext context, DateTime started)
    {
        _activity = ActivitySources.Messaging.StartActivity(
            $"receive {context.Topic}",
            ActivityKind.Client,
            GetParent(context),
            startTime: started)
            ?.AddMessagingSystem()
            ?.AddMessagingOperation("receive")
            ?.AddMessagingClientId()
            ?.AddSubscribedContext(context);
    }

    private static string? GetParent<T>(IncomingContext<T> context)
        where T : QueueData
    {
        return context.Headers.Diagnostics.StartNewTraceOnReceive
            ? null : context.Headers.Diagnostics.TraceParent;
    }
}
