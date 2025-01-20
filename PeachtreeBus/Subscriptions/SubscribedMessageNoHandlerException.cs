using System;

namespace PeachtreeBus.Subscriptions
{
    public class SubscribedMessageNoHandlerException : PeachtreeBusException
    {
        public Guid MessageId { get; private set; }
        public Type MessageType { get; private set; }
        public Guid SubscriberId { get; private set; }

        public SubscribedMessageNoHandlerException(Guid messageId, Guid subscriberId, Type messageType)
            : base($"Message {messageId} for subscriber {subscriberId} is a message class of {messageType} for which no handlers were found.")
        {
            MessageId = messageId;
            MessageType = messageType;
            SubscriberId = subscriberId;
        }
    }
}
