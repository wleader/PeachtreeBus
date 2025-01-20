using PeachtreeBus.Queues;
using System;

namespace PeachtreeBus.Sagas
{
    public class SagaNotStartedException : PeachtreeBusException
    {
        public Guid MessageId { get; }
        public Type MessageType { get; }
        public Type SagaType { get; }
        public QueueName SourceQueue { get; }
        public string SagaKey { get; }

        internal SagaNotStartedException(Guid messageId, QueueName sourceQueue, Type messageType, Type sagaType, string sagaKey)
            : base($"Message {messageId} from queue {sourceQueue} is a message class of {messageType} for which a saga of type {sagaType} with a saga key {sagaKey} has not been started.")
        {
            MessageId = messageId;
            MessageType = messageType;
            SourceQueue = sourceQueue;
            SagaType = sagaType;
            SagaKey = sagaKey;
        }
    }
}
