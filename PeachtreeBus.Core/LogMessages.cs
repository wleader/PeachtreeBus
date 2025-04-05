//------------------------------------------------------
// This is a generated file. Do not make manual changes.
//------------------------------------------------------
using PeachtreeBus.Data;
using PeachtreeBus.Queues;
using PeachtreeBus.Sagas;
using PeachtreeBus.Subscriptions;
using Microsoft.Extensions.Logging;
using System;
using System.Diagnostics.CodeAnalysis;

namespace PeachtreeBus
{
    [ExcludeFromCodeCoverage]
	public static class GeneratedLoggerMessages
	{
        internal static readonly EventId PeachtreeBus_BaseThread_ThreadStart_Event
            = new(1001001, "PeachtreeBus_BaseThread_ThreadStart");
        internal static readonly Action<ILogger, string, Exception> PeachtreeBus_BaseThread_ThreadStart_Action
            = LoggerMessage.Define<string>(LogLevel.Information,
                PeachtreeBus_BaseThread_ThreadStart_Event,
                "Starting {ThreadName} thread.");
        /// <summary>
        /// (1001001) Information: Starting {ThreadName} thread.
        /// </summary>
        public static void BaseThread_ThreadStart(this ILogger logger, string threadName)
            => PeachtreeBus_BaseThread_ThreadStart_Action(logger, threadName, null!);

        internal static readonly EventId PeachtreeBus_BaseThread_ThreadStop_Event
            = new(1001002, "PeachtreeBus_BaseThread_ThreadStop");
        internal static readonly Action<ILogger, string, Exception> PeachtreeBus_BaseThread_ThreadStop_Action
            = LoggerMessage.Define<string>(LogLevel.Information,
                PeachtreeBus_BaseThread_ThreadStop_Event,
                "Thread {ThreadName} stopped.");
        /// <summary>
        /// (1001002) Information: Thread {ThreadName} stopped.
        /// </summary>
        public static void BaseThread_ThreadStop(this ILogger logger, string threadName)
            => PeachtreeBus_BaseThread_ThreadStop_Action(logger, threadName, null!);

        internal static readonly EventId PeachtreeBus_BaseThread_ThreadError_Event
            = new(1001003, "PeachtreeBus_BaseThread_ThreadError");
        internal static readonly Action<ILogger, string, Exception> PeachtreeBus_BaseThread_ThreadError_Action
            = LoggerMessage.Define<string>(LogLevel.Error,
                PeachtreeBus_BaseThread_ThreadError_Event,
                "Thread {ThreadName} errored.");
        /// <summary>
        /// (1001003) Error: Thread {ThreadName} errored.
        /// </summary>
        public static void BaseThread_ThreadError(this ILogger logger, string threadName, Exception ex)
            => PeachtreeBus_BaseThread_ThreadError_Action(logger, threadName, ex);

        internal static readonly EventId PeachtreeBus_BaseThread_RollbackFailed_Event
            = new(1001004, "PeachtreeBus_BaseThread_RollbackFailed");
        internal static readonly Action<ILogger, string, Exception> PeachtreeBus_BaseThread_RollbackFailed_Action
            = LoggerMessage.Define<string>(LogLevel.Error,
                PeachtreeBus_BaseThread_RollbackFailed_Event,
                "Thread {ThreadName} failed to roll back its transaction.");
        /// <summary>
        /// (1001004) Error: Thread {ThreadName} failed to roll back its transaction.
        /// </summary>
        public static void BaseThread_RollbackFailed(this ILogger logger, string threadName, Exception ex)
            => PeachtreeBus_BaseThread_RollbackFailed_Action(logger, threadName, ex);

        internal static readonly EventId PeachtreeBus_Data_DapperDataAccess_DataAccessError_Event
            = new(2001001, "PeachtreeBus_Data_DapperDataAccess_DataAccessError");
        internal static readonly Action<ILogger, string, Exception> PeachtreeBus_Data_DapperDataAccess_DataAccessError_Action
            = LoggerMessage.Define<string>(LogLevel.Error,
                PeachtreeBus_Data_DapperDataAccess_DataAccessError_Event,
                "There was an exception interacting with the database. Method: {Method}.");
        /// <summary>
        /// (2001001) Error: There was an exception interacting with the database. Method: {Method}.
        /// </summary>
        public static void DapperDataAccess_DataAccessError(this ILogger logger, string method, Exception ex)
            => PeachtreeBus_Data_DapperDataAccess_DataAccessError_Action(logger, method, ex);

