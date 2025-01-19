using System;

namespace PeachtreeBus.Subscriptions
{
    public class SubscribedMessageClassNotRecognizedException : PeachtreeBusException
    {
        public Guid MessageId { get; private set; }
        public string? TypeName { get; private set; }
        public Guid SubscriberId { get; private set; }

        internal SubscribedMessageClassNotRecognizedException(Guid messageId, Guid subscriberId, string? typeName)
            : base($"Message {messageId} for subscriber {subscriberId} is a message class of {typeName} which was not a recognized type.")
        {
            MessageId = messageId;
            TypeName = typeName;
            SubscriberId = subscriberId;
        }
    }
}
