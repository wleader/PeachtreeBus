using PeachtreeBus.ClassNames;
using PeachtreeBus.Data;
using PeachtreeBus.Queues;

namespace PeachtreeBus.Exceptions;

public class QueueMessageClassNotRecognizedException(
    UniqueIdentity messageId,
    QueueName sourceQueue,
    ClassName className)
    : PeachtreeBusException($"Message {messageId} from queue {sourceQueue} is a message class of {className} which was not a recognized type.")
{
    public UniqueIdentity MessageId { get; } = messageId;
    public ClassName ClassName { get; } = className;
    public QueueName SourceQueue { get; } = sourceQueue;
}