        internal static readonly EventId PeachtreeBus_Queues_QueueReader_HeaderNotDeserializable_Event
            = new(3001001, "PeachtreeBus_Queues_QueueReader_HeaderNotDeserializable");
        internal static readonly Action<ILogger, UniqueIdentity, QueueName, Exception> PeachtreeBus_Queues_QueueReader_HeaderNotDeserializable_Action
            = LoggerMessage.Define<UniqueIdentity, QueueName>(LogLevel.Warning,
                PeachtreeBus_Queues_QueueReader_HeaderNotDeserializable_Event,
                "Headers could not be deserialized for message {MessageId} from queue {QueueName}.");
        /// <summary>
        /// (3001001) Warning: Headers could not be deserialized for message {MessageId} from queue {QueueName}.
        /// </summary>
        public static void QueueReader_HeaderNotDeserializable(this ILogger logger, UniqueIdentity messageId, QueueName queueName)
            => PeachtreeBus_Queues_QueueReader_HeaderNotDeserializable_Action(logger, messageId, queueName, null!);

        internal static readonly EventId PeachtreeBus_Queues_QueueReader_BodyNotDeserializable_Event
            = new(3001002, "PeachtreeBus_Queues_QueueReader_BodyNotDeserializable");
        internal static readonly Action<ILogger, UniqueIdentity, QueueName, Exception> PeachtreeBus_Queues_QueueReader_BodyNotDeserializable_Action
            = LoggerMessage.Define<UniqueIdentity, QueueName>(LogLevel.Warning,
                PeachtreeBus_Queues_QueueReader_BodyNotDeserializable_Event,
                "Body could not be deserialized for message {MessageId} from queue {QueueName}.");
        /// <summary>
        /// (3001002) Warning: Body could not be deserialized for message {MessageId} from queue {QueueName}.
        /// </summary>
        public static void QueueReader_BodyNotDeserializable(this ILogger logger, UniqueIdentity messageId, QueueName queueName, Exception ex)
            => PeachtreeBus_Queues_QueueReader_BodyNotDeserializable_Action(logger, messageId, queueName, ex);

        internal static readonly EventId PeachtreeBus_Queues_QueueReader_MessageClassNotRecognized_Event
            = new(3001003, "PeachtreeBus_Queues_QueueReader_MessageClassNotRecognized");
        internal static readonly Action<ILogger, string, UniqueIdentity, QueueName, Exception> PeachtreeBus_Queues_QueueReader_MessageClassNotRecognized_Action
            = LoggerMessage.Define<string, UniqueIdentity, QueueName>(LogLevel.Warning,
                PeachtreeBus_Queues_QueueReader_MessageClassNotRecognized_Event,
                "Message class '{MessageClass}' was not recognized for message {MessageId} from queue {QueueName}.");
        /// <summary>
        /// (3001003) Warning: Message class '{MessageClass}' was not recognized for message {MessageId} from queue {QueueName}.
        /// </summary>
        public static void QueueReader_MessageClassNotRecognized(this ILogger logger, string messageClass, UniqueIdentity messageId, QueueName queueName)
            => PeachtreeBus_Queues_QueueReader_MessageClassNotRecognized_Action(logger, messageClass, messageId, queueName, null!);

        internal static readonly EventId PeachtreeBus_Queues_QueueReader_MessageFailed_Event
            = new(3001004, "PeachtreeBus_Queues_QueueReader_MessageFailed");
        internal static readonly Action<ILogger, UniqueIdentity, QueueName, Exception> PeachtreeBus_Queues_QueueReader_MessageFailed_Action
            = LoggerMessage.Define<UniqueIdentity, QueueName>(LogLevel.Warning,
                PeachtreeBus_Queues_QueueReader_MessageFailed_Event,
                "Message {MessageId} from queue {QueueName} has failed and will not be retried.");
        /// <summary>
        /// (3001004) Warning: Message {MessageId} from queue {QueueName} has failed and will not be retried.
        /// </summary>
        public static void QueueReader_MessageFailed(this ILogger logger, UniqueIdentity messageId, QueueName queueName)
            => PeachtreeBus_Queues_QueueReader_MessageFailed_Action(logger, messageId, queueName, null!);

