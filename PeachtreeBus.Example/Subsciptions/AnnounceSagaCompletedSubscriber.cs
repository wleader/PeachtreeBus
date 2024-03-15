using Microsoft.Extensions.Logging;
using PeachtreeBus.Example.Messages;
using PeachtreeBus.Subscriptions;
using System.Threading.Tasks;

namespace PeachtreeBus.Example.Subsciptions
{
    /// <summary>
    /// An example of handling a subscribed message.
    /// </summary>
    public class AnnounceSagaCompletedSubscriber(
        ILogger<AnnounceSagaCompletedSubscriber> log)
        : IHandleSubscribedMessage<AnnounceSagaCompleted>
    {
        private readonly ILogger<AnnounceSagaCompletedSubscriber> _log = log;

        public Task Handle(SubscribedContext context, AnnounceSagaCompleted message)
        {
            // nothing fancy, just note that we handled the subscribed message.
            _log.SubscribedSagaComplete(context.SubscriberId, message.AppId);
            return Task.CompletedTask;
        }
    }
}
