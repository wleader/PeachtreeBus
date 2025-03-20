using PeachtreeBus.Queues;
using System;

namespace PeachtreeBus.Sagas;

public interface ISagaMessageMap
{
    void Add<TMessage>(Func<TMessage, SagaKey> MapFunction) where TMessage : IQueueMessage;
};
