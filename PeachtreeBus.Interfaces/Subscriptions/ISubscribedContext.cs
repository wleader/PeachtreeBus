namespace PeachtreeBus.Subscriptions;

public interface ISubscribedContext : IIncomingContext
{
    public SubscriberId SubscriberId { get; }
}
