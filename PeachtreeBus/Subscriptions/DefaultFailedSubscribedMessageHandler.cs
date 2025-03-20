using System;
using System.Threading.Tasks;

namespace PeachtreeBus.Subscriptions
{
    public class DefaultFailedSubscribedMessageHandler : IHandleFailedSubscribedMessages
    {
        public Task Handle(ISubscribedContext context, object message, Exception exception)
        {
            return Task.CompletedTask;
        }
    }
}
