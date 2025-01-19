using System;

namespace PeachtreeBus.Queues
{
    public class QueueMessageNoHandlerException : PeachtreeBusException
    {
        public Guid MessageId { get; private set; }
        public Type MessageType { get; private set; }
        public QueueName SourceQueue { get; private set; }

        internal QueueMessageNoHandlerException(Guid messageId, QueueName sourceQueue, Type messageType)
            : base($"Message {messageId} from queue {sourceQueue} is a message class of {messageType} for which no handlers were found.")
        {
            MessageId = messageId;
            MessageType = messageType;
            SourceQueue = sourceQueue;
        }
    }
}
