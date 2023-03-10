using PeachtreeBus.Subscriptions;
using System;
using System.Threading.Tasks;

namespace PeachtreeBus.Errors
{
    public interface IHandleFailedSubscribedMessages
    {
        Task Handle(SubscribedContext context, object message, Exception exception);
    }
}
