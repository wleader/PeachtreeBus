using System;

namespace PeachtreeBus.Queues
{
    public class QueueMessageClassNotRecognizedException : PeachtreeBusException
    {
        public Guid MessageId { get; private set; }
        public string TypeName { get; private set; }
        public QueueName SourceQueue { get; private set; }

        internal QueueMessageClassNotRecognizedException(Guid messageId, QueueName sourceQueue, string typeName)
            : base($"Message {messageId} from queue {sourceQueue} is a message class of {typeName} which was not a recognized type.")
        {
            MessageId = messageId;
            TypeName = typeName;
            SourceQueue = sourceQueue;
        }
    }
}
