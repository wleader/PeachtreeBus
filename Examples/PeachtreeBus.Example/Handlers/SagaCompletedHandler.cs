using Microsoft.Extensions.Logging;
using PeachtreeBus.Example.Data;
using PeachtreeBus.Example.Messages;
using PeachtreeBus.Example.Subsciptions;
using PeachtreeBus.Queues;
using PeachtreeBus.Subscriptions;
using System.Threading.Tasks;

namespace PeachtreeBus.Example.Handlers
{
    /// <summary>
    /// An example handler for the SampleSagaCompelteMessage
    /// </summary>
    public class SagaCompletedHandler(
        ILogger<SagaCompletedHandler> log,
        IExampleDataAccess dataAccess,
        ISubscribedPublisher publisher)
        : IHandleQueueMessage<SampleSagaComplete>
    {
        private readonly ILogger _log = log;
        private readonly IExampleDataAccess _dataAccess = dataAccess;
        private readonly ISubscribedPublisher _publisher = publisher;

        public async Task Handle(IQueueContext context, SampleSagaComplete message)
        {
            _log.DistributedSagaComplete();

            // send an announcement message to all that have subscribed.
            await _publisher.Publish(Topics.Announcements, new AnnounceSagaCompleted
            {
                AppId = message.AppId
            });
            await _dataAccess.Audit("Example Saga Completed.");
        }
    }
}
