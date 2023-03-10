using System;
using System.Threading.Tasks;
using PeachtreeBus.Queues;

namespace PeachtreeBus.Errors
{
    public interface IHandleFailedQueueMessages
    {
        Task Handle(QueueContext context, object message, Exception exception);
    }
}
