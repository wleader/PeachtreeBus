﻿using Microsoft.Extensions.Logging;
using PeachtreeBus.Data;
using PeachtreeBus.Errors;
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
        Task<SubscribedContext?> GetNext(SubscriberId subscriberId);
        Task Complete(SubscribedContext subsriptionContext);
        Task Fail(SubscribedContext subsriptionContext, Exception ex);
    }

    /// <summary>
    /// Reads and updates, completes, and fails subscribed messages.
    /// </summary>
    public class SubscribedReader(
        IBusDataAccess dataAccess,
        ISerializer serializer,
        ILogger<SubscribedReader> log,
        IPerfCounters counters,
        ISystemClock clock,
        ISubscribedFailures failures,
        ISubscribedRetryStrategy retryStrategy)
        : ISubscribedReader
    {
        private readonly IBusDataAccess _dataAccess = dataAccess;
        private readonly ISerializer _serializer = serializer;
        private readonly ILogger<SubscribedReader> _log = log;
        private readonly IPerfCounters _counters = counters;
        private readonly ISystemClock _clock = clock;
        private readonly ISubscribedFailures _failures = failures;
        private readonly ISubscribedRetryStrategy _retryStrategy = retryStrategy;

        /// <summary>
        /// Completes a subscribed message
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        public async Task Complete(SubscribedContext context)
        {
            context.Data.Completed = _clock.UtcNow;
            _counters.CompleteMessage();
            await _dataAccess.CompleteMessage(context.Data);
        }

        /// <summary>
        /// Failes a subscribed message
        /// </summary>
        /// <param name="context"></param>
        /// <param name="exception"></param>
        /// <returns></returns>
        public async Task Fail(SubscribedContext context, Exception exception)
        {
            context.Data.Retries++;
            context.Headers.ExceptionDetails = exception.ToString();
            context.Data.Headers = _serializer.SerializeHeaders(context.Headers);

            var retryResult = _retryStrategy.DetermineRetry(context, exception, context.Data.Retries);

            if (retryResult.ShouldRetry)
            {
                context.Data.NotBefore = _clock.UtcNow.Add(retryResult.Delay);
                _log.SubscribedReader_MessageWillBeRetried(context.Data.MessageId, context.SubscriberId, context.Data.NotBefore);
                _counters.RetryMessage();
                await _dataAccess.UpdateMessage(context.Data);
            }
            else
            {
                _log.SubscribedReader_MessageFailed(context.Data.MessageId, context.SubscriberId);
                context.Data.Failed = _clock.UtcNow;
                _counters.FailMessage();
                await _dataAccess.FailMessage(context.Data);
                await _failures.Failed(context, context.Message, exception);
            }
        }

        /// <summary>
        /// Reads an eligible subscribed message.
        /// </summary>
        /// <param name="subscriberId"></param>
        /// <returns></returns>
        public async Task<SubscribedContext?> GetNext(SubscriberId subscriberId)
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
            object message = null!;
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
            return new()
            {
                Data = subscriptionMessage,
                Headers = headers,
                Message = message,
            };
        }
    }
}