        internal static readonly EventId PeachtreeBus_Queues_QueueReader_MessageWillBeRetried_Event
            = new(3001005, "PeachtreeBus_Queues_QueueReader_MessageWillBeRetried");
        internal static readonly Action<ILogger, UniqueIdentity, QueueName, DateTime, Exception> PeachtreeBus_Queues_QueueReader_MessageWillBeRetried_Action
            = LoggerMessage.Define<UniqueIdentity, QueueName, DateTime>(LogLevel.Warning,
                PeachtreeBus_Queues_QueueReader_MessageWillBeRetried_Event,
                "Message {MessageId} from queue {QueueName} will be retried after {NotBefore}.");
        /// <summary>
        /// (3001005) Warning: Message {MessageId} from queue {QueueName} will be retried after {NotBefore}.
        /// </summary>
        public static void QueueReader_MessageWillBeRetried(this ILogger logger, UniqueIdentity messageId, QueueName queueName, DateTime notBefore)
            => PeachtreeBus_Queues_QueueReader_MessageWillBeRetried_Action(logger, messageId, queueName, notBefore, null!);

        internal static readonly EventId PeachtreeBus_Queues_QueueReader_LoadingSagaData_Event
            = new(3001006, "PeachtreeBus_Queues_QueueReader_LoadingSagaData");
        internal static readonly Action<ILogger, SagaName, SagaKey, Exception> PeachtreeBus_Queues_QueueReader_LoadingSagaData_Action
            = LoggerMessage.Define<SagaName, SagaKey>(LogLevel.Information,
                PeachtreeBus_Queues_QueueReader_LoadingSagaData_Event,
                "Loading saga data for {SagaName} - {SagaKey}.");
        /// <summary>
        /// (3001006) Information: Loading saga data for {SagaName} - {SagaKey}.
        /// </summary>
        public static void QueueReader_LoadingSagaData(this ILogger logger, SagaName sagaName, SagaKey sagaKey)
            => PeachtreeBus_Queues_QueueReader_LoadingSagaData_Action(logger, sagaName, sagaKey, null!);

        internal static readonly EventId PeachtreeBus_Queues_QueueReader_DeletingSagaData_Event
            = new(3001007, "PeachtreeBus_Queues_QueueReader_DeletingSagaData");
        internal static readonly Action<ILogger, SagaName, SagaKey, Exception> PeachtreeBus_Queues_QueueReader_DeletingSagaData_Action
            = LoggerMessage.Define<SagaName, SagaKey>(LogLevel.Information,
                PeachtreeBus_Queues_QueueReader_DeletingSagaData_Event,
                "Deleting saga data for {SagaName} - {SagaKey}.");
        /// <summary>
        /// (3001007) Information: Deleting saga data for {SagaName} - {SagaKey}.
        /// </summary>
        public static void QueueReader_DeletingSagaData(this ILogger logger, SagaName sagaName, SagaKey sagaKey)
            => PeachtreeBus_Queues_QueueReader_DeletingSagaData_Action(logger, sagaName, sagaKey, null!);

        internal static readonly EventId PeachtreeBus_Queues_QueueReader_SavingSagaData_Event
            = new(3001008, "PeachtreeBus_Queues_QueueReader_SavingSagaData");
        internal static readonly Action<ILogger, SagaName, SagaKey, Exception> PeachtreeBus_Queues_QueueReader_SavingSagaData_Action
            = LoggerMessage.Define<SagaName, SagaKey>(LogLevel.Information,
                PeachtreeBus_Queues_QueueReader_SavingSagaData_Event,
                "Saving saga data for {SagaName} - {SagaKey}.");
        /// <summary>
        /// (3001008) Information: Saving saga data for {SagaName} - {SagaKey}.
        /// </summary>
        public static void QueueReader_SavingSagaData(this ILogger logger, SagaName sagaName, SagaKey sagaKey)
            => PeachtreeBus_Queues_QueueReader_SavingSagaData_Action(logger, sagaName, sagaKey, null!);

        internal static readonly EventId PeachtreeBus_Queues_QueueWork_ProcessingMessage_Event
            = new(3002001, "PeachtreeBus_Queues_QueueWork_ProcessingMessage");
        internal static readonly Action<ILogger, UniqueIdentity, string, Exception> PeachtreeBus_Queues_QueueWork_ProcessingMessage_Action
            = LoggerMessage.Define<UniqueIdentity, string>(LogLevel.Debug,
                PeachtreeBus_Queues_QueueWork_ProcessingMessage_Event,
                "Processing Message {MessageId}, Type: {MessageClass}.");
        /// <summary>
        /// (3002001) Debug: Processing Message {MessageId}, Type: {MessageClass}.
        /// </summary>
        public static void QueueWork_ProcessingMessage(this ILogger logger, UniqueIdentity messageId, string messageClass)
            => PeachtreeBus_Queues_QueueWork_ProcessingMessage_Action(logger, messageId, messageClass, null!);

