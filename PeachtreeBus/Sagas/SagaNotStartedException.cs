using PeachtreeBus.Data;
using PeachtreeBus.Queues;
using System;

namespace PeachtreeBus.Sagas
{
    public class SagaNotStartedException(
        UniqueIdentity messageId,
        QueueName sourceQueue,
        Type messageType,
        Type sagaType,
        SagaKey sagaKey)
        : PeachtreeBusException($"Message {messageId} from queue {sourceQueue} is a message class of {messageType} for which a saga of type {sagaType} with a saga key {sagaKey} has not been started.")
    {
        public UniqueIdentity MessageId { get; } = messageId;
        public Type MessageType { get; } = messageType;
        public Type SagaType { get; } = sagaType;
        public QueueName SourceQueue { get; } = sourceQueue;
        public SagaKey SagaKey { get; } = sagaKey;
    }
}
