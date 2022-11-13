using Microsoft.Extensions.Logging;
using PeachtreeBus.Data;
using PeachtreeBus.Interfaces;
using PeachtreeBus.Model;
using System;
using System.Threading.Tasks;

namespace PeachtreeBus.Queues
{
    /// <summary>
    /// Defines an interface for reading and completeing messages,
    /// and loading and saving saga data.
    /// </summary>
    public interface IQueueReader
    {
        /// <summary>
        /// The maximum number of times a message can have an exception before it is considered to be failed.
        /// </summary>
        byte MaxRetries { get; set; }

        /// <summary>
        /// Gets one message and deserializes it and its headers into a message context.
        /// </summary>
        /// <param name="queueId"></param>
        /// <returns></returns>
        Task<QueueContext> GetNext(string queueName);

        /// <summary>
        /// Marks a message as successfully processed.
        /// </summary>
        /// <param name="messageContext"></param>
        Task Complete(QueueContext context);

        /// <summary>
        /// Sets a message to be processed later.
        /// </summary>
        /// <param name="messageContext"></param>
        /// <param name="seconds"></param>
        Task DelayMessage(QueueContext context, int milliseconds);

        /// <summary>
        /// Increases the retry count on the message.
        /// Records the exception details.
        /// Marks the message failed if it exceeded the retry count.
        /// </summary>
        /// <param name="messageContext"></param>
        /// <param name="exception"></param>
        Task Fail(QueueContext context, Exception exception);

        /// <summary>
        /// Reads and deserialized saga data into the Message Context.
        /// and the saga itself.
        /// </summary>
        /// <param name="saga">The saga to load data for.</param>
        /// <param name="context">The mesage context. SagaKey must be set.</param>
        Task LoadSaga(object saga, QueueContext context);

        /// <summary>
        /// Stores the saga data in the database after a message is processed.
        /// If the saga is compelte, the data is deleted from the database.
        /// </summary>
        /// <param name="saga"></param>
        /// <param name="context"></param>
        Task SaveSaga(object saga, QueueContext context);
    }

    internal static class QueueReader_LogMessages
    {
        internal static readonly Action<ILogger, string, Guid, Exception> QueueReader_HeaderNotDeserilizable_Action =
            LoggerMessage.Define<string, Guid>(
                LogLevel.Warning,
                Events.QueueReader_HeaderNotDeserializable,
                "Headers could not be deserialized for message {MessageId} from queue {QueueName}");

        internal static void QueueReader_HeaderNotDeserializable(this ILogger logger, string queueName, Guid messageId, Exception ex)
        {
            QueueReader_HeaderNotDeserilizable_Action(logger, queueName, messageId, ex);
        }

        internal static readonly Action<ILogger, string, Guid, Exception> QueueReader_BodyNotDeserializable_Action =
            LoggerMessage.Define<string, Guid>(
                LogLevel.Warning,
                Events.QueueReader_BodyNotDeserializable,
                "Body could not be deserialized for message {MessageId} from queue {QueueName}");

        internal static void QueueReader_BodyNotDeserizeralizable(this ILogger logger, string queueName, Guid messageId, Exception ex)
        {
            QueueReader_BodyNotDeserializable_Action(logger, queueName, messageId, ex);
        }

        internal static readonly Action<ILogger, string, Guid, string, Exception> QueueReader_MessageClassNotRecognized_Action =
            LoggerMessage.Define<string, Guid, string>(
                LogLevel.Warning,
                Events.QueueReader_MessageClassNotRecognized,
                "Message class '{MessageClass}' was not recognized for message {MessageId} from queue {QueueName}");

        internal static void QueueReader_MessageClassNotRecognized(this ILogger logger, string queueName, Guid messageId, string messageClass)
        {
            QueueReader_MessageClassNotRecognized_Action(logger, queueName, messageId, messageClass, null);
        }

        internal static readonly Action<ILogger, string, Guid, int, Exception> QueueReader_MessageExceededMaxRetries_Action =
            LoggerMessage.Define<string, Guid, int>(
                LogLevel.Warning,
                Events.QueueReader_MessageExceededMaxRetries,
                "Message {MessageId} from queue {QueueName} exceeded the maximum number of allowed retries ({MaxRetries}) and has failed.");