        internal static readonly EventId PeachtreeBus_Queues_QueueWork_LoadingSaga_Event
            = new(3002002, "PeachtreeBus_Queues_QueueWork_LoadingSaga");
        internal static readonly Action<ILogger, string, SagaKey, Exception> PeachtreeBus_Queues_QueueWork_LoadingSaga_Action
            = LoggerMessage.Define<string, SagaKey>(LogLevel.Debug,
                PeachtreeBus_Queues_QueueWork_LoadingSaga_Event,
                "Saga Loading {SagaType} {SagaKey}.");
        /// <summary>
        /// (3002002) Debug: Saga Loading {SagaType} {SagaKey}.
        /// </summary>
        public static void QueueWork_LoadingSaga(this ILogger logger, string sagaType, SagaKey sagaKey)
            => PeachtreeBus_Queues_QueueWork_LoadingSaga_Action(logger, sagaType, sagaKey, null!);

        internal static readonly EventId PeachtreeBus_Queues_QueueWork_SagaBlocked_Event
            = new(3002003, "PeachtreeBus_Queues_QueueWork_SagaBlocked");
        internal static readonly Action<ILogger, string, SagaKey, Exception> PeachtreeBus_Queues_QueueWork_SagaBlocked_Action
            = LoggerMessage.Define<string, SagaKey>(LogLevel.Information,
                PeachtreeBus_Queues_QueueWork_SagaBlocked_Event,
                "The saga {SagaType} for key {SagaKey} is blocked. The current message will be delayed and retried.");
        /// <summary>
        /// (3002003) Information: The saga {SagaType} for key {SagaKey} is blocked. The current message will be delayed and retried.
        /// </summary>
        public static void QueueWork_SagaBlocked(this ILogger logger, string sagaType, SagaKey sagaKey)
            => PeachtreeBus_Queues_QueueWork_SagaBlocked_Action(logger, sagaType, sagaKey, null!);

        internal static readonly EventId PeachtreeBus_Queues_QueueWork_InvokeHandler_Event
            = new(3002004, "PeachtreeBus_Queues_QueueWork_InvokeHandler");
        internal static readonly Action<ILogger, UniqueIdentity, string, string, Exception> PeachtreeBus_Queues_QueueWork_InvokeHandler_Action
            = LoggerMessage.Define<UniqueIdentity, string, string>(LogLevel.Debug,
                PeachtreeBus_Queues_QueueWork_InvokeHandler_Event,
                "Handling message {MessageId} of type {MessageClass} with {HandlerType}.");
        /// <summary>
        /// (3002004) Debug: Handling message {MessageId} of type {MessageClass} with {HandlerType}.
        /// </summary>
        public static void QueueWork_InvokeHandler(this ILogger logger, UniqueIdentity messageId, string messageClass, string handlerType)
            => PeachtreeBus_Queues_QueueWork_InvokeHandler_Action(logger, messageId, messageClass, handlerType, null!);

        internal static readonly EventId PeachtreeBus_Queues_QueueWork_SagaSaved_Event
            = new(3002005, "PeachtreeBus_Queues_QueueWork_SagaSaved");
        internal static readonly Action<ILogger, string, SagaKey, Exception> PeachtreeBus_Queues_QueueWork_SagaSaved_Action
            = LoggerMessage.Define<string, SagaKey>(LogLevel.Debug,
                PeachtreeBus_Queues_QueueWork_SagaSaved_Event,
                "Saga Saved {SagaType} {SagaKey}.");
        /// <summary>
        /// (3002005) Debug: Saga Saved {SagaType} {SagaKey}.
        /// </summary>
        public static void QueueWork_SagaSaved(this ILogger logger, string sagaType, SagaKey sagaKey)
            => PeachtreeBus_Queues_QueueWork_SagaSaved_Action(logger, sagaType, sagaKey, null!);

        internal static readonly EventId PeachtreeBus_Queues_QueueWork_HandlerException_Event
            = new(3002006, "PeachtreeBus_Queues_QueueWork_HandlerException");
        internal static readonly Action<ILogger, string, UniqueIdentity, string, Exception> PeachtreeBus_Queues_QueueWork_HandlerException_Action
            = LoggerMessage.Define<string, UniqueIdentity, string>(LogLevel.Warning,
                PeachtreeBus_Queues_QueueWork_HandlerException_Event,
                "There was an exception in {HandlerType} when handling Message {MessageId} of type {MessageType}.");
        /// <summary>
        /// (3002006) Warning: There was an exception in {HandlerType} when handling Message {MessageId} of type {MessageType}.
        /// </summary>
        public static void QueueWork_HandlerException(this ILogger logger, string handlerType, UniqueIdentity messageId, string messageType, Exception ex)
            => PeachtreeBus_Queues_QueueWork_HandlerException_Action(logger, handlerType, messageId, messageType, ex);

