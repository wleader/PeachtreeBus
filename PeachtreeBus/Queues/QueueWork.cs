using Microsoft.Extensions.Logging;
using PeachtreeBus.Data;
using PeachtreeBus.Sagas;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PeachtreeBus.Queues
{
    /// <summary>
    /// Reads one QueueMessage, and dispatches it through known message handlers.
    /// </summary>
    public interface IQueueWork : IUnitOfWork
    {
        public string QueueName { get; set; }
    }

    internal static class QueueWork_LogMessages
    {
        internal static Action<ILogger, Guid, string, Exception> QueueWork_ProcessingMessage_Action =
            LoggerMessage.Define<Guid, string>(
                LogLevel.Debug,
                Events.QueueWork_ProcessingMessage,
                "Processing Message {MessageId}, Type: {MessageClass}.");

        internal static void QueueWork_ProcessingMessage(this ILogger logger, Guid messageId, string messageClass)
        {
            QueueWork_ProcessingMessage_Action(logger, messageId, messageClass, null);
        }

        internal static Action<ILogger, string, string, Exception> QueueWork_LoadingSaga_Action =
            LoggerMessage.Define<string, string>(
                LogLevel.Debug,
                Events.QueueWork_LoadingSaga,
                "Saga Loading {SagaType} {SagaKey}");

        internal static void QueueWork_LoadingSaga(this ILogger logger, string sagaType, string sagaKey)
        {
            QueueWork_LoadingSaga_Action(logger, sagaType, sagaKey, null);
        }

        internal static Action<ILogger, string, string, Exception> QueueWork_SagaBlocked_Action =
            LoggerMessage.Define<string, string>(
                LogLevel.Information,
                Events.QueueWork_SagaBlocked,
                "The saga {SagaType} for key {SagaKey} is blocked. The current message will be delayed and retried.");

        internal static void QueueWork_SagaBlocked(this ILogger logger, string sagaType, string sagaKey)
        {
            QueueWork_SagaBlocked_Action(logger, sagaType, sagaKey, null);
        }

        internal static Action<ILogger, Guid, string, string, Exception> QueueWork_InvokeHandler_Action =
            LoggerMessage.Define<Guid, string, string>(
                LogLevel.Debug,
                Events.QueueWork_InvokeHandler,
                "Handling message {MessageId} of type {MessageClass} with {HandlerType}.");

        internal static void QueueWork_InvokeHandler(this ILogger logger, Guid messageId, string messageClass, string HandlerType)
        {
            QueueWork_InvokeHandler_Action(logger, messageId, messageClass, HandlerType, null);
        }

        internal static Action<ILogger, string, string, Exception> QueueWork_SagaSaved_Action =
            LoggerMessage.Define<string, string>(
                LogLevel.Debug,
                Events.QueueWork_SagaSaved,
                "Saga Saved {SagaType} {SagaKey}.");

        internal static void QueueWork_SagaSaved(this ILogger logger, string sagaType, string sagaKey)
        {
            QueueWork_SagaSaved_Action(logger, sagaType, sagaKey, null);
        }

        internal static Action<ILogger, string, Guid, string, Exception> QueueWork_HandlerException_Action =
            LoggerMessage.Define<string, Guid, string>(
                LogLevel.Warning,
                Events.QueueWork_HandlerException,
                "There was an exception in {HandlerType} when handling Message {MessageId} of type {MessageType}.");
            
        internal static void QueueWork_HandlerException(this ILogger logger, string handlerType, Guid messageId, string messageType, Exception ex)
        {
            QueueWork_HandlerException_Action(logger, handlerType, messageId, messageType, ex);
        }
    }

    /// <inheritdoc/>>
    public class QueueWork : IQueueWork
    {
        private readonly ILogger<QueueWork> _log;
        private readonly IPerfCounters _counters;
        private readonly IFindQueueHandlers _findHandlers;
        private readonly IQueueReader _queueReader;
        private readonly IBusDataAccess _dataAccess;
        private readonly ISagaMessageMapManager _sagaMessageMapManager;

        public QueueWork(
            ILogger<QueueWork> log,
            IPerfCounters counters,
            IFindQueueHandlers findHandlers,
            IQueueReader queueReader,
            IBusDataAccess dataAccess,
            ISagaMessageMapManager sagaMessageMapManager)
        {
            _log = log;
            _counters = counters;
            _findHandlers = findHandlers;
            _queueReader = queueReader;
            _dataAccess = dataAccess;
            _sagaMessageMapManager = sagaMessageMapManager;
        }

        public string QueueName { get; set; }

        /// <summary>
        /// Actually does the work of processing a single message.
        /// </summary>
        /// <param name="queueId"></param>
        /// <returns></returns>
        public async Task<bool> DoWork()
        {
            const string savepointName = "BeforeMessageHandler";

            // get a message.
            var messageContext = await _queueReader.GetNext(QueueName);

            // there are no messages, so we are done. Return false so the transaction will roll back,  will sleep for a while.
            if (messageContext == null)
            {
                return false;
            }

            // we found a message to process.
            _log.QueueWork_ProcessingMessage(messageContext.MessageData.MessageId, messageContext.Headers.MessageClass);
            var started = DateTime.UtcNow;
            string handlerTypeName = null!;
            try
            {
                _counters.StartMessage();

                // creat a save point. If anything goes wrong we can roll back to here,
                // increment the retry count and try again later.
                _dataAccess.CreateSavepoint(savepointName);

                // determine what type of message it is.
                var messageType = Type.GetType(messageContext.Headers.MessageClass);
                if (messageType == null)
                {
                    throw new ApplicationException($"Message {messageContext.MessageData.MessageId} is a message class of {messageContext.Headers.MessageClass} which was not a recognized type.");
                }

                // Get the message handlers for this message type from the Dependency Injection container.
                // the list will contain both regular handlers and sagas.
                // if a message has mulitple handlers, we'll get multiple handlers.
                var method = typeof(IFindQueueHandlers).GetMethod("FindHandlers");
                var genericMethod = method.MakeGenericMethod(messageType);
                var handlers = genericMethod.Invoke(_findHandlers, null);
                var castHandlers = (handlers as IEnumerable<object>).ToArray();

                // sanity check that the Depenency Injection container found at least one handler.
                // we shouldn't process a message that has no handlers.
                if (castHandlers.Length < 1)
                {
                    throw new ApplicationException($"There were no message handlers for {messageContext.Headers.MessageClass}.");
                }

                // invoke each of the handlers.
                foreach (var handler in castHandlers)
                {
                    // determine if this handler is a saga.
                    var handlerType = handler.GetType();
                    handlerTypeName = handlerType.FullName;

                    var handlerIsSaga = handlerType.IsSubclassOfSaga();
                    if (handlerIsSaga)
                    {
                        messageContext.SagaKey = _sagaMessageMapManager.GetKey(handler, messageContext.Message);
                        _log.QueueWork_LoadingSaga(handlerTypeName, messageContext.SagaKey);
                        
                        await _queueReader.LoadSaga(handler, messageContext);

                        if (messageContext.SagaData != null && messageContext.SagaData.Blocked)
                        {
                            // the saga is blocked. delay the message and try again later.
                            _log.QueueWork_SagaBlocked(handlerTypeName, messageContext.SagaKey);
                            _dataAccess.RollbackToSavepoint(savepointName);
                            await _queueReader.DelayMessage(messageContext, 250);
                            _counters.SagaBlocked();
                            return true;
                        }

                        if (messageContext.SagaData == null && !handlerType.IsSagaStartHandler(messageType))
                        {
                            // the saga was not locked, and it doesn't exist, and this message doesn't start a saga.
                            // we are processing a saga message but it is not a saga start message and we didnt read previous
                            // saga data from the DB. This means we are processing a non-start messge before the saga is started.
                            // we could continute but that would mean that the saga handler might not know it needs to initialize
                            // the saga data, so its better to stop and make things get fixed.
                            throw new ApplicationException($"A Message of Type {messageType} is being processed, but the saga {handlerType} has not been started for key {messageContext.SagaKey}. An IHandleSagaStartMessage<> handler on the saga must be processed first to start the saga.");
                        }
                    }

                    // find the right method on the handler.
                    var parameterTypes = new[] { typeof(QueueContext), messageType };
                    var handleMethod = handler.GetType().GetMethod("Handle", parameterTypes);


                    // Invoke and await the method.
                    // should it have a seperate try-catch around this and treat it differently?
                    // that would allow us to tell the difference between a problem in a handler, or if the problem was in the bus code.
                    // does that mater for the retry?
                    _log.QueueWork_InvokeHandler(messageContext.MessageData.MessageId, messageContext.Headers.MessageClass, handlerType.FullName);
                    {
                        var taskObject = handleMethod.Invoke(handler, new object[] { messageContext, messageContext.Message });
                        var castTask = taskObject as Task;
                        await castTask;
                    }

                    if (handlerIsSaga)
                    {
                        await _queueReader.SaveSaga(handler, messageContext);
                        _log.QueueWork_SagaSaved(handlerTypeName, messageContext.SagaKey);
                    }
                }

                // if nothing threw an exception, we can mark the message as processed.
                await _queueReader.Complete(messageContext);
                // return true so the transaction commits and the main loop looks for another mesage right away.
                return true;
            }
            catch (Exception ex)
            {
                // there was an exception, Rollback to the save point to undo
                // any db changes done by the handlers.
                _log.QueueWork_HandlerException(handlerTypeName, messageContext.MessageData.MessageId, messageContext.Headers.MessageClass, ex);
                _dataAccess.RollbackToSavepoint(savepointName);
                // increment the retry count, (or maybe even fail the message)
                await _queueReader.Fail(messageContext, ex);
                // return true so the transaction commits and the main loop looks for another mesage right away.
                return true;
            }
            finally
            {
                _counters.FinishMessage(started);
            }
        }
    }
}
