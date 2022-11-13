using Microsoft.Extensions.Logging;
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

    internal static class SubscribedReader_LogMessages
    {
        internal static Action<ILogger, Guid, Guid, int, Exception> SubscribedReader_MessageExceededMaxRetries_Action =
            LoggerMessage.Define<Guid, Guid, int>(
                LogLevel.Error,
                Events.SubscribedReader_MessageExceededMaxRetries,
                "Message {MessageId} for Subscriber {SubscriberId} execeed the max number of retries ({MaxRetries}) and has failed.");

        internal static void SubscribedReader_MessageExceededMaxRetries(this ILogger logger,
            Guid messageId, Guid subscriberId, int maxRetries)
        {
            SubscribedReader_MessageExceededMaxRetries_Action(logger, messageId, subscriberId, maxRetries, null);
        }

        internal static Action<ILogger, Guid, Guid, Exception> SubscribedReader_HeaderNotDeserializable_Action =
            LoggerMessage.Define<Guid, Guid>(
                LogLevel.Warning,
                Events.SubscribedReader_HeaderNotDeserializable,
                "Headers could not be deserialized for message {MessageId} for subscriber {SubscriberId}");

        internal static void SubscribedReader_HeaderNotDeserializable(this ILogger logger,
            Guid messageId, Guid subscriberId)
        {
            SubscribedReader_HeaderNotDeserializable_Action(logger, messageId, subscriberId, null);
        }

        internal static Action<ILogger, Guid, Guid, Exception> SubscribedReader_BodyNotDeserializable_Action =
            LoggerMessage.Define<Guid, Guid>(
                LogLevel.Warning,
                Events.SubscribedReader_BodyNotDeserializable,
                "Message Body could not be deserialized for message {MessageId} for subscriber {SubscriberId}.");

        internal static void SubscribedReader_BodyNotDeserializable(this ILogger logger,
            Guid messageId, Guid subscriberId, Exception ex)
        {
            SubscribedReader_HeaderNotDeserializable_Action(logger, messageId, subscriberId, ex);
        }

        internal static readonly Action<ILogger, string, Guid, Guid, Exception> SubscribedReader_MessageClassNotRecognized_Action =
            LoggerMessage.Define<string, Guid, Guid>(
                LogLevel.Warning,
                Events.SubscribedReader_MessageClassNotRecognized,
                "Message class '{MessageClass}' was not recognized for message {MessageId} for subscriber {SubscriberId}");

        internal static void SubscribedReader_MessageClassNotRecognized(this ILogger logger, Guid messageId, Guid subscriberId, string messageClass)
        {
            SubscribedReader_MessageClassNotRecognized_Action(logger, messageClass, messageId, subscriberId, null);
        }

        internal static readonly Action<ILogger, Guid, Guid, DateTime, Exception> SubscribedReader_MessageWillBeRetried_Action =
            LoggerMessage.Define<Guid, Guid, DateTime>(
                LogLevel.Warning,
                Events.SubscribedReader_MessageWillBeRetried,
                "Message {MessageId} from queue {QueueName} will be retried after {NotBefore}.");

        internal static void SubscribedReader_MessageWillBeRetried(this ILogger logger, Guid messageId, Guid subscriberId, DateTime notBefore)
        {
            SubscribedReader_MessageWillBeRetried_Action(logger, messageId, subscriberId, notBefore, null);
        }
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

        public byte MaxRetries { get; set; }

        public SubscribedReader(IBusDataAccess dataAccess,
            ISerializer serializer,
            ILogger<SubscribedReader> log,
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
                _log.SubscribedReader_MessageExceededMaxRetries(context.MessageData.MessageId, context.SubscriberId, MaxRetries);
                context.MessageData.Failed = _clock.UtcNow;
                _counters.FailMessage();
                await _dataAccess.FailMessage(context.MessageData);
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
            catch
            {
                _log.SubscribedReader_HeaderNotDeserializable(subscriptionMessage.MessageId, subscriberId);
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
                _log.SubscribedReader_MessageClassNotRecognized(subscriptionMessage.MessageId, subscriberId, headers.MessageClass);
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
