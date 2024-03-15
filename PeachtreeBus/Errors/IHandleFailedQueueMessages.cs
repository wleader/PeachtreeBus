using PeachtreeBus.Queues;
using System;
using System.Threading.Tasks;

namespace PeachtreeBus.Errors
{
    public interface IHandleFailedQueueMessages
    {
        Task Handle(QueueContext context, object message, Exception exception);
    }
}