        internal static void QueueReader_MessageExceededMaxRetries(this ILogger logger, string queueName, Guid messageId, int maxRetries)
        {
            QueueReader_MessageExceededMaxRetries_Action(logger, queueName, messageId, maxRetries, null);
        }

        internal static readonly Action<ILogger, string, Guid, DateTime, Exception> QueueReader_MessageWillBeRetried_Action =
            LoggerMessage.Define<string, Guid, DateTime>(
                LogLevel.Warning,
                Events.QueueReader_MessageWillBeRetried,
                "Message {MessageId} from queue {QueueName} will be retried after {NotBefore}.");

        internal static void QueueReader_MessageWillBeRetried(this ILogger logger, string queueName, Guid messageId, DateTime notBefore)
        {
            QueueReader_MessageWillBeRetried_Action(logger, queueName, messageId, notBefore, null);
        }

        internal static readonly Action<ILogger, string, string, Exception> QueueReader_LoadingSagaData_Action =
            LoggerMessage.Define<string, string>(
                LogLevel.Information,
                Events.QueueReader_LoadingSagaData,
                "Loading saga data for {SagaName} - {SagaKey}");

        internal static void QueueReader_LoadingSagaData(this ILogger logger, string sagaName, string sagaKey)
        {
            QueueReader_LoadingSagaData_Action(logger, sagaName, sagaKey, null);
        }
    }

    /// <summary>
    /// Implements IQueueReader Using an IBusDataAccess and JSON serialization.
    /// </summary>
    public class QueueReader :IQueueReader
    {
        private readonly IBusDataAccess _dataAccess;
        private readonly ILogger<QueueReader> _log;
        private readonly IPerfCounters _counters;
        private readonly ISerializer _serializer;
        private readonly ISystemClock _clock;

        public byte MaxRetries { get; set; }

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="dataAccess">The Data access.</param>
        /// <param name="log"></param>
        public QueueReader(IBusDataAccess dataAccess,
            ILogger<QueueReader> log,
            IPerfCounters counters,
            ISerializer serializer,
            ISystemClock clock)
        {
            _log = log;
            _dataAccess = dataAccess;
            _counters = counters;
            _serializer = serializer;
            MaxRetries = 5;
            _clock = clock;
        }

        /// <inheritdoc/>
        public async Task<QueueContext> GetNext(string queueName)
        {
            // get a message.
            // if it retuned null there is no message to pocess currently.
            var queueMessage = await _dataAccess.GetPendingQueued(queueName);
            if (queueMessage == null) return null;


            // deserialize the headers.
            Headers headers;
            try
            {
                headers =  _serializer.DeserializeHeaders(queueMessage.Headers);
            }
            catch (Exception ex)
            {
                _log.QueueReader_HeaderNotDeserializable(queueName, queueMessage.MessageId, ex);
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
                    message = _serializer.DeserializeMessage(queueMessage.Body, messageType);
                }
                catch (Exception ex)
                {
                    _log.QueueReader_BodyNotDeserizeralizable(queueName, queueMessage.MessageId, ex);
                }
            }
            else
            {
                _log.QueueReader_MessageClassNotRecognized(queueName, queueMessage.MessageId, headers.MessageClass);
            }

            // return the new message context.
            return new QueueContext
            {
                MessageData = queueMessage,
                Headers = headers,
                Message = message,
                SourceQueue = queueName
            };
        }

        /// <inheritdoc/>
        public async Task Complete(QueueContext messageContext)
        {
            messageContext.MessageData.Completed = _clock.UtcNow;
            _counters.CompleteMessage();
            await _dataAccess.CompleteMessage(messageContext.MessageData, messageContext.SourceQueue);
        }

