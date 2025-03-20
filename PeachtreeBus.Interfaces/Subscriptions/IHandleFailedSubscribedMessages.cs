using System;
using System.Threading.Tasks;

namespace PeachtreeBus.Subscriptions;

public interface IHandleFailedSubscribedMessages
{
    Task Handle(ISubscribedContext context, object message, Exception exception);
}
