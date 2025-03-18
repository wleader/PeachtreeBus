using PeachtreeBus.Queues;
using System;
using System.Threading.Tasks;

namespace PeachtreeBus.Errors;

public class DefaultFailedQueueMessageHandler : IHandleFailedQueueMessages
{
    public Task Handle(IQueueContext context, object message, Exception exception)
    {
        return Task.CompletedTask;
    }
}
