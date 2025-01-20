using System;

namespace PeachtreeBus.Queues
{
    public class QueueMessageClassNotRecognizedException : PeachtreeBusException
    {
        public Guid MessageId { get; }
        public string TypeName { get; }
        public QueueName SourceQueue { get; }

        internal QueueMessageClassNotRecognizedException(Guid messageId, QueueName sourceQueue, string typeName)
            : base($"Message {messageId} from queue {sourceQueue} is a message class of {typeName} which was not a recognized type.")
        {
            MessageId = messageId;
            TypeName = typeName;
            SourceQueue = sourceQueue;
        }
    }
}
