using System;

namespace PeachtreeBus.Subscriptions
{
    public class SubscribedMessageClassNotRecognizedException(
        Guid messageId,
        Guid subscriberId,
        string? typeName)
        : PeachtreeBusException($"Message {messageId} for subscriber {subscriberId} is a message class of {typeName} which was not a recognized type.")
    {
        public Guid MessageId { get; } = messageId;
        public string? TypeName { get; } = typeName;
        public Guid SubscriberId { get; } = subscriberId;
    }
}
