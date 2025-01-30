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
        /// <param name="queueName">Which queue to write to.</param>
        /// <param name="type">The Type of the message.</param>
        /// <param name="message">an instance of an IQueueMessage.</param>
        /// <param name="notBefore">Indicates when this message can be processed. Will default to 'Now' if not provided.</param>
        /// <param name="priority">A priority for the message. Higher priorities are processed first.</param>
        /// <param name="userHeaders">Any user headers to be included with the message.</param>
        /// <returns></returns>
        Task WriteMessage(
            QueueName queueName,
            Type type,
            object message,
            DateTime? notBefore = null,
            int priority = 0,
            UserHeaders? userHeaders = null);
    }

    /// <summary>
    ///  Adds a message to a queue using an IBusDataAccess.
    /// </summary>
    public class QueueWriter(IBusDataAccess dataAccess,
        IPerfCounters counters,
        ISerializer serializer,
        ISystemClock clock) : IQueueWriter
    {
        private readonly IBusDataAccess _dataAccess = dataAccess;
        private readonly IPerfCounters _counters = counters;
        private readonly ISerializer _serializer = serializer;
        private readonly ISystemClock _clock = clock;

        public async Task WriteMessage(
            QueueName queueName,
            Type type,
            object message,
            DateTime? notBefore = null,
            int priority = 0,
            UserHeaders? userHeaders = null)
        {
            if (message == null) throw new ArgumentNullException(nameof(message), $"{nameof(message)} must not be null.");
            if (type == null) throw new ArgumentNullException(nameof(type), $"{nameof(type)} must not be null.");

            TypeIsNotIQueueMessageException.ThrowIfMissingInterface(type);

            // note the type in the headers so it can be deserialized.
            var headers = new Headers(type, userHeaders);

            // create the message entity, serializing the headers and body.
            var qm = new QueueMessage
            {
                MessageId = UniqueIdentity.New(),
                Priority = priority,
                NotBefore = notBefore ?? _clock.UtcNow,
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
        public static async Task WriteMessage<T>(
            this IQueueWriter writer,
            QueueName queueName,
            T message,
            DateTime? NotBefore = null,
            int priority = 0,
            UserHeaders? userHeaders = null)
            where T : notnull
        {
            await writer.WriteMessage(queueName, typeof(T), message, NotBefore, priority, userHeaders);
        }
    }

}
