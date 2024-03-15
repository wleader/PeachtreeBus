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
        Task Publish(string category, Type type, object message, DateTime? notBefore = null);
    }

    /// <summary>
    /// Publishes a subscription message to all current subscribers.
    /// </summary>
    public class SubscribedPublisher : ISubscribedPublisher
    {
        private readonly IBusDataAccess _dataAccess;
        private readonly ISubscribedLifespan _subscribedLifespan;
        private readonly ISerializer _serializer;
        private readonly IPerfCounters _counters;
        private readonly ISystemClock _clock;

        public SubscribedPublisher(IBusDataAccess dataAccess,
            ISubscribedLifespan subscriptionConfiguration,
            ISerializer serializer,
            IPerfCounters counters,
            ISystemClock clock)
        {
            _dataAccess = dataAccess;
            _subscribedLifespan = subscriptionConfiguration;
            _serializer = serializer;
            _counters = counters;
            _clock = clock;
        }

        /// <summary>
        /// Publishes the message
        /// </summary>
        /// <param name="category"></param>
        /// <param name="type"></param>
        /// <param name="message"></param>
        /// <param name="notBefore"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="ArgumentException"></exception>
        public async Task Publish(string category, Type type, object message, DateTime? notBefore = null)
        {
            if (message == null) throw new ArgumentNullException(nameof(message), $"{nameof(message)} must not be null.");
            if (string.IsNullOrWhiteSpace(category)) throw new ArgumentException($"{nameof(category)} must not be null and not empty.");
            if (type == null) throw new ArgumentNullException(nameof(type), $"{nameof(type)} must not be null.");

            if (notBefore.HasValue && notBefore.Value.Kind == DateTimeKind.Unspecified)
                throw new ArgumentException($"{nameof(notBefore)} must not have an Unspecified DateTimeKind.", nameof(notBefore));

            if (!typeof(ISubscribedMessage).IsAssignableFrom(type))
                throw new MissingInterfaceException(type, typeof(ISubscribedMessage));

            // expire any out of data subscribers so we don't waste resources
            // sending to subscribers that are not renewing their subscriptions.

            await _dataAccess.ExpireSubscriptions();

            // get a list of all subscribers for the category
            var subscribers = await _dataAccess.GetSubscribers(category);

            // note the type in the headers so it can be deserialized.
            var headers = new Headers
            {
                MessageClass = type.FullName + ", " + type.Assembly.GetName().Name
            };

            var validUntil = notBefore.HasValue
                ? notBefore.Value.ToUniversalTime().Add(_subscribedLifespan.Duration)
                : _clock.UtcNow.Add(_subscribedLifespan.Duration);

            var nb = notBefore.HasValue ? notBefore.Value.ToUniversalTime() : _clock.UtcNow;
            var headerstring = _serializer.SerializeHeaders(headers);
            var bodystring = _serializer.SerializeMessage(message, type);

            foreach (var subscriber in subscribers)
            {

                // create the message entity, serializing the headers and body.
                var sm = new Model.SubscribedMessage
                {
                    ValidUntil = validUntil,
                    SubscriberId = subscriber,
                    MessageId = Guid.NewGuid(),
                    NotBefore = nb,
                    Enqueued = _clock.UtcNow,
                    Completed = null,
                    Failed = null,
                    Retries = 0,
                    Headers = headerstring,
                    Body = bodystring
                };

                await _dataAccess.AddMessage(sm);
                _counters.SentMessage();
            }
        }
    }

    public static class SubscriptionPublisherExtensions
    {
        /// <summary>
        /// Publishes the message
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="publisher"></param>
        /// <param name="category"></param>
        /// <param name="message"></param>
        /// <param name="notBefore"></param>
        /// <returns></returns>
        public static async Task PublishMessage<T>(this ISubscribedPublisher publisher,
           string category, T message, DateTime? notBefore = null)
            where T: notnull
        {
            await publisher.Publish(category, typeof(T), message, notBefore);
        }
    }
}
