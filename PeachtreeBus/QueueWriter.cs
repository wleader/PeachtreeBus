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
        Task WriteMessage(string queueName, Type type, object message, DateTime? NotBefore = null);
    }

    /// <summary>
    ///  Adds a message to a queue using an IBusDataAccess.
    /// </summary>
    public class QueueWriter : IQueueWriter
    {
        private readonly IBusDataAccess _dataAccess;
        private readonly IPerfCounters _counters;
        private readonly ISerializer _serializer;

        public QueueWriter(IBusDataAccess dataAccess,
            IPerfCounters counters,
            ISerializer serializer)
        {
            _dataAccess = dataAccess;
            _counters = counters;
            _serializer = serializer;
        }

        public Task WriteMessage(string queueName, Type type, object message, DateTime? NotBefore = null)
        {
            if (message == null) throw new ArgumentNullException(nameof(message), $"{nameof(message)} must not be null.");
            if (string.IsNullOrEmpty(queueName)) throw new ArgumentException($"{nameof(queueName)} must not be null and not empty.");

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
                Headers = _serializer.SerializeHeaders(headers),
                Body = _serializer.SerializeMessage(message, type)
            };

            _counters.SentMessage();

            // store the message in the queue.
            return _dataAccess.EnqueueMessage(qm, queueName);
        }

    }

    public static class QueueWriterExtensions
    {
        public static Task WriteMessage<T>(this IQueueWriter writer, string queueName, T message, DateTime? NotBefore = null)
        {
            return writer.WriteMessage(queueName, typeof(T), message, NotBefore);
        }
    }

}