        internal static readonly EventId PeachtreeBus_Queues_QueueWork_SagaNotStarted_Event
            = new(3002007, "PeachtreeBus_Queues_QueueWork_SagaNotStarted");
        internal static readonly Action<ILogger, string, SagaKey, UniqueIdentity, Exception> PeachtreeBus_Queues_QueueWork_SagaNotStarted_Action
            = LoggerMessage.Define<string, SagaKey, UniqueIdentity>(LogLevel.Information,
                PeachtreeBus_Queues_QueueWork_SagaNotStarted_Event,
                "The saga {SagaType} for key '{SagaKey}' has not been started, or has completed. The Message {MessageId} will not be handled by the saga.");
        /// <summary>
        /// (3002007) Information: The saga {SagaType} for key '{SagaKey}' has not been started, or has completed. The Message {MessageId} will not be handled by the saga.
        /// </summary>
        public static void QueueWork_SagaNotStarted(this ILogger logger, string sagaType, SagaKey sagaKey, UniqueIdentity messageId)
            => PeachtreeBus_Queues_QueueWork_SagaNotStarted_Action(logger, sagaType, sagaKey, messageId, null!);

        internal static readonly EventId PeachtreeBus_Subscriptions_SubscribedWork_ProcessingMessage_Event
            = new(4001001, "PeachtreeBus_Subscriptions_SubscribedWork_ProcessingMessage");
        internal static readonly Action<ILogger, UniqueIdentity, SubscriberId, Exception> PeachtreeBus_Subscriptions_SubscribedWork_ProcessingMessage_Action
            = LoggerMessage.Define<UniqueIdentity, SubscriberId>(LogLevel.Debug,
                PeachtreeBus_Subscriptions_SubscribedWork_ProcessingMessage_Event,
                "Processing message {MessageId} for subscriber {SubscriberId}.");
        /// <summary>
        /// (4001001) Debug: Processing message {MessageId} for subscriber {SubscriberId}.
        /// </summary>
        public static void SubscribedWork_ProcessingMessage(this ILogger logger, UniqueIdentity messageId, SubscriberId subscriberId)
            => PeachtreeBus_Subscriptions_SubscribedWork_ProcessingMessage_Action(logger, messageId, subscriberId, null!);

        internal static readonly EventId PeachtreeBus_Subscriptions_SubscribedWork_MessageHandlerException_Event
            = new(4001002, "PeachtreeBus_Subscriptions_SubscribedWork_MessageHandlerException");
        internal static readonly Action<ILogger, UniqueIdentity, SubscriberId, Exception> PeachtreeBus_Subscriptions_SubscribedWork_MessageHandlerException_Action
            = LoggerMessage.Define<UniqueIdentity, SubscriberId>(LogLevel.Warning,
                PeachtreeBus_Subscriptions_SubscribedWork_MessageHandlerException_Event,
                "There was an exception while processing message {MessageId} for subscriber {SusbscriberId}.");
        /// <summary>
        /// (4001002) Warning: There was an exception while processing message {MessageId} for subscriber {SusbscriberId}.
        /// </summary>
        public static void SubscribedWork_MessageHandlerException(this ILogger logger, UniqueIdentity messageId, SubscriberId susbscriberId, Exception ex)
            => PeachtreeBus_Subscriptions_SubscribedWork_MessageHandlerException_Action(logger, messageId, susbscriberId, ex);

        internal static readonly EventId PeachtreeBus_Subscriptions_SubscribedReader_MessageFailed_Event
            = new(4002001, "PeachtreeBus_Subscriptions_SubscribedReader_MessageFailed");
        internal static readonly Action<ILogger, UniqueIdentity, SubscriberId, Exception> PeachtreeBus_Subscriptions_SubscribedReader_MessageFailed_Action
            = LoggerMessage.Define<UniqueIdentity, SubscriberId>(LogLevel.Warning,
                PeachtreeBus_Subscriptions_SubscribedReader_MessageFailed_Event,
                "Message {MessageId} for Subscriber {SubscriberId} has failed and will not be retried.");
        /// <summary>
        /// (4002001) Warning: Message {MessageId} for Subscriber {SubscriberId} has failed and will not be retried.
        /// </summary>
        public static void SubscribedReader_MessageFailed(this ILogger logger, UniqueIdentity messageId, SubscriberId subscriberId)
            => PeachtreeBus_Subscriptions_SubscribedReader_MessageFailed_Action(logger, messageId, subscriberId, null!);

