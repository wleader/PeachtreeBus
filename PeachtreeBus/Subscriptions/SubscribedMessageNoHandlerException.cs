using PeachtreeBus.Data;
using PeachtreeBus.Exceptions;
using System;

namespace PeachtreeBus.Subscriptions
{
    public class SubscribedMessageNoHandlerException(
        UniqueIdentity messageId,
        SubscriberId subscriberId,
        Type messageType)
        : PeachtreeBusException($"Message {messageId} for subscriber {subscriberId} is a message class of {messageType} for which no handlers were found.")
    {
        public UniqueIdentity MessageId { get; } = messageId;
        public Type MessageType { get; } = messageType;
        public SubscriberId SubscriberId { get; } = subscriberId;
    }
}
