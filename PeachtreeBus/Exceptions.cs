using System;

namespace PeachtreeBus
{
    public abstract class PeachtreeBusException : Exception
    {
        internal PeachtreeBusException(string message)
            : base(message)
        { }

        internal PeachtreeBusException(string message, Exception innerException)
            : base(message, innerException)
        { }

    }

    public class QueueMessageClassNotRecognizedException : PeachtreeBusException
    {
        public Guid MessageId { get; private set; }
        public string TypeName { get; private set; }
        public string SourceQueue { get; private set; }

        internal QueueMessageClassNotRecognizedException(Guid messageId, string sourceQueue, string typeName)
            : base($"Message {messageId} from queue {sourceQueue} is a message class of {typeName} which was not a recognized type.")
        {
            MessageId = messageId;
            TypeName = typeName;
            SourceQueue = sourceQueue;
        }
    }

    public class QueueMessageNoHandlerException : PeachtreeBusException
    {
        public Guid MessageId { get; private set; }
        public Type MessageType { get; private set; }
        public string SourceQueue { get; private set; }

        internal QueueMessageNoHandlerException(Guid messageId, string sourceQueue, Type messageType)
            : base($"Message {messageId} from queue {sourceQueue} is a message class of {messageType} for which no handlers were found.")
        {
            MessageId = messageId;
            MessageType = messageType;
            SourceQueue = sourceQueue;
        }
    }

    public class SagaNotStartedException : PeachtreeBusException
    {
        public Guid MessageId { get; private set; }
        public Type MessageType { get; private set; }
        public Type SagaType { get; private set; }
        public string SourceQueue { get; private set; }
        public string SagaKey { get; private set; }

        internal SagaNotStartedException(Guid messageId, string sourceQueue, Type messageType, Type sagaType, string sagaKey)
            : base($"Message {messageId} from queue {sourceQueue} is a message class of {messageType} for which a saga of type {sagaType} with a saga key {sagaKey} has not been started.")
        {
            MessageId = messageId;
            MessageType = messageType;
            SourceQueue = sourceQueue;
            SagaType = sagaType;
            SagaKey = sagaKey;
        }
    }

    public class SubscribedMessageClassNotRecognizedException : PeachtreeBusException
    {
        public Guid MessageId { get; private set; }
        public string TypeName { get; private set; }
        public Guid SubscriberId { get; private set; }

        internal SubscribedMessageClassNotRecognizedException(Guid messageId, Guid subscriberId, string typeName)
            : base($"Message {messageId} for subscriber {subscriberId} is a message class of {typeName} which was not a recognized type.")
        {
            MessageId = messageId;
            TypeName = typeName;
            SubscriberId = subscriberId;
        }
    }

    public class SubscribedMessageNoHandlerException : PeachtreeBusException
    {
        public Guid MessageId { get; private set; }
        public Type MessageType { get; private set; }
        public Guid SubscriberId { get; private set; }

        internal SubscribedMessageNoHandlerException(Guid messageId, Guid subscriberId, Type messageType)
            : base($"Message {messageId} for subscriber {subscriberId} is a message class of {messageType} for which no handlers were found.")
        {
            MessageId = messageId;
            MessageType = messageType;
            SubscriberId = subscriberId;
        }
    }
}
