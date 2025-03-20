using System;
using System.Threading.Tasks;

namespace PeachtreeBus.Queues;

public interface IHandleFailedQueueMessages
{
    Task Handle(IQueueContext context, object message, Exception exception);
}
