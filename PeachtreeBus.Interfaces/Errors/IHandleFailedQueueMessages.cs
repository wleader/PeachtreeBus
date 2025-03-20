using PeachtreeBus.Queues;
using System;
using System.Threading.Tasks;

namespace PeachtreeBus.Errors;

public interface IHandleFailedQueueMessages
{
    Task Handle(IQueueContext context, object message, Exception exception);
}
