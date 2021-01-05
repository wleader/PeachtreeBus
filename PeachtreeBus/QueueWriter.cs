using PeachtreeBus.Data;
using System;
using System.Text.Json;

namespace PeachtreeBus
{
    /// <summary>
    /// Defines an interface for adding a message to a queue.
    /// </summary>
    public interface IQueueWriter
    {
        void WriteMessage<T>(int queueId, T message, DateTime? NotBefore = null);
        void WriteMessage(int queueId, Type type, object message, DateTime? NotBefore = null);
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

        public void WriteMessage<T>(int queueId, T message, DateTime? NotBefore = null)
        {
            WriteMessage(queueId, typeof(T), message, NotBefore);
        }

        public void WriteMessage(int queueId, Type type, object message, DateTime? NotBefore = null)
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
                QueueId = queueId,
                Headers = JsonSerializer.Serialize<Headers>(headers),
                Body = JsonSerializer.Serialize(message, type)
            };

            // store the message in the queue.
            _dataAccess.Insert(qm);
        }

    }
}
