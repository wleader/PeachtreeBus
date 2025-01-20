using Microsoft.Extensions.Logging;
using PeachtreeBus.Pipelines;
using PeachtreeBus.Sagas;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading.Tasks;

namespace PeachtreeBus.Queues
{
    /// <summary>
    /// A Pipeline step that passed the message off to all the registered handlers.
    /// Intended to be the final link in the pipline chain.
    /// </summary>
    public interface IQueueHandlersPipelineStep : IPipelineStep<QueueContext> { }

    /// <summary>
    /// A Pipeline step that passed the message off to all the registered handlers.
    /// Intended to be the final link in the pipline chain.
    /// </summary>
    public class QueueHandlersPipelineStep(
        IFindQueueHandlers findHandlers,
        ILogger<QueueHandlersPipelineStep> log,
        ISagaMessageMapManager sagaMessageMapManager,
        IQueueReader queueReader)
        : IQueueHandlersPipelineStep
    {
        private readonly IFindQueueHandlers _findHandlers = findHandlers;
        private readonly ILogger<QueueHandlersPipelineStep> _log = log;
        private readonly ISagaMessageMapManager _sagaMessageMapManager = sagaMessageMapManager;
        private readonly IQueueReader _queueReader = queueReader;

        // This property isn't used as the handlers step is always last in the pipeline
        // but it is requred by the interface.
        [ExcludeFromCodeCoverage]
        public int Priority { get => 0; }

        public async Task Invoke(QueueContext externalContext, Func<QueueContext, Task>? next)
        {
            var context = (InternalQueueContext)externalContext;

            // determine what type of message it is.
            var messageType = Type.GetType(context.Headers.MessageClass)
                ?? throw new QueueMessageClassNotRecognizedException(
                    context.MessageData.MessageId,
                    context.SourceQueue,
                    context.Headers.MessageClass);

            // check that messageType is IQueueMessage
            // otherwise the MakeGenericMethod call below will throw an nasty exception
            // that won't make sense to someone debugging.
            TypeIsNotIQueueMessageException.ThrowIfMissingInterface(messageType);

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
                    context.SourceQueue,
                    messageType);
            }

            // invoke each of the handlers.
            foreach (var handler in castHandlers)
            {
                // determine if this handler is a saga.
                var handlerType = handler.GetType();
                context.CurrentHandler = handlerType.FullName;

                var handlerIsSaga = handlerType.IsSubclassOfSaga();
                if (handlerIsSaga)
                {
                    context.SagaKey = _sagaMessageMapManager.GetKey(handler, context.Message);
                    _log.QueueWork_LoadingSaga(context.CurrentHandler, context.SagaKey);

                    await _queueReader.LoadSaga(handler, context);

                    // if the saga is blocked. Stop.
                    if (context.SagaBlocked) return;

                    if (context.SagaData == null && !handlerType.IsSagaStartHandler(messageType))
                    {
                        // the saga was not locked, and it doesn't exist, and this message doesn't start a saga.
                        // we are processing a saga message but it is not a saga start message and we didnt read previous
                        // saga data from the DB. This means we are processing a non-start messge before the saga is started.
                        // we could continute but that would mean that the saga handler might not know it needs to initialize
                        // the saga data, so its better to stop and make things get fixed.
                        throw new SagaNotStartedException(context.MessageData.MessageId,
                            context.SourceQueue,
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
                    var taskObject = handleMethod.Invoke(handler, [context, context.Message]);
                    var castTask = (Task)taskObject;
                    await castTask;
                }

                if (handlerIsSaga)
                {
                    await _queueReader.SaveSaga(handler, context);
                    _log.QueueWork_SagaSaved(context.CurrentHandler, context.SagaKey!);
                }
            }
        }
    }
}