        internal static readonly EventId PeachtreeBus_Subscriptions_SubscribedReader_HeaderNotDeserializable_Event
            = new(4002002, "PeachtreeBus_Subscriptions_SubscribedReader_HeaderNotDeserializable");
        internal static readonly Action<ILogger, UniqueIdentity, SubscriberId, Exception> PeachtreeBus_Subscriptions_SubscribedReader_HeaderNotDeserializable_Action
            = LoggerMessage.Define<UniqueIdentity, SubscriberId>(LogLevel.Warning,
                PeachtreeBus_Subscriptions_SubscribedReader_HeaderNotDeserializable_Event,
                "Headers could not be deserialized for message {MessageId} for subscriber {SubscriberId}.");
        /// <summary>
        /// (4002002) Warning: Headers could not be deserialized for message {MessageId} for subscriber {SubscriberId}.
        /// </summary>
        public static void SubscribedReader_HeaderNotDeserializable(this ILogger logger, UniqueIdentity messageId, SubscriberId subscriberId)
            => PeachtreeBus_Subscriptions_SubscribedReader_HeaderNotDeserializable_Action(logger, messageId, subscriberId, null!);

        internal static readonly EventId PeachtreeBus_Subscriptions_SubscribedReader_BodyNotDeserializable_Event
            = new(4002003, "PeachtreeBus_Subscriptions_SubscribedReader_BodyNotDeserializable");
        internal static readonly Action<ILogger, UniqueIdentity, SubscriberId, Exception> PeachtreeBus_Subscriptions_SubscribedReader_BodyNotDeserializable_Action
            = LoggerMessage.Define<UniqueIdentity, SubscriberId>(LogLevel.Warning,
                PeachtreeBus_Subscriptions_SubscribedReader_BodyNotDeserializable_Event,
                "Message Body could not be deserialized for message {MessageId} for subscriber {SubscriberId}.");
        /// <summary>
        /// (4002003) Warning: Message Body could not be deserialized for message {MessageId} for subscriber {SubscriberId}.
        /// </summary>
        public static void SubscribedReader_BodyNotDeserializable(this ILogger logger, UniqueIdentity messageId, SubscriberId subscriberId, Exception ex)
            => PeachtreeBus_Subscriptions_SubscribedReader_BodyNotDeserializable_Action(logger, messageId, subscriberId, ex);

        internal static readonly EventId PeachtreeBus_Subscriptions_SubscribedReader_MessageClassNotRecognized_Event
            = new(4002004, "PeachtreeBus_Subscriptions_SubscribedReader_MessageClassNotRecognized");
        internal static readonly Action<ILogger, string, UniqueIdentity, SubscriberId, Exception> PeachtreeBus_Subscriptions_SubscribedReader_MessageClassNotRecognized_Action
            = LoggerMessage.Define<string, UniqueIdentity, SubscriberId>(LogLevel.Warning,
                PeachtreeBus_Subscriptions_SubscribedReader_MessageClassNotRecognized_Event,
                "Message class '{MessageClass}' was not recognized for message {MessageId} for subscriber {SubscriberId}.");
        /// <summary>
        /// (4002004) Warning: Message class '{MessageClass}' was not recognized for message {MessageId} for subscriber {SubscriberId}.
        /// </summary>
        public static void SubscribedReader_MessageClassNotRecognized(this ILogger logger, string messageClass, UniqueIdentity messageId, SubscriberId subscriberId)
            => PeachtreeBus_Subscriptions_SubscribedReader_MessageClassNotRecognized_Action(logger, messageClass, messageId, subscriberId, null!);

        internal static readonly EventId PeachtreeBus_Subscriptions_SubscribedReader_MessageWillBeRetried_Event
            = new(4002005, "PeachtreeBus_Subscriptions_SubscribedReader_MessageWillBeRetried");
        internal static readonly Action<ILogger, UniqueIdentity, SubscriberId, DateTime, Exception> PeachtreeBus_Subscriptions_SubscribedReader_MessageWillBeRetried_Action
            = LoggerMessage.Define<UniqueIdentity, SubscriberId, DateTime>(LogLevel.Warning,
                PeachtreeBus_Subscriptions_SubscribedReader_MessageWillBeRetried_Event,
                "Message {MessageId} from Subscriber {SubscriberId} will be retried after {NotBefore}.");
        /// <summary>
        /// (4002005) Warning: Message {MessageId} from Subscriber {SubscriberId} will be retried after {NotBefore}.
        /// </summary>
        public static void SubscribedReader_MessageWillBeRetried(this ILogger logger, UniqueIdentity messageId, SubscriberId subscriberId, DateTime notBefore)
            => PeachtreeBus_Subscriptions_SubscribedReader_MessageWillBeRetried_Action(logger, messageId, subscriberId, notBefore, null!);

