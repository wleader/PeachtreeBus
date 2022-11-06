using PeachtreeBus.Data;
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
        private readonly ILog<SubscribedReader> _log;
        private readonly IPerfCounters _counters;
        private readonly ISystemClock _clock;

        public byte MaxRetries { get; set; }

        public SubscribedReader(IBusDataAccess dataAccess,
            ISerializer serializer,
            ILog<SubscribedReader> log,
            IPerfCounters counters,
            ISystemClock clock)
        {
            _dataAccess = dataAccess;
            _serializer = serializer;
            _log = log;
            _counters = counters;
            _clock = clock;
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
                _log.Error($"Message {context.MessageData.MessageId} exceeded max retries ({MaxRetries}) and has failed.");
                context.MessageData.Failed = _clock.UtcNow;
                _counters.FailMessage();
                await _dataAccess.FailMessage(context.MessageData);
            }
            else
            {
                _log.Error($"Message {context.MessageData.MessageId} will be retried at {context.MessageData.NotBefore}.");
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
            catch
            {
                _log.Warn($"Headers could not be deserialized for message {subscriptionMessage.MessageId}.");
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
                catch
                {
                    _log.Warn($"Message Body could not be deserialized for message {subscriptionMessage.MessageId}.");
                }
            }
            else
            {
                _log.Warn($"Message class {headers.MessageClass} was not recognized for message {subscriptionMessage.MessageId}.");
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
