using PeachtreeBus.Queues;
using System;
using System.Diagnostics;

namespace PeachtreeBus.Telemetry;

public class SendActivity : BaseActivity, IDisposable
{
    public SendActivity(ISendContext context)
    {
        _activity =
        ActivitySources.Messaging.StartActivity(
            "send " + context.Destination.ToString(),
            ActivityKind.Producer)
            ?.AddOutgoingContext(context)
            ?.AddDestination(context.Destination);
    }
}
