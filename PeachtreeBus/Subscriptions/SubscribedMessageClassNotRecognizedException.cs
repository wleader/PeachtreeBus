using System;

namespace PeachtreeBus.Subscriptions
{
    public class SubscribedMessageClassNotRecognizedException : PeachtreeBusException
    {
        public Guid MessageId { get; }
        public string? TypeName { get; }
        public Guid SubscriberId { get; }

        public SubscribedMessageClassNotRecognizedException(Guid messageId, Guid subscriberId, string? typeName)
            : base($"Message {messageId} for subscriber {subscriberId} is a message class of {typeName} which was not a recognized type.")
        {
            MessageId = messageId;
            TypeName = typeName;
            SubscriberId = subscriberId;
        }
    }
}