        /// <inheritdoc/>
        public async Task Fail(QueueContext context, Exception exception)
        {
            context.MessageData.Retries++;
            context.MessageData.NotBefore = _clock.UtcNow.AddSeconds(5 * context.MessageData.Retries); // Wait longer between retries.
            context.Headers.ExceptionDetails = exception.ToString();
            context.MessageData.Headers = _serializer.SerializeHeaders(context.Headers);
            if (context.MessageData.Retries >= MaxRetries)
            {
                _log.QueueReader_MessageExceededMaxRetries(context.SourceQueue, context.MessageData.MessageId, MaxRetries);
                context.MessageData.Failed = DateTime.UtcNow;
                _counters.FailMessage();
                await _dataAccess.FailMessage(context.MessageData, context.SourceQueue);
            }
            else
            {
                _log.QueueReader_MessageWillBeRetried(context.SourceQueue, context.MessageData.MessageId, context.MessageData.NotBefore);
                _counters.RetryMessage();
                await _dataAccess.Update(context.MessageData, context.SourceQueue);
            }
        }

        /// <inheritdoc/>
        public async Task LoadSaga(object saga, QueueContext context)
        {
            // work out the class name of the saga.
            var sagaType = saga.GetType();
            var nameProperty = sagaType.GetProperty("SagaName");
            var sagaName = (string)nameProperty.GetValue(saga);

            // fetch the data from the DB.
            _log.QueueReader_LoadingSagaData(sagaName, context.SagaKey);
            context.SagaData = await _dataAccess.GetSagaData(sagaName, context.SagaKey);
            if (context.SagaData != null && context.SagaData.Blocked) return;

            // Hypothetically locked could be false, and sagadata could be null if the saga hasn't been started.
            // and if two saga starts are processed at the same time, a second insert will occur and 
            // that will fail with a duplicate key constraint.

            // determine the type to deserialze to or create.
            var dataProperty = sagaType.GetProperty("Data");
            var sagaDataType = dataProperty.PropertyType;

            object dataObject;
            if (context.SagaData == null)
            {
                // no data in the DB, create a new object.
                dataObject = Activator.CreateInstance(sagaDataType);
            }
            else 
            {
                // deserialize
                // try catch needed? Probably better to throw and let the error handling deal with it.
                // Someone may have to fix the saga data and retry the failed message though.
                dataObject = _serializer.DeserializeSaga(context.SagaData.Data, sagaDataType);
            }

            // assign the data to the saga.
            dataProperty.SetValue(saga, dataObject);
        }


        /// <inheritdoc/>
        public async Task SaveSaga(object saga, QueueContext context)
        {
            var sagaType = saga.GetType();
            var nameProperty = sagaType.GetProperty("SagaName");
            var sagaName = (string)nameProperty.GetValue(saga);

            // if the saga is complete, we can delete the data.
            var completeProperty = sagaType.GetProperty("SagaComplete");
            bool IsComplete = completeProperty.GetValue(saga) is bool completeValue && completeValue;
            if (IsComplete)
            {
                await _dataAccess.DeleteSagaData(sagaName, context.SagaKey);
            }

            // the saga is not complete, serialize it.
            var dataProperty = sagaType.GetProperty("Data");
            var dataObject = dataProperty.GetValue(saga);
            dataObject ??= Activator.CreateInstance(dataProperty.PropertyType);
            var serializedData = _serializer.SerializeSaga(dataObject, dataProperty.PropertyType);

            if (context.SagaData == null)
            {
                // we have never persisted saga data for this instance (Message was a saga start message).
                // create a new row in the DB.
                context.SagaData = new SagaData
                {
                    SagaId = Guid.NewGuid(),
                    Key = context.SagaKey,
                    Data = serializedData
                };

                // if two start messages are processed at the same time, two inserts could occur on different threads.
                // if that happens, the second insert is expected to throw a duplicate key constraint violation.
                await _dataAccess.Insert(context.SagaData, sagaName);
            }
            else
            {
                // update the existing row.
                context.SagaData.Data = serializedData;
                await _dataAccess.Update(context.SagaData, sagaName);
            }
        }

        /// <inheritdoc/>
        public async Task DelayMessage(QueueContext messageContext, int milliseconds)
        {
            messageContext.MessageData.NotBefore = _clock.UtcNow.AddMilliseconds(milliseconds);
            _counters.DelayMessage();
            await _dataAccess.Update(messageContext.MessageData, messageContext.SourceQueue);
        }
    }
}
