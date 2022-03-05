using PeachtreeBus.Example.Data;
using PeachtreeBus.Example.Messages;
using PeachtreeBus.Queues;
using PeachtreeBus.Subscriptions;
using System.Threading.Tasks;

namespace PeachtreeBus.Example.Handlers
{
    /// <summary>
    /// An example handler for the SampleSagaCompelteMessage
    /// </summary>
    public class SagaCompletedHandler : IHandleQueueMessage<SampleSagaComplete>
    {
        private readonly ILog _log;
        private readonly IExampleDataAccess _dataAccess;
        private readonly ISubscribedPublisher _publisher;

        public SagaCompletedHandler(ILog<SagaCompletedHandler> log,
            IExampleDataAccess dataAccess,
            ISubscribedPublisher publisher)
        {
            _log = log;
            _dataAccess = dataAccess;
            _publisher = publisher;
        }

        public async Task Handle(QueueContext context, SampleSagaComplete message)
        {
            _log.Info("Distributed Saga Complete!");

            // send an announcement message to all that have subscribed.
            await _publisher.PublishMessage("Announcements", new AnnounceSagaCompleted
            {
                AppId = message.AppId
            });
            await _dataAccess.Audit("Example Saga Completed.");
        }
    }
}
