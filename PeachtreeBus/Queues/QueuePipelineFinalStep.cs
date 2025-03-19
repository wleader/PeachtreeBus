using Microsoft.Extensions.Logging;
using PeachtreeBus.Exceptions;
using PeachtreeBus.Pipelines;
using PeachtreeBus.Sagas;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;

namespace PeachtreeBus.Queues
{
    /// <summary>
    /// A Pipeline step that passed the message off to all the registered handlers.
    /// Intended to be the final link in the pipline chain.
    /// </summary>
    public interface IQueuePipelineFinalStep : IPipelineFinalStep<QueueContext, IQueueContext>;

    /// <summary>
    /// A Pipeline step that passed the message off to all the registered handlers.
    /// Intended to be the final link in the pipline chain.
    /// </summary>
    public class QueuePipelineFinalStep(
        IFindQueueHandlers findHandlers,
        ILogger<QueuePipelineFinalStep> log,
        ISagaMessageMapManager sagaMessageMapManager,
        IQueueReader queueReader)
        : IQueuePipelineFinalStep
    {
        private readonly IFindQueueHandlers _findHandlers = findHandlers;
        private readonly ILogger<QueuePipelineFinalStep> _log = log;
        private readonly ISagaMessageMapManager _sagaMessageMapManager = sagaMessageMapManager;
        private readonly IQueueReader _queueReader = queueReader;

        // This property isn't used as the handlers step is always last in the pipeline
        // but it is requred by the interface.
        [ExcludeFromCodeCoverage]
        public int Priority { get => 0; }

        public QueueContext InternalContext { get; set; } = default!;

        public async Task Invoke(IQueueContext context, Func<IQueueContext, Task>? next)
        {
            // determine what type of message it is.
            var messageType = Type.GetType(context.MessageClass)
                ?? throw new QueueMessageClassNotRecognizedException(
                    context.MessageId,
                    context.SourceQueue,
                    context.MessageClass);

            // check that messageType is IQueueMessage
            // otherwise the MakeGenericMethod call below will throw an nasty exception
            // that won't make sense to someone debugging.
            TypeIsNotIQueueMessageException.ThrowIfMissingInterface(messageType);

            // Get the message handlers for this message type from the Dependency Injection container.
            // the list will contain both regular handlers and sagas.
            // if a message has mulitple handlers, we'll get multiple handlers.
            var method = typeof(IFindQueueHandlers).GetMethod(nameof(IFindQueueHandlers.FindHandlers));
            var genericMethod = method!.MakeGenericMethod(messageType);
            var handlers = genericMethod.Invoke(_findHandlers, null);

            if (handlers is not IEnumerable<object> castHandlers)
                throw new IncorrectImplementationException("The FindHandlers method should not return null.",
                    _findHandlers.GetType(), typeof(IFindQueueHandlers));

            int handlerCount = 0;
            // invoke each of the handlers.
            foreach (var handler in castHandlers)
            {
                handlerCount++;

                // determine if this handler is a saga.
                var handlerType = handler.GetType();
                InternalContext.CurrentHandler = handlerType.FullName ?? handlerType.ToString();

                var handlerIsSaga = handlerType.IsSubclassOfSaga();
                if (handlerIsSaga)
                {
                    InternalContext.SagaKey = _sagaMessageMapManager.GetKey(handler, context.Message);
                    _log.QueueWork_LoadingSaga(InternalContext.CurrentHandler, context.SagaKey);

                    await _queueReader.LoadSaga(handler, InternalContext);

                    // if the saga is blocked. Stop.
                    if (InternalContext.SagaBlocked) return;

                    if (InternalContext.SagaData == null && !handlerType.IsSagaStartHandler(messageType))
                    {
                        // the saga was not locked, and it doesn't exist, and this message doesn't start a saga.
                        // we are processing a saga message but it is not a saga start message and we didnt read previous
                        // saga data from the DB. This means we are processing a non-start messge before the saga is started.
                        // we could continute but that would mean that the saga handler might not know it needs to initialize
                        // the saga data, so its better to stop and make things get fixed.
                        throw new SagaNotStartedException(context.MessageId,
                            context.SourceQueue,
                            messageType,
                            handlerType,
                            context.SagaKey);
                    }
                }

                // find the right method on the handler.
                Type[] parameterTypes = [typeof(IQueueContext), messageType];
                var handleMethod = UnreachableException.ThrowIfNull(handler.GetType().GetMethod("Handle", parameterTypes),
                    message: "IHandleQueueMessage<> must have a Handle method.");

                // Invoke and await the method.
                // should it have a seperate try-catch around this and treat it differently?
                // that would allow us to tell the difference between a problem in a handler, or if the problem was in the bus code.
                // does that mater for the retry?
                _log.QueueWork_InvokeHandler(context.MessageId, context.MessageClass, InternalContext.CurrentHandler);
                {
                    var taskObject = handleMethod.Invoke(handler, [context, context.Message]);
                    var castTask = UnreachableException.ThrowIfNull(taskObject as Task,
                        message: "IHandleQueueMessage<>.Handle must return a not null Task.");
                    await castTask;
                }

                if (handlerIsSaga)
                {
                    await _queueReader.SaveSaga(handler, InternalContext);
                    _log.QueueWork_SagaSaved(InternalContext.CurrentHandler, context.SagaKey!);
                }
            }

            if (handlerCount < 1)
            {
                // sanity check that the Depenency Injection container found at least one handler.
                // we shouldn't process a message that has no handlers.
                throw new QueueMessageNoHandlerException(
                    context.MessageId,
                    context.SourceQueue,
                    messageType);
            }
        }
    }
}
