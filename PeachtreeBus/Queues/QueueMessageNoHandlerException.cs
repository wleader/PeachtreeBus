using System;

namespace PeachtreeBus.Queues
{
    public class QueueMessageNoHandlerException(Guid messageId, QueueName sourceQueue, Type messageType) : PeachtreeBusException($"Message {messageId} from queue {sourceQueue} is a message class of {messageType} for which no handlers were found.")
    {
        public Guid MessageId { get; } = messageId;
        public Type MessageType { get; } = messageType;
        public QueueName SourceQueue { get; } = sourceQueue;
    }
}
