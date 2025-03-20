using PeachtreeBus.Sagas;

namespace PeachtreeBus.Queues;

public interface IQueueContext : IIncomingContext
{
    QueueName SourceQueue { get; }
    SagaKey SagaKey { get; }
}
