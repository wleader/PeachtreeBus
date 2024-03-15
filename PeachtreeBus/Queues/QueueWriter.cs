using PeachtreeBus.Data;
using PeachtreeBus.Interfaces;
using System;
using System.Threading.Tasks;

namespace PeachtreeBus.Queues
{

    /// <summary>
    /// Defines an interface for adding a message to a queue.
    /// </summary>
    public interface IQueueWriter
    {
        /// <summary>
        /// Writes a message to a queue
        /// </summary>
        /// <param name="queueName"></param>
        /// <param name="type"></param>
        /// <param name="message"></param>
        /// <param name="notBefore"></param>
        /// <returns></returns>
        Task WriteMessage(string queueName, Type type, object message, DateTime? notBefore = null);
    }

    /// <summary>
    ///  Adds a message to a queue using an IBusDataAccess.
    /// </summary>
    public class QueueWriter : IQueueWriter
    {
        private readonly IBusDataAccess _dataAccess;
        private readonly IPerfCounters _counters;
        private readonly ISerializer _serializer;
        private readonly ISystemClock _clock;

        public QueueWriter(IBusDataAccess dataAccess,
            IPerfCounters counters,
            ISerializer serializer,
            ISystemClock clock)
        {
            _dataAccess = dataAccess;
            _counters = counters;
            _serializer = serializer;
            _clock = clock;
        }

        public async Task WriteMessage(string queueName, Type type, object message, DateTime? notBefore = null)
        {
            if (message == null) throw new ArgumentNullException(nameof(message), $"{nameof(message)} must not be null.");
            if (type == null) throw new ArgumentNullException(nameof(type), $"{nameof(type)} must not be null.");
            if (string.IsNullOrWhiteSpace(queueName)) throw new ArgumentNullException($"{nameof(queueName)} must not be null and not empty.");

            if (notBefore.HasValue && notBefore.Value.Kind == DateTimeKind.Unspecified)
                throw new ArgumentException($"{nameof(notBefore)} must not have an Unspecified DateTimeKind.", nameof(notBefore));

            if (!typeof(IQueueMessage).IsAssignableFrom(type))
                throw new MissingInterfaceException(type, typeof(IQueueMessage));

            // note the type in the headers so it can be deserialized.
            var headers = new Headers
            {
                MessageClass = type.FullName + ", " + type.Assembly.GetName().Name
            };

            // create the message entity, serializing the headers and body.
            var qm = new Model.QueueMessage
            {
                MessageId = Guid.NewGuid(),
                NotBefore = notBefore.HasValue ? notBefore.Value.ToUniversalTime() : _clock.UtcNow,
                Enqueued = _clock.UtcNow,
                Completed = null,
                Failed = null,
                Retries = 0,
                Headers = _serializer.SerializeHeaders(headers),
                Body = _serializer.SerializeMessage(message, type)
            };

            _counters.SentMessage();

            // store the message in the queue.
            await _dataAccess.AddMessage(qm, queueName);
        }

    }

    public static class QueueWriterExtensions
    {
        /// <summary>
        /// Writes a message to a queue
        /// </summary>
        public static async Task WriteMessage<T>(this IQueueWriter writer, string queueName, T message, DateTime? NotBefore = null)
            where T : notnull
        {
            await writer.WriteMessage(queueName, typeof(T), message, NotBefore);
        }
    }

}
