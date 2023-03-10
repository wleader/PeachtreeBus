using PeachtreeBus.Subscriptions;
using System;
using System.Threading.Tasks;

namespace PeachtreeBus.Errors
{
    public class DefaultFailedSubscribedMessageHandler : IHandleFailedSubscribedMessages
    {
        public Task Handle(SubscribedContext context, object message, Exception exception)
        {
            return Task.CompletedTask;
        }
    }
}
