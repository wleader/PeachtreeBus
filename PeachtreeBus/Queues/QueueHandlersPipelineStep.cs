using Microsoft.Extensions.Logging;
using PeachtreeBus.Data;
using PeachtreeBus.Pipelines;
using PeachtreeBus.Sagas;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PeachtreeBus.Queues
{
    public class QueueHandlersPipelineStep : IPipelineStep<QueueContext>
    {
        private readonly string _queueName;
        private readonly IFindQueueHandlers _findHandlers;
        private readonly ILogger _log;
        private readonly ISagaMessageMapManager _sagaMessageMapManager;
        private readonly IQueueReader _queueReader;
        private readonly IPerfCounters _counters;
        private readonly IBusDataAccess _dataAccess;
        private readonly string _savepointName;

        public QueueHandlersPipelineStep(string queueName,
            IFindQueueHandlers findHandlers,
            ILogger log,
            ISagaMessageMapManager sagaMessageMapManager,
            IQueueReader queueReader,
            IPerfCounters counters,
            IBusDataAccess dataAccess,
            string savepointName)
        {
            _queueName = queueName;
            _findHandlers = findHandlers;
            _log = log;
            _sagaMessageMapManager = sagaMessageMapManager;
            _queueReader = queueReader;
            _counters = counters;
            _dataAccess = dataAccess;
            _savepointName = savepointName;
        }

        public string CurrentHandlerTypeName { get; set; } = default;
        public bool SagaBlocked { get; set; } = false;

        public async Task Invoke(QueueContext context, Func<QueueContext, Task> next)
        {
            // determine what type of message it is.
            var messageType = Type.GetType(context.Headers.MessageClass);
            if (messageType == null)
            {
                throw new QueueMessageClassNotRecognizedException(
                    context.MessageData.MessageId,
                    _queueName,
                    context.Headers.MessageClass);
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
                throw new QueueMessageNoHandlerException(
                    context.MessageData.MessageId,
                    _queueName,
                    messageType);
            }

            // invoke each of the handlers.
            foreach (var handler in castHandlers)
            {
                // determine if this handler is a saga.
                var handlerType = handler.GetType();
                CurrentHandlerTypeName = handlerType.FullName;

                var handlerIsSaga = handlerType.IsSubclassOfSaga();
                if (handlerIsSaga)
                {
                    context.SagaKey = _sagaMessageMapManager.GetKey(handler, context.Message);
                    _log.QueueWork_LoadingSaga(CurrentHandlerTypeName, context.SagaKey);

                    await _queueReader.LoadSaga(handler, context);

                    if (context.SagaData != null && context.SagaData.Blocked)
                    {
                        // the saga is blocked. delay the message and try again later.
                        _log.QueueWork_SagaBlocked(CurrentHandlerTypeName, context.SagaKey);
                        _dataAccess.RollbackToSavepoint(_savepointName);
                        await _queueReader.DelayMessage(context, 250);
                        _counters.SagaBlocked();
                        SagaBlocked = true;
                        return;
                    }

                    if (context.SagaData == null && !handlerType.IsSagaStartHandler(messageType))
                    {
                        // the saga was not locked, and it doesn't exist, and this message doesn't start a saga.
                        // we are processing a saga message but it is not a saga start message and we didnt read previous
                        // saga data from the DB. This means we are processing a non-start messge before the saga is started.
                        // we could continute but that would mean that the saga handler might not know it needs to initialize
                        // the saga data, so its better to stop and make things get fixed.
                        throw new SagaNotStartedException(context.MessageData.MessageId,
                            _queueName,
                            messageType,
                            handlerType,
                            context.SagaKey);
                    }
                }

                // find the right method on the handler.
                var parameterTypes = new[] { typeof(QueueContext), messageType };
                var handleMethod = handler.GetType().GetMethod("Handle", parameterTypes);


                // Invoke and await the method.
                // should it have a seperate try-catch around this and treat it differently?
                // that would allow us to tell the difference between a problem in a handler, or if the problem was in the bus code.
                // does that mater for the retry?
                _log.QueueWork_InvokeHandler(context.MessageData.MessageId, context.Headers.MessageClass, handlerType.FullName);
                {
                    var taskObject = handleMethod.Invoke(handler, new object[] { context, context.Message });
                    var castTask = taskObject as Task;
                    await castTask;
                }

                if (handlerIsSaga)
                {
                    await _queueReader.SaveSaga(handler, context);
                    _log.QueueWork_SagaSaved(CurrentHandlerTypeName, context.SagaKey);
                }
            }
        }
    }
}
