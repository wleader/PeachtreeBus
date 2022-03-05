using PeachtreeBus.Example.Messages;
using PeachtreeBus.Subscriptions;
using System.Threading.Tasks;

namespace PeachtreeBus.Example.Subsciptions
{
    /// <summary>
    /// An example of handling a subscribed message.
    /// </summary>
    public class AnnounceSagaCompletedSubscriber : IHandleSubscribedMessage<AnnounceSagaCompleted>
    {
        private readonly ILog<AnnounceSagaCompletedSubscriber> _log;

        public AnnounceSagaCompletedSubscriber(
            ILog<AnnounceSagaCompletedSubscriber> log)
        {
            _log = log;
        }

        public Task Handle(SubscribedContext context, AnnounceSagaCompleted message)
        {
            // nothing fancy, just note that we handled the subscribed message.
            _log.Info($"Subscriber {context.SubscriberId} got Saga complete announcement {message.AppId}");
            return Task.CompletedTask;
        }
    }
}