        internal static readonly EventId PeachtreeBus_Errors_QueueFailures_NoHandler_Event
            = new(5001001, "PeachtreeBus_Errors_QueueFailures_NoHandler");
        internal static readonly Action<ILogger, Exception> PeachtreeBus_Errors_QueueFailures_NoHandler_Action
            = LoggerMessage.Define(LogLevel.Error,
                PeachtreeBus_Errors_QueueFailures_NoHandler_Event,
                "No implemenation of IHandleFailedQueueMessages was available. You can register DefaultFailedQueueMessageHandler with your dependency injection provider to disable this message.");
        /// <summary>
        /// (5001001) Error: No implemenation of IHandleFailedQueueMessages was available. You can register DefaultFailedQueueMessageHandler with your dependency injection provider to disable this message.
        /// </summary>
        public static void QueueFailures_NoHandler(this ILogger logger, Exception ex)
            => PeachtreeBus_Errors_QueueFailures_NoHandler_Action(logger, ex);

        internal static readonly EventId PeachtreeBus_Errors_QueueFailures_HandlerThrow_Event
            = new(5001002, "PeachtreeBus_Errors_QueueFailures_HandlerThrow");
        internal static readonly Action<ILogger, Type, Type, Exception> PeachtreeBus_Errors_QueueFailures_HandlerThrow_Action
            = LoggerMessage.Define<Type, Type>(LogLevel.Error,
                PeachtreeBus_Errors_QueueFailures_HandlerThrow_Event,
                "Failed Queue Message Handler {HandlerType} threw an exception while handling a message of type {MessageType}. Database will be rolled back.");
        /// <summary>
        /// (5001002) Error: Failed Queue Message Handler {HandlerType} threw an exception while handling a message of type {MessageType}. Database will be rolled back.
        /// </summary>
        public static void QueueFailures_HandlerThrow(this ILogger logger, Type handlerType, Type messageType, Exception ex)
            => PeachtreeBus_Errors_QueueFailures_HandlerThrow_Action(logger, handlerType, messageType, ex);

        internal static readonly EventId PeachtreeBus_Errors_QueueFailures_MessageFailed_Event
            = new(5001003, "PeachtreeBus_Errors_QueueFailures_MessageFailed");
        internal static readonly Action<ILogger, Type, Exception> PeachtreeBus_Errors_QueueFailures_MessageFailed_Action
            = LoggerMessage.Define<Type>(LogLevel.Warning,
                PeachtreeBus_Errors_QueueFailures_MessageFailed_Event,
                "A queue message of type {MessageType} exceeded its maximum retries and has failed.");
        /// <summary>
        /// (5001003) Warning: A queue message of type {MessageType} exceeded its maximum retries and has failed.
        /// </summary>
        public static void QueueFailures_MessageFailed(this ILogger logger, Type messageType)
            => PeachtreeBus_Errors_QueueFailures_MessageFailed_Action(logger, messageType, null!);

        internal static readonly EventId PeachtreeBus_Errors_SubscribedFailures_NoHandler_Event
            = new(5002001, "PeachtreeBus_Errors_SubscribedFailures_NoHandler");
        internal static readonly Action<ILogger, Exception> PeachtreeBus_Errors_SubscribedFailures_NoHandler_Action
            = LoggerMessage.Define(LogLevel.Error,
                PeachtreeBus_Errors_SubscribedFailures_NoHandler_Event,
                "No implemenation of IHandleFailedSubscribedMessages was available. You can register DefaultFailedSubscribedMessageHandler with your dependency injection provider to disable this message.");
        /// <summary>
        /// (5002001) Error: No implemenation of IHandleFailedSubscribedMessages was available. You can register DefaultFailedSubscribedMessageHandler with your dependency injection provider to disable this message.
        /// </summary>
        public static void SubscribedFailures_NoHandler(this ILogger logger, Exception ex)
            => PeachtreeBus_Errors_SubscribedFailures_NoHandler_Action(logger, ex);

