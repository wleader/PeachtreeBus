using System;

namespace PeachtreeBus.Subscriptions
{
    public class SubscribedMessageNoHandlerException(
        Guid messageId,
        Guid subscriberId,
        Type messageType)
        : PeachtreeBusException($"Message {messageId} for subscriber {subscriberId} is a message class of {messageType} for which no handlers were found.")
    {
        public Guid MessageId { get; private set; } = messageId;
        public Type MessageType { get; private set; } = messageType;
        public Guid SubscriberId { get; private set; } = subscriberId;
    }
}
