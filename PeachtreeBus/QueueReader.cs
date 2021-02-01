﻿using PeachtreeBus.Data;
using PeachtreeBus.Model;
using System;
using System.Text.Json;
using System.Threading.Tasks;

namespace PeachtreeBus
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
        MessageContext GetNextMessage(string queueName);

        /// <summary>
        /// Marks a message as successfully processed.
        /// </summary>
        /// <param name="messageContext"></param>
        void CompleteMessage(MessageContext messageContext);

        /// <summary>
        /// Sets a message to be processed later.
        /// </summary>
        /// <param name="messageContext"></param>
        /// <param name="seconds"></param>
        void DelayMessage(MessageContext messageContext, int seconds);

        /// <summary>
        /// Increases the retry count on the message.
        /// Records the exception details.
        /// Marks the message failed if it exceeded the retry count.
        /// </summary>
        /// <param name="messageContext"></param>
        /// <param name="exception"></param>
        void FailMessage(MessageContext messageContext, Exception exception);

        /// <summary>
        /// Reads and deserialized saga data into the Message Context.
        /// and the saga itself.
        /// </summary>
        /// <param name="saga">The saga to load data for.</param>
        /// <param name="context">The mesage context. SagaKey must be set.</param>
        Task LoadSagaData(object saga, MessageContext context);

        /// <summary>
        /// Stores the saga data in the database after a message is processed.
        /// If the saga is compelte, the data is deleted from the database.
        /// </summary>
        /// <param name="saga"></param>
        /// <param name="context"></param>
        void PersistSagaData(object saga, MessageContext context);
     }


    /// <summary>
    /// Implements IQueueReader Using an IBusDataAccess and JSON serialization.
    /// </summary>
    public class QueueReader :IQueueReader
    {
        private readonly IBusDataAccess _dataAccess;
        private readonly ILog<QueueReader> _log;

        public byte MaxRetries { get; set; }

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="dataAccess">The Data access.</param>
        /// <param name="log"></param>
        public QueueReader(IBusDataAccess dataAccess, ILog<QueueReader> log)
        {
            _log = log;
            _dataAccess = dataAccess;
            MaxRetries = 5;
        }

        /// <inheritdoc/>
        public MessageContext GetNextMessage(string queueName)
        {
            // get a message.
            // if it retuned null there is no message to pocess currently.
            var queueMessage = _dataAccess.GetOneQueueMessage(queueName);
            if (queueMessage == null) return null;


            // deserialize the headers.
            Headers headers;
            try
            {
                headers = JsonSerializer.Deserialize<Headers>(queueMessage.Headers);
            }
            catch
            {
                _log.Warn($"Headers could not be deserialized for message {queueMessage.MessageId}.");
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
                message = JsonSerializer.Deserialize(queueMessage.Body, messageType);
            }
            else
            {
                _log.Warn($"Message class {headers.MessageClass} was not recognized for message {queueMessage.MessageId}.");
            }

            // return the new message context.
            return new MessageContext
            {
                MessageData = queueMessage,
                Headers = headers,
                Message = message,
                SourceQueue = queueName
            };
        }

        /// <inheritdoc/>
        public void CompleteMessage(MessageContext messageContext)
        {
            messageContext.MessageData.Completed = DateTime.UtcNow;
            _dataAccess.Update(messageContext.MessageData, messageContext.SourceQueue);
        }

        /// <inheritdoc/>
        public void FailMessage(MessageContext messageContext, Exception exception)
        {
            messageContext.MessageData.Retries++;
            messageContext.MessageData.NotBefore = DateTime.UtcNow.AddSeconds(5 * messageContext.MessageData.Retries); // Wait longer between retries.
            messageContext.Headers.ExceptionDetails = exception.ToString();
            messageContext.MessageData.Headers = JsonSerializer.Serialize(messageContext.Headers);
            if (messageContext.MessageData.Retries >= MaxRetries)
            {
                _log.Error($"Message {messageContext.MessageData.MessageId} exceeded max retries ({MaxRetries}) and has failed.");
                messageContext.MessageData.Failed = DateTime.UtcNow;
            }
            else
            {
                _log.Error($"Message {messageContext.MessageData.MessageId} will be retried at {messageContext.MessageData.NotBefore}.");
            }

            _dataAccess.Update(messageContext.MessageData, messageContext.SourceQueue);
        }

        /// <inheritdoc/>
        public async Task LoadSagaData(object saga, MessageContext messageContext)
        {
            // work out the class name of the saga.
            var sagaType = saga.GetType();
            var nameProperty = sagaType.GetProperty("SagaName");
            var sagaName = (string)nameProperty.GetValue(saga);

            // this is a bit confusing because it returns true if another connection to the database locked it.
            // but it will lock it for us if it can. 
            messageContext.SagaLocked = await _dataAccess.IsSagaLocked(sagaName, messageContext.SagaKey);
            if (messageContext.SagaLocked) return;

            // fetch the data from the DB.
            messageContext.SagaData = _dataAccess.GetSagaData(sagaName, messageContext.SagaKey);
            
            // Hypothetically locked could be false, and sagadata could be null if the saga hasn't been started.
            // and if two saga starts are processed at the same time, a second insert will occur and 
            // that will fail with a duplicate key constraint.

            // determine the type to deserialze to or create.
            var dataProperty = sagaType.GetProperty("Data");
            var sagaDataType = dataProperty.PropertyType;

            object dataObject;
            if (messageContext.SagaData == null)
            {
                // no data in the DB, create a new object.
                dataObject = Activator.CreateInstance(sagaDataType);
            }
            else 
            {
                // deserialize
                // try catch needed? Probably better to throw and let the error handling deal with it.
                // Someone may have to fix the saga data and retry the failed message though.
                dataObject = JsonSerializer.Deserialize(messageContext.SagaData.Data, sagaDataType);
            }

            // assign the data to the saga.
            dataProperty.SetValue(saga, dataObject);
        }


        /// <inheritdoc/>
        public void PersistSagaData(object saga, MessageContext context)
        {
            var sagaType = saga.GetType();
            var nameProperty = sagaType.GetProperty("SagaName");
            var sagaName = (string)nameProperty.GetValue(saga);

            // if the saga is complete, we can delete the data.
            var completeProperty = sagaType.GetProperty("SagaComplete");
            bool IsComplete = completeProperty.GetValue(saga) is bool completeValue && completeValue;
            if (IsComplete)
            {
                var rowsDeleted = _dataAccess.DeleteSagaData(sagaName, context.SagaKey);
                if (rowsDeleted != 1)
                {
                    throw new ApplicationException("Too many Saga Data rows deleted.");
                }
                return;
            }

            // the saga is not complete, serialize it.
            var dataProperty = sagaType.GetProperty("Data");
            var dataObject = dataProperty.GetValue(saga);
            if (dataObject == null) dataObject = Activator.CreateInstance(dataProperty.PropertyType);
            var serializedData = JsonSerializer.Serialize(dataObject, dataProperty.PropertyType);



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
                _dataAccess.Insert(context.SagaData, sagaName);
            }
            else
            {
                // update the existing row.
                context.SagaData.Data = serializedData;
                _dataAccess.Update(context.SagaData, sagaName);
            }
        }

        public void DelayMessage(MessageContext messageContext, int ms)
        {
            messageContext.MessageData.NotBefore = DateTime.UtcNow.AddMilliseconds(ms);
            _dataAccess.Update(messageContext.MessageData, messageContext.SourceQueue);
        }
    }
}
