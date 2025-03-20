using PeachtreeBus.Data;
using PeachtreeBus.Subscriptions;

namespace PeachtreeBus.Exceptions;

public class SubscribedMessageClassNotRecognizedException(
    UniqueIdentity messageId,
    SubscriberId subscriberId,
    string? typeName)
    : PeachtreeBusException($"Message {messageId} for subscriber {subscriberId} is a message class of {typeName} which was not a recognized type.")
{
    public UniqueIdentity MessageId { get; } = messageId;
    public string? TypeName { get; } = typeName;
    public SubscriberId SubscriberId { get; } = subscriberId;
}
