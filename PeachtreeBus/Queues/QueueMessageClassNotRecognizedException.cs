using PeachtreeBus.Data;
using PeachtreeBus.Exceptions;

namespace PeachtreeBus.Queues
{
    public class QueueMessageClassNotRecognizedException(UniqueIdentity messageId, QueueName sourceQueue, string typeName) : PeachtreeBusException($"Message {messageId} from queue {sourceQueue} is a message class of {typeName} which was not a recognized type.")
    {
        public UniqueIdentity MessageId { get; } = messageId;
        public string TypeName { get; } = typeName;
        public QueueName SourceQueue { get; } = sourceQueue;
    }
}
