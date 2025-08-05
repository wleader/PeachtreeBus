using PeachtreeBus.Queues;
using PeachtreeBus.Subscriptions;
using System;
using System.Threading;

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

public class BusContextAccessor : IBusContextAccessor
{
    private static readonly AsyncLocal<IQueueContext?> _currentQueueContext = new();
    private static readonly AsyncLocal<ISubscribedContext?> _currentSubscribedContext = new();
    private static readonly AsyncLocal<ISendContext?> _currentSendContext = new();
    private static readonly AsyncLocal<IPublishContext?> _currentPublishContext = new();

    public static void Set<T>(T? value) where T : IContext
    {
        if (typeof(T).IsAssignableTo(typeof(IQueueContext)))
        {
            _currentQueueContext.Value = (IQueueContext?)value;
            _currentPublishContext.Value = null;
        }
        else if (typeof(T).IsAssignableTo(typeof(ISubscribedContext)))
        {
            _currentSubscribedContext.Value = (ISubscribedContext?)value;
            _currentQueueContext.Value = null;
        }
        else if (typeof(T).IsAssignableTo(typeof(ISendContext)))
        {
            _currentSendContext.Value = (ISendContext?)value;
            _currentPublishContext.Value = null;
        }
        else if (typeof(T).IsAssignableTo(typeof(IPublishContext)))
        {
            _currentPublishContext.Value = (IPublishContext?)value;
            _currentSendContext.Value = null;
        }
        else
        {
            throw new ArgumentException(
                $"The type {typeof(T)} is not supported by The ContextAccessor.",
                nameof(value));
        }
    }

    public IQueueContext? QueueContext => _currentQueueContext.Value;
    public ISubscribedContext? SubscribedContext => _currentSubscribedContext.Value;
    public ISendContext? SendContext => _currentSendContext.Value;
    public IPublishContext? PublishContext => _currentPublishContext.Value;

    public IIncomingContext? IncomingContext => (IIncomingContext?)QueueContext ?? SubscribedContext;
    public IOutgoingContext? OutgoingContext => (IOutgoingContext?)SendContext ?? PublishContext;
}
