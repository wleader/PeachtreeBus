﻿using PeachtreeBus.Data;
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
        Task Publish(Category category, Type type, object message, DateTime? notBefore = null, int priority = 0);
    }

    /// <summary>
    /// Publishes a subscription message to all current subscribers.
    /// </summary>
    public class SubscribedPublisher(
        IBusDataAccess dataAccess,
        ISubscribedLifespan subscriptionConfiguration,
        ISerializer serializer,
        IPerfCounters counters,
        ISystemClock clock)
        : ISubscribedPublisher
    {
        private readonly IBusDataAccess _dataAccess = dataAccess;
        private readonly ISubscribedLifespan _subscribedLifespan = subscriptionConfiguration;
        private readonly ISerializer _serializer = serializer;
        private readonly IPerfCounters _counters = counters;
        private readonly ISystemClock _clock = clock;

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
        public async Task Publish(Category category, Type type, object message, DateTime? notBefore = null, int priority = 0)
        {
            if (message == null) throw new ArgumentNullException(nameof(message), $"{nameof(message)} must not be null.");
            if (type == null) throw new ArgumentNullException(nameof(type), $"{nameof(type)} must not be null.");

            if (notBefore.HasValue && notBefore.Value.Kind == DateTimeKind.Unspecified)
                throw new ArgumentException($"{nameof(notBefore)} must not have an Unspecified DateTimeKind.", nameof(notBefore));

            TypeIsNotISubscribedMessageException.ThrowIfMissingInterface(type);

            // get a list of all subscribers for the category
            var subscribers = await _dataAccess.GetSubscribers(category);

            // note the type in the headers so it can be deserialized.
            var headers = new Headers(type);

            var validUntil = notBefore.HasValue
                ? notBefore.Value.ToUniversalTime().Add(_subscribedLifespan.Duration)
                : _clock.UtcNow.Add(_subscribedLifespan.Duration);

            var nb = notBefore.HasValue ? notBefore.Value.ToUniversalTime() : _clock.UtcNow;
            var headerstring = _serializer.SerializeHeaders(headers);
            var bodystring = _serializer.SerializeMessage(message, type);

            foreach (var subscriber in subscribers)
            {

                // create the message entity, serializing the headers and body.
                var sm = new SubscribedMessage
                {
                    ValidUntil = validUntil,
                    SubscriberId = subscriber,
                    MessageId = UniqueIdentity.New(),
                    Priority = priority,
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
           Category category, T message, DateTime? notBefore = null, int priority = 0)
            where T : notnull
        {
            await publisher.Publish(category, typeof(T), message, notBefore, priority);
        }
    }
}
