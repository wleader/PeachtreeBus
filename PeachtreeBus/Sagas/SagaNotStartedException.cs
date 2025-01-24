using PeachtreeBus.Queues;
using System;

namespace PeachtreeBus.Sagas
{
    public class SagaNotStartedException(
        Guid messageId,
        QueueName sourceQueue,
        Type messageType,
        Type sagaType,
        string sagaKey)
        : PeachtreeBusException($"Message {messageId} from queue {sourceQueue} is a message class of {messageType} for which a saga of type {sagaType} with a saga key {sagaKey} has not been started.")
    {
        public Guid MessageId { get; } = messageId;
        public Type MessageType { get; } = messageType;
        public Type SagaType { get; } = sagaType;
        public QueueName SourceQueue { get; } = sourceQueue;
        public string SagaKey { get; } = sagaKey;
    }
}
