using PeachtreeBus.Data;
using System;
using System.Text.Json;
using System.Threading.Tasks;

namespace PeachtreeBus
{
    /// <summary>
    /// Defines an interface for adding a message to a queue.
    /// </summary>
    public interface IQueueWriter
    {
        Task WriteMessage<T>(string queueName, T message, DateTime? NotBefore = null);
        Task WriteMessage(string queueName, Type type, object message, DateTime? NotBefore = null);
    }

    /// <summary>
    ///  Adds a message to a queue using an IBusDataAccess.
    /// </summary>
    public class QueueWriter : IQueueWriter
    {
        private readonly IBusDataAccess _dataAccess;

        public QueueWriter(IBusDataAccess dataAccess)
        {
            _dataAccess = dataAccess;
        }

        public Task WriteMessage<T>(string queueName, T message, DateTime? NotBefore = null)
        {
            return WriteMessage(queueName, typeof(T), message, NotBefore);
        }

        public Task WriteMessage(string queueName, Type type, object message, DateTime? NotBefore = null)
        { 
            // note the type in the headers so it can be deserialized.
            var headers = new Headers
            {
                MessageClass = type.FullName + ", " + type.Assembly.GetName().Name
            };

            // create the message entity, serializing the headers and body.
            var qm = new Model.QueueMessage
            {
                MessageId = Guid.NewGuid(),
                NotBefore = NotBefore.HasValue ? NotBefore.Value.ToUniversalTime() : DateTime.UtcNow,
                Enqueued = DateTime.UtcNow,
                Completed = null,
                Failed = null,
                Retries = 0,
                Headers = JsonSerializer.Serialize<Headers>(headers),
                Body = JsonSerializer.Serialize(message, type)
            };

            Counters.PeachtreeBusCounters.SentMessage();

            // store the message in the queue.
            return _dataAccess.EnqueueMessage(qm, queueName);
        }

    }
}