        internal static readonly EventId PeachtreeBus_Errors_SubscribedFailures_HandlerThrow_Event
            = new(5002002, "PeachtreeBus_Errors_SubscribedFailures_HandlerThrow");
        internal static readonly Action<ILogger, Type, Type, Exception> PeachtreeBus_Errors_SubscribedFailures_HandlerThrow_Action
            = LoggerMessage.Define<Type, Type>(LogLevel.Error,
                PeachtreeBus_Errors_SubscribedFailures_HandlerThrow_Event,
                "Failed Subscribed Message Handler {HandlerType} threw an exception while handling a message of type {MessageType}. Database will be rolled back.");
        /// <summary>
        /// (5002002) Error: Failed Subscribed Message Handler {HandlerType} threw an exception while handling a message of type {MessageType}. Database will be rolled back.
        /// </summary>
        public static void SubscribedFailures_HandlerThrow(this ILogger logger, Type handlerType, Type messageType, Exception ex)
            => PeachtreeBus_Errors_SubscribedFailures_HandlerThrow_Action(logger, handlerType, messageType, ex);

        internal static readonly EventId PeachtreeBus_Errors_SubscribedFailures_MessageFailed_Event
            = new(5002003, "PeachtreeBus_Errors_SubscribedFailures_MessageFailed");
        internal static readonly Action<ILogger, Type, Exception> PeachtreeBus_Errors_SubscribedFailures_MessageFailed_Action
            = LoggerMessage.Define<Type>(LogLevel.Warning,
                PeachtreeBus_Errors_SubscribedFailures_MessageFailed_Event,
                "A subscribed message of type {MessageType} exceeded its maximum retries and has failed.");
        /// <summary>
        /// (5002003) Warning: A subscribed message of type {MessageType} exceeded its maximum retries and has failed.
        /// </summary>
        public static void SubscribedFailures_MessageFailed(this ILogger logger, Type messageType)
            => PeachtreeBus_Errors_SubscribedFailures_MessageFailed_Action(logger, messageType, null!);

        internal static readonly EventId PeachtreeBus_Errors_CircuitBreaker_Cleared_Event
            = new(5003001, "PeachtreeBus_Errors_CircuitBreaker_Cleared");
        internal static readonly Action<ILogger, string, Exception> PeachtreeBus_Errors_CircuitBreaker_Cleared_Action
            = LoggerMessage.Define<string>(LogLevel.Warning,
                PeachtreeBus_Errors_CircuitBreaker_Cleared_Event,
                "The CircuitBreaker '{FriendlyName} is cleared.");
        /// <summary>
        /// (5003001) Warning: The CircuitBreaker '{FriendlyName} is cleared.
        /// </summary>
        public static void CircuitBreaker_Cleared(this ILogger logger, string friendlyName)
            => PeachtreeBus_Errors_CircuitBreaker_Cleared_Action(logger, friendlyName, null!);

        internal static readonly EventId PeachtreeBus_Errors_CircuitBreaker_Armed_Event
            = new(5003002, "PeachtreeBus_Errors_CircuitBreaker_Armed");
        internal static readonly Action<ILogger, string, Exception> PeachtreeBus_Errors_CircuitBreaker_Armed_Action
            = LoggerMessage.Define<string>(LogLevel.Warning,
                PeachtreeBus_Errors_CircuitBreaker_Armed_Event,
                "The CircuitBreaker '{FriendlyName} is armed.");
        /// <summary>
        /// (5003002) Warning: The CircuitBreaker '{FriendlyName} is armed.
        /// </summary>
        public static void CircuitBreaker_Armed(this ILogger logger, string friendlyName)
            => PeachtreeBus_Errors_CircuitBreaker_Armed_Action(logger, friendlyName, null!);

        internal static readonly EventId PeachtreeBus_Errors_CircuitBreaker_Faulted_Event
            = new(5003003, "PeachtreeBus_Errors_CircuitBreaker_Faulted");
        internal static readonly Action<ILogger, string, Exception> PeachtreeBus_Errors_CircuitBreaker_Faulted_Action
            = LoggerMessage.Define<string>(LogLevel.Warning,
                PeachtreeBus_Errors_CircuitBreaker_Faulted_Event,
                "The CircuitBreaker '{FriendlyName} is faulted.");
        /// <summary>
        /// (5003003) Warning: The CircuitBreaker '{FriendlyName} is faulted.
        /// </summary>
        public static void CircuitBreaker_Faulted(this ILogger logger, string friendlyName)
            => PeachtreeBus_Errors_CircuitBreaker_Faulted_Action(logger, friendlyName, null!);

	}
}
