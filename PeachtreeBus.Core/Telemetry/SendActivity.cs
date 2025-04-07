using PeachtreeBus.Queues;
using PeachtreeBus.Subscriptions;
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
            ?.AddMessagingSystem()
            ?.AddMessagingOperation("send")
            ?.AddMessagingClientId()
            ?.AddOutgoingContext(context)
            ?.AddDestination(context.Destination);
    }

    public SendActivity(IPublishContext context)
    {
        _activity =
        ActivitySources.Messaging.StartActivity(
            "send " + context.Topic.ToString(),
            ActivityKind.Producer)
            ?.AddMessagingSystem()
            ?.AddMessagingOperation("send")
            ?.AddMessagingClientId()
            ?.AddOutgoingContext(context)
            ?.AddDestination(context.Topic);
    }
}
