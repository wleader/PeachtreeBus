using PeachtreeBus.ClassNames;
using PeachtreeBus.Data;
using PeachtreeBus.Subscriptions;

namespace PeachtreeBus.Exceptions;

public class SubscribedMessageClassNotRecognizedException(
    UniqueIdentity messageId,
    SubscriberId subscriberId,
    ClassName className)
    : PeachtreeBusException($"Message {messageId} for subscriber {subscriberId} is a message class of {className} which was not a recognized type.")
{
    public UniqueIdentity MessageId { get; } = messageId;
    public ClassName ClassName { get; } = className;
    public SubscriberId SubscriberId { get; } = subscriberId;
}
