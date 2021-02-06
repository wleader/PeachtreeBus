using PeachtreeBus.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PeachtreeBus
{
    /// <summary>
    /// Define an task that will continuously read messages from a queue and attempt to process them
    /// </summary>
    public interface IMessageProcessor
    {
        Task Run(string queueName);
    }

    /// <summary>
    /// Reads messages from the queue and attempts to process them.
    /// </summary>
    public class MessageProcessor : IMessageProcessor
    {
        private readonly IProvideShutdownSignal _provideShutdownSignal;
        private readonly IQueueReader _queueReader;
        private readonly IQueueWriter _queueWriter;
        private readonly IBusDataAccess _transactionContext;
        private readonly IFindMessageHandlers _findMessageHandlers;
        private readonly ILog<MessageProcessor> _log;
        private readonly IPerfCounters _counters;

        public MessageProcessor(IProvideShutdownSignal provideShutdownSignal,
            IQueueReader queueReader,
            IQueueWriter queueWriter,
            IBusDataAccess transactionContext,
            IFindMessageHandlers findMessageHandlers,
            ILog<MessageProcessor> log,
            IPerfCounters counters)
        {
            _log = log;
            _provideShutdownSignal = provideShutdownSignal;
            _queueReader = queueReader;
            _queueWriter = queueWriter;
            _transactionContext = transactionContext;
            _findMessageHandlers = findMessageHandlers;
            _counters = counters;
        }

        /// <summary>
        /// Main Loop. Looks for a message, and if it processess successfully, commit the transaction.
        /// If a failure occurs rollback.
        /// </summary>
        /// <param name="queueId"></param>
        /// <returns></returns>
        public async Task Run(string queueName)
        {
            _log.Info($"Starting Message Processor");

            while (!_provideShutdownSignal.ShouldShutdown)
            {
                try
                {
                    _transactionContext.BeginTransaction();
                    if (await DoWork(queueName))
                    {
                        _transactionContext.CommitTransaction();
                    }
                    else
                    {
                        // there was no work to do.
                        // Rollback and go to sleep.
                        _transactionContext.RollbackTransaction();
                        await Task.Delay(100);
                    }

                }
                catch (Exception e)
                {
                    _transactionContext.RollbackTransaction();
                    _log.Error(e.ToString());
                }
            }
            return;
        }

        /// <summary>
        /// Actually does the work of processing a single message.
        /// </summary>
        /// <param name="queueId"></param>
        /// <returns></returns>
        private async Task<bool> DoWork(string queueName)
        {
            const string savepointName = "BeforeMessageHandler";

            // get a message.
            var messageContext = await _queueReader.GetNextMessage(queueName);

            // there are no messages, so we are done. Return false so the transaction will roll back,  will sleep for a while.
            if (messageContext == null)
            {
                return false;
            }

            // we found a message to process.
            _log.Debug($"Processing {messageContext.MessageData.MessageId}");
            var started = DateTime.UtcNow;
            try
            {
                _counters.StartMessage();

                // creat a save point. If anything goes wrong we can roll back to here,
                // increment the retry count and try again later.
                _transactionContext.CreateSavepoint(savepointName);

                // determine what type of message it is.
                var messageType = Type.GetType(messageContext.Headers.MessageClass);
                if (messageType == null)
                {
                    throw new ApplicationException($"Message {messageContext.MessageData.MessageId}  as a message class of {messageContext.Headers.MessageClass} which was not a recognized type.");
                }

                // Get the message handlers for this message type from the Dependency Injection container.
                // the list will contain both regular handlers and sagas.
                // if a message has mulitple handlers, we'll get multiple handlers.
                var method = typeof(IFindMessageHandlers).GetMethod("FindHandlers");
                var genericMethod = method.MakeGenericMethod(messageType);
                var handlers = genericMethod.Invoke(_findMessageHandlers, null);
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

                    var handlerIsSaga = IsSubclassOfSaga(handlerType);
                    if (handlerIsSaga)
                    {
                        messageContext.SagaKey = SagaMessageMapManager.GetKey(handler, messageContext.Message);
                        _log.Debug($"Active Saga {messageContext.SagaKey}");

                        await _queueReader.LoadSagaData(handler, messageContext);

                        if (messageContext.SagaData != null && messageContext.SagaData.Blocked)
                        {
                            // the saga is blocked. delay the message and try again later.
                            _log.Info($"The saga {handlerType} for key {messageContext.SagaKey} is blocked. The current message will be delayed and retried.");
                            _transactionContext.RollbackToSavepoint(savepointName);
                            await _queueReader.DelayMessage(messageContext, 250);
                            _counters.SagaBlocked();
                            return true;
                        }

                        if (messageContext.SagaData == null && !IsSagaStartHandler(handlerType, messageType))
                        {
                            // the saga was not locked, and it doesn't exist, and this message doesn't start a saga.
                            // we are processing a saga message but it is not a saga start message and we didnt read previous
                            // saga data from the DB. This means we are processing a non-start messge before the saga is started.
                            // we could continute but that might be bad. Its probably better to stop and draw attention to a probable bug in the saga or message order.
                            throw new ApplicationException($"A Message of Type {messageType} is being processed, but the saga {handlerType} has not been started for key {messageContext.SagaKey}. An IHandleSagaStartMessage<> handler on the saga must be processed first to start the saga.");
                        }
                        
                    }

                    // find the right method on the handler.
                    var parameterTypes = new[] { typeof(MessageContext), messageType };
                    var handleMethod = handler.GetType().GetMethod("Handle", parameterTypes);


                    // Invoke and await the method.
                    // should it have a seperate try-catch around this and treat it differently?
                    // that would allow us to tell the difference between a problem in a handler, or if the problem was in the bus code.
                    // does that mater for the retry?
                    {
                        var taskObject = handleMethod.Invoke(handler, new object[] { messageContext, messageContext.Message });
                        var castTask = taskObject as Task;
                        await castTask;
                    }

                    if (handlerIsSaga)
                    {
                        await _queueReader.PersistSagaData(handler, messageContext);
                        _log.Debug($"Inactive Saga {messageContext.SagaKey}");
                    }
                }

                foreach(var csm in messageContext.SentMessages)
                {
                    await _queueWriter.WriteMessage(csm.QueueName, csm.Type, csm.Message, csm.NotBefore);
                }

                // if nothing threw an exception, we can mark the message as processed.
                await _queueReader.CompleteMessage(messageContext);
                // return true so the transaction commits and the main loop looks for another mesage right away.
                return true;
            }
            catch (Exception ex)
            {
                // there was an exception, Rollback to the save point to undo
                // any db changes done by the handlers.
                _log.Warn($"There was an execption processing the message. {ex}");
                _transactionContext.RollbackToSavepoint(savepointName);
                // increment the retry count, (or maybe even fail the message)
                await _queueReader.FailMessage(messageContext, ex);
                // return true so the transaction commits and the main loop looks for another mesage right away.
                return true;
            }
            finally
            {
                _counters.FinishMessage(started);
            }

        }

        private bool IsSagaStartHandler(Type handlerType, Type messageType)
        {
            var handlerInterfaces = handlerType.GetInterfaces();
            var handleSagaStartMessageInterface = typeof(IHandleSagaStartMessage<>).MakeGenericType(messageType);
            return handlerInterfaces.Any(i => i == handleSagaStartMessageInterface);
        }

        /// <summary>
        /// Returns True if the Type inherits Saga<>.
        /// </summary>
        /// <param name="toCheck"></param>
        /// <returns></returns>
        private static bool IsSubclassOfSaga(Type toCheck)
        {
            while (toCheck != null && toCheck != typeof(object))
            {
                var cur = toCheck.IsGenericType ? toCheck.GetGenericTypeDefinition() : toCheck;
                if (typeof(Saga<>) == cur)
                {
                    return true;
                }
                toCheck = toCheck.BaseType;
            }
            return false;
        }
    }
}
