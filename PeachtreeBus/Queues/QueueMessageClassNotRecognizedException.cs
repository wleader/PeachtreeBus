using System;

namespace PeachtreeBus.Queues
{
    public class QueueMessageClassNotRecognizedException(Guid messageId, QueueName sourceQueue, string typeName) : PeachtreeBusException($"Message {messageId} from queue {sourceQueue} is a message class of {typeName} which was not a recognized type.")
    {
        public Guid MessageId { get; } = messageId;
        public string TypeName { get; } = typeName;
        public QueueName SourceQueue { get; } = sourceQueue;
    }
}
