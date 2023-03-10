using Microsoft.Extensions.Logging;
using PeachtreeBus.Data;
using PeachtreeBus.Errors;
using PeachtreeBus.Interfaces;
using System;
using System.Threading.Tasks;

namespace PeachtreeBus.Subscriptions
{

    /// <summary>
    /// Defines an interface for reading and updating subscription messages.
    /// </summary>
    public interface ISubscribedReader
    {
        /// <summary>
        /// Gets one message and deserializes it and its headers into a message context.
        /// </summary>
        /// <param name="queueId"></param>
        /// <returns></returns>
        Task<SubscribedContext> GetNext(Guid subscriberId);
        Task Complete(SubscribedContext subsriptionContext);
        Task Fail(SubscribedContext subsriptionContext, Exception ex);
    }

    /// <summary>
    /// Reads and updates, completes, and fails subscribed messages.
    /// </summary>
    public class SubscribedReader : ISubscribedReader
    {
        private readonly IBusDataAccess _dataAccess;
        private readonly ISerializer _serializer;
        private readonly ILogger<SubscribedReader> _log;
        private readonly IPerfCounters _counters;
        private readonly ISystemClock _clock;
        private readonly ISubscribedFailures _failures;

        public byte MaxRetries { get; set; }

        public SubscribedReader(IBusDataAccess dataAccess,
            ISerializer serializer,
            ILogger<SubscribedReader> log,
            IPerfCounters counters,
            ISystemClock clock,
            ISubscribedFailures failures)
        {
            _dataAccess = dataAccess;
            _serializer = serializer;
            _log = log;
            _counters = counters;
            _clock = clock;
            _failures = failures;
            MaxRetries = 5;
        }

        /// <summary>
        /// Completes a subscribed message
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        public async Task Complete(SubscribedContext context)
        {
            context.MessageData.Completed = _clock.UtcNow;
            _counters.CompleteMessage();
            await _dataAccess.CompleteMessage(context.MessageData);
        }

        /// <summary>
        /// Failes a subscribed message
        /// </summary>
        /// <param name="context"></param>
        /// <param name="exception"></param>
        /// <returns></returns>
        public async Task Fail(SubscribedContext context, Exception exception)
        {
            context.MessageData.Retries++;
            context.MessageData.NotBefore = _clock.UtcNow.AddSeconds(5 * context.MessageData.Retries); // Wait longer between retries.
            context.Headers.ExceptionDetails = exception.ToString();
            context.MessageData.Headers = _serializer.SerializeHeaders(context.Headers);
            if (context.MessageData.Retries >= MaxRetries)
            {
                _log.SubscribedReader_MessageExceededMaxRetries(context.MessageData.MessageId, context.SubscriberId, MaxRetries);
                context.MessageData.Failed = _clock.UtcNow;
                _counters.FailMessage();
                await _dataAccess.FailMessage(context.MessageData);
                await _failures.Failed(context, context.Message, exception);
            }
            else
            {
                _log.SubscribedReader_MessageWillBeRetried(context.MessageData.MessageId, context.SubscriberId, context.MessageData.NotBefore);
                _counters.RetryMessage();
                await _dataAccess.Update(context.MessageData);
            }
        }

        /// <summary>
        /// Reads an eligible subscribed message.
        /// </summary>
        /// <param name="subscriberId"></param>
        /// <returns></returns>
        public async Task<SubscribedContext> GetNext(Guid subscriberId)
        {
            // get a message.
            // if it retuned null there is no message to pocess currently.
            var subscriptionMessage = await _dataAccess.GetPendingSubscribed(subscriberId);
            if (subscriptionMessage == null) return null;

            // deserialize the headers.
            Headers headers;
            try
            {
                headers = _serializer.DeserializeHeaders(subscriptionMessage.Headers);
            }
            catch (Exception ex)
            {
                _log.SubscribedReader_HeaderNotDeserializable(subscriptionMessage.MessageId, subscriberId, ex);
                // this might not work, The body might deserialize but there won't be an
                // IHandleMessages<System.Object> so it won't get handled. This really just gives
                // us a chance to get farther and log more about the bad message.
                // this message woulld proably have to be removed from the database by hand?
                headers = new Headers { MessageClass = "System.Object" };
            }

            // Deserialize the message.
            var messageType = Type.GetType(headers.MessageClass);
            object message = null;
            if (messageType != null)
            {
                try
                {
                    message = _serializer.DeserializeMessage(subscriptionMessage.Body, messageType);
                }
                catch (Exception ex)
                {
                    _log.SubscribedReader_BodyNotDeserializable(subscriptionMessage.MessageId, subscriberId, ex);
                }
            }
            else
            {
                _log.SubscribedReader_MessageClassNotRecognized(headers.MessageClass, subscriptionMessage.MessageId, subscriberId);
            }

            // return the new message context.
            return new SubscribedContext
            {
                MessageData = subscriptionMessage,
                Headers = headers,
                Message = message,
                SubscriberId = subscriberId
            };
        }
    }
}
