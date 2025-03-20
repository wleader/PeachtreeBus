using System;
using System.Threading.Tasks;

namespace PeachtreeBus.Queues;

public class DefaultFailedQueueMessageHandler : IHandleFailedQueueMessages
{
    public Task Handle(IQueueContext context, object message, Exception exception)
    {
        return Task.CompletedTask;
    }
}
