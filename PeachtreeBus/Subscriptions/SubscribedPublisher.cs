using PeachtreeBus.Data;
using PeachtreeBus.Interfaces;
using System;
using System.Threading.Tasks;

namespace PeachtreeBus.Subscriptions
{
    /// <summary>
    /// Defines an interface for publishing a message to all registered subscribers
    /// </summary>
    public interface ISubscribedPublisher
    {
        Task<long> Publish(
            Topic topic,
            Type type,
            object message,
            UtcDateTime? notBefore = null,
            int priority = 0,
            UserHeaders? userHeaders = null);
    }

    /// <summary>
    /// Publishes a subscription message to all current subscribers.
    /// </summary>
    public class SubscribedPublisher(
        IBusDataAccess dataAccess,
        IBusConfiguration configuration,
        ISerializer serializer,
        IPerfCounters counters,
        ISystemClock clock)
        : ISubscribedPublisher
    {
        private readonly IBusDataAccess _dataAccess = dataAccess;
        private readonly IBusConfiguration _configuation = configuration;
        private readonly ISerializer _serializer = serializer;
        private readonly IPerfCounters _counters = counters;
        private readonly ISystemClock _clock = clock;

        /// <summary>
        /// Publishes the message
        /// </summary>
        /// <param name="topic"></param>
        /// <param name="type"></param>
        /// <param name="message"></param>
        /// <param name="notBefore"></param>
        /// <param name="priority"></param>
        /// <param name="userHeaders"></param>
        /// <returns>The number of subscribers that the message was published to.</returns>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="ArgumentException"></exception>
        public async Task<long> Publish(
            Topic topic,
            Type type,
            object message,
            UtcDateTime? notBefore = null,
            int priority = 0,
            UserHeaders? userHeaders = null)
        {
            if (message == null) throw new ArgumentNullException(nameof(message), $"{nameof(message)} must not be null.");
            if (type == null) throw new ArgumentNullException(nameof(type), $"{nameof(type)} must not be null.");
            TypeIsNotISubscribedMessageException.ThrowIfMissingInterface(type);

            // note the type in the headers so it can be deserialized.
            var headers = new Headers(type, userHeaders);

            // create the message entity, serializing the headers and body.
            var sm = new SubscribedMessage
            {
                ValidUntil = _clock.UtcNow.Add(_configuation.PublishConfiguration.Lifespan),
                MessageId = UniqueIdentity.Empty, // will be ignored and the database will generate.
                Priority = priority,
                NotBefore = notBefore ?? _clock.UtcNow,
                Enqueued = _clock.UtcNow,
                Completed = null,
                Failed = null,
                Retries = 0,
                Headers = _serializer.SerializeHeaders(headers),
                Body = _serializer.SerializeMessage(message, type)
            };

            var count = await _dataAccess.Publish(sm, topic);
            _counters.PublishMessage(count);
            return count;
        }
    }

    public static class SubscriptionPublisherExtensions
    {
        /// <summary>
        /// Publishes the message
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="publisher"></param>
        /// <param name="topic"></param>
        /// <param name="message"></param>
        /// <param name="notBefore"></param>
        /// <returns></returns>
        public static Task<long> PublishMessage<T>(
            this ISubscribedPublisher publisher,
            Topic topic,
            T message,
            DateTime? notBefore = null,
            int priority = 0,
            UserHeaders? userHeaders = null)
            where T : notnull
        {
            return publisher.Publish(topic, typeof(T), message, notBefore, priority, userHeaders);
        }
    }
}
