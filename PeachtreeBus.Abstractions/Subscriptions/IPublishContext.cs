namespace PeachtreeBus.Subscriptions;

public interface IPublishContext : IOutgoingContext
{
    public Topic Topic { get; }
    public long? RecipientCount { get; set; }
}
