using PeachtreeBus.Queues;
using PeachtreeBus.Subscriptions;

namespace PeachtreeBus;

public interface IBusContextAccessor
{
    IIncomingContext? IncomingContext { get; }
    IOutgoingContext? OutgoingContext { get; }
    IQueueContext? QueueContext { get; }
    ISubscribedContext? SubscribedContext { get; }
    ISendContext? SendContext { get; }
    IPublishContext? PublishContext { get; }
}
