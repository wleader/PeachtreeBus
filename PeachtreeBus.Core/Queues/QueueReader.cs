﻿using Microsoft.Extensions.Logging;
using PeachtreeBus.ClassNames;
using PeachtreeBus.Data;
using PeachtreeBus.Exceptions;
using PeachtreeBus.Sagas;
using PeachtreeBus.Serialization;
using PeachtreeBus.Telemetry;
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
        /// Gets one message and deserializes it and its headers into a message context.
        /// </summary>
        /// <param name="queueId"></param>
        /// <returns></returns>
        Task<QueueContext?> GetNext(QueueName queueName);

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

    /// <summary>
    /// Implements IQueueReader Using an IBusDataAccess and JSON serialization.
    /// </summary>
    /// <remarks>
    /// Constructor.
    /// </remarks>
    /// <param name="dataAccess">The Data access.</param>
    /// <param name="log"></param>
    public class QueueReader(IBusDataAccess dataAccess,
        ILogger<QueueReader> log,
        IMeters meters,
        ISerializer serializer,
        ISystemClock clock,
        IQueueFailures failures,
        IQueueRetryStrategy retryStrategy,
        IClassNameService classNameService) : IQueueReader
    {
        private readonly IBusDataAccess _dataAccess = dataAccess;
        private readonly ILogger<QueueReader> _log = log;
        private readonly IMeters _meters = meters;
        private readonly ISerializer _serializer = serializer;
        private readonly ISystemClock _clock = clock;
        private readonly IQueueFailures _failures = failures;
        private readonly IQueueRetryStrategy _retryStrategy = retryStrategy;
        private readonly IClassNameService _classNameService = classNameService;

        /// <inheritdoc/>
        public async Task<QueueContext?> GetNext(QueueName queueName)
        {
            // get a message.
            // if it returned null there is no message to pocess currently.
            var queueMessage = await _dataAccess.GetPendingQueued(queueName);
            if (queueMessage == null) return null;

            var result = new QueueContext()
            {
                Data = queueMessage,
                SourceQueue = queueName,
                Message = null!
            };

            if (result.Headers is null)
            {
                _log.HeaderNotDeserializable(queueMessage.MessageId, queueName);
                // this might not work, The body might deserialize but there won't be an
                // IHandleMessages<System.Object> so it won't get handled. This really just gives
                // us a chance to get farther and log more about the bad message.
                // this message would proably have to be removed from the database by hand?
                result.Data.Headers = new() { MessageClass = ClassName.Default };
            }

            // Deserialize the message.
            var messageType = _classNameService.GetTypeForClassName(result.MessageClass);
            if (messageType is null)
            {
                _log.MessageClassNotRecognized(result.MessageClass, queueMessage.MessageId, queueName);
                return result;
            }

            try
            {
                result.Message = _serializer.Deserialize(queueMessage.Body, messageType);
            }
            catch (Exception ex)
            {
                _log.BodyNotDeserializable(queueMessage.MessageId, queueName, ex);
            }

            return result;
        }

        /// <inheritdoc/>
        public async Task Complete(QueueContext messageContext)
        {
            messageContext.Data.Completed = _clock.UtcNow;
            _meters.CompleteMessage();
            await _dataAccess.CompleteMessage(messageContext.Data, messageContext.SourceQueue);
        }

        /// <inheritdoc/>
        public async Task Fail(QueueContext context, Exception exception)
        {
            context.Data.Retries++;
            context.Headers.ExceptionDetails = exception.ToString();
            context.Data.Headers = context.Headers;

            var retryResult = _retryStrategy.DetermineRetry(context, exception, context.Data.Retries);

            if (retryResult.ShouldRetry)
            {
                context.Data.NotBefore = _clock.UtcNow.Add(retryResult.Delay);
                _log.MessageWillBeRetried(context.Data.MessageId, context.SourceQueue, context.Data.NotBefore);
                _meters.RetryMessage();
                await _dataAccess.UpdateMessage(context.Data, context.SourceQueue);
            }
            else
            {
                _log.MessageFailed(context.Data.MessageId, context.SourceQueue);
                context.Data.Failed = _clock.UtcNow;
                _meters.FailMessage();
                await _dataAccess.FailMessage(context.Data, context.SourceQueue);
                await _failures.Failed(context, context.Message, exception);
            }
        }

        /// <inheritdoc/>
        public async Task LoadSaga(object saga, QueueContext context)
        {
            // work out the class name of the saga.
            var sagaType = saga.GetType();
            var nameProperty = sagaType.GetProperty("SagaName", typeof(SagaName));
            nameProperty = UnreachableException.ThrowIfNull(nameProperty,
                message: "Saga<> must have a SagaName property of type SagaName.");

            var sagaNameObject = nameProperty.GetValue(saga);
            sagaNameObject = UnreachableException.ThrowIfNull(sagaNameObject,
                message: "The SagaName Type must not be a nullable type.");

            var sagaName = (SagaName)sagaNameObject;
            IncorrectImplementationException.ThrowIfNull(sagaName.Value,
                sagaType,
                typeof(Saga<>),
                message: "Saga<>.SagaName must not return an uninitialized SagaName.");

            // fetch the data from the DB.
            _log.LoadingSagaData(sagaName, context.SagaKey);
            context.SagaData = await _dataAccess.GetSagaData(sagaName, context.SagaKey);

            if (context.SagaBlocked) return;

            // Hypothetically locked could be false, and sagadata could be null if the saga hasn't been started.
            // and if two saga starts are processed at the same time, a second insert will occur and 
            // that will fail with a duplicate key constraint.

            // determine the type to deserialze to or create.
            var dataProperty = sagaType.GetProperty("Data");
            dataProperty = UnreachableException.ThrowIfNull(dataProperty,
                message: "Saga<> must have a Data Property.");

            var sagaDataType = dataProperty.PropertyType;

            object? dataObject;
            if (context.SagaData == null)
            {
                // no data in the DB, create a new object.
                dataObject = Activator.CreateInstance(sagaDataType);
                dataObject = UnreachableException.ThrowIfNull(dataObject,
                    message: "Saga<TSagaData> type parameter must be a reference type (where TSagaData : class, new()).");
            }
            else
            {
                // deserialize
                // try catch needed? Probably better to throw and let the error handling deal with it.
                // Someone may have to fix the saga data and retry the failed message though.
                dataObject = _serializer.Deserialize(context.SagaData.Data, sagaDataType);
            }

            // assign the data to the saga.
            dataProperty.SetValue(saga, dataObject);
        }


        /// <inheritdoc/>
        public async Task SaveSaga(object saga, QueueContext context)
        {
            var sagaType = saga.GetType();
            var nameProperty = sagaType.GetProperty("SagaName", typeof(SagaName));
            nameProperty = UnreachableException.ThrowIfNull(nameProperty,
                message: "Saga<> must have a SagaName property of type SagaName.");

            var sagaNameObject = nameProperty.GetValue(saga);
            sagaNameObject = UnreachableException.ThrowIfNull(sagaNameObject,
                message: "The SagaName type must not be a nullable type.");

            var sagaName = (SagaName)sagaNameObject;
            IncorrectImplementationException.ThrowIfNull(sagaName.Value,
                sagaType,
                typeof(Saga<>),
                message: "Saga<>.SagaName must not return an uninitialized SagaName.");

            // if the saga is complete, we can delete the data.
            var completeProperty = sagaType.GetProperty("SagaComplete", typeof(bool));
            completeProperty = UnreachableException.ThrowIfNull(completeProperty,
                message: "Saga<> must have a SagaComplete property that is of type bool.");

            bool IsComplete = completeProperty.GetValue(saga) is bool completeValue && completeValue;
            if (IsComplete)
            {
                _log.DeletingSagaData(sagaName, context.SagaKey);
                await _dataAccess.DeleteSagaData(sagaName, context.SagaKey);
                return;
            }

            _log.SavingSagaData(sagaName, context.SagaKey);

            // the saga is not complete, serialize it.
            var dataProperty = sagaType.GetProperty("Data");
            dataProperty = UnreachableException.ThrowIfNull(dataProperty,
                message: "Saga<> implmentations must have a Data property.");

            var dataObject = dataProperty.GetValue(saga);
            dataObject ??= Activator.CreateInstance(dataProperty.PropertyType);
            dataObject = UnreachableException.ThrowIfNull(dataObject,
                message: "Saga<TSagaData> type parameter must be a reference type (where TSagaData : class, new()).");

            var serializedData = _serializer.Serialize(dataObject, dataProperty.PropertyType);

            if (context.SagaData == null)
            {
                // we have never persisted saga data for this instance (Message was a saga start message).
                // create a new row in the DB.
                context.SagaData = new SagaData
                {
                    SagaId = UniqueIdentity.New(),
                    Key = context.SagaKey,
                    Data = serializedData,
                    MetaData = new(_clock.UtcNow, _clock.UtcNow),
                    Blocked = false,
                };

                // if two start messages are processed at the same time, two inserts could occur on different threads.
                // if that happens, the second insert is expected to throw a duplicate key constraint violation.
                await _dataAccess.InsertSagaData(context.SagaData, sagaName);
            }
            else
            {
                // update the existing row.
                context.SagaData.Data = serializedData;
                context.SagaData.MetaData = context.SagaData.MetaData with { LastMessageTime = _clock.UtcNow };
                await _dataAccess.UpdateSagaData(context.SagaData, sagaName);
            }
        }

        /// <inheritdoc/>
        public async Task DelayMessage(QueueContext messageContext, int milliseconds)
        {
            messageContext.Data.NotBefore = _clock.UtcNow.AddMilliseconds(milliseconds);
            await _dataAccess.UpdateMessage(messageContext.Data, messageContext.SourceQueue);
        }
    }
}
