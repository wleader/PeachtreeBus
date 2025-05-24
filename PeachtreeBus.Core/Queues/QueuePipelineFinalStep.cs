using Microsoft.Extensions.Logging;
using PeachtreeBus.ClassNames;
using PeachtreeBus.Exceptions;
using PeachtreeBus.Pipelines;
using PeachtreeBus.Sagas;
using PeachtreeBus.Telemetry;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace PeachtreeBus.Queues
{
    /// <summary>
    /// A Pipeline step that passed the message off to all the registered handlers.
    /// Intended to be the final link in the pipline chain.
    /// </summary>
    public interface IQueuePipelineFinalStep : IPipelineFinalStep<IQueueContext>;

    /// <summary>
    /// A Pipeline step that passed the message off to all the registered handlers.
    /// Intended to be the final link in the pipline chain.
    /// </summary>
    public class QueuePipelineFinalStep(
        IFindQueueHandlers findHandlers,
        ILogger<QueuePipelineFinalStep> log,
        ISagaMessageMapManager sagaMessageMapManager,
        IQueueReader queueReader,
        IClassNameService classNameService)
        : PipelineFinalStep<IQueueContext>
        , IQueuePipelineFinalStep
    {
        private readonly IFindQueueHandlers _findHandlers = findHandlers;
        private readonly ILogger<QueuePipelineFinalStep> _log = log;
        private readonly ISagaMessageMapManager _sagaMessageMapManager = sagaMessageMapManager;
        private readonly IQueueReader _queueReader = queueReader;
        private readonly IClassNameService _classNameService = classNameService;

        public override async Task Invoke(IQueueContext context, Func<IQueueContext, Task>? next)
        {
            // determine what type of message it is.
            var messageType = _classNameService.GetTypeForClassName(context.MessageClass)
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
                await InvokeHandler(context, messageType, handler);
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

        private async Task InvokeHandler(IQueueContext context, Type messageType, object handler)
        {
            using var activity = new HandlerActivity(handler.GetType(), context);

            // determine if this handler is a saga.
            var handlerType = handler.GetType();
            var queueContext = (QueueContext)context;

            queueContext.CurrentHandler = handlerType.FullName ?? handlerType.ToString();

            var handlerIsSaga = handlerType.IsSubclassOfSaga();
            if (handlerIsSaga)
            {
                queueContext.SagaKey = _sagaMessageMapManager.GetKey(handler, context.Message);
                _log.LoadingSaga(queueContext.CurrentHandler, context.SagaKey);

                await _queueReader.LoadSaga(handler, queueContext);

                // if the saga is blocked. Stop.
                if (queueContext.SagaBlocked)
                {
                    activity.SagaBlocked();
                    return;
                }

                if (queueContext.SagaData == null && !handlerType.IsSagaStartHandler(messageType))
                {
                    // Its possible that a message was sent when the saga was still active,
                    // or as a time delayed message, but by the time the message was handled,
                    // the saga completed. Its actually normal for that to happen, so an exception
                    // would be a false positive. Likewise writing a warning would also be a false
                    // positive. This code is assuming that the user knows that they completed the
                    // saga, or if they send a message before starting, that they will discover 
                    // their error. All we can really do is log something so that its discoverable.
                    _log.SagaNotStarted(queueContext.CurrentHandler, context.SagaKey,
                        context.MessageId);
                    return;
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
            _log.InvokeHandler(context.MessageId, context.MessageClass, queueContext.CurrentHandler);
            {
                var taskObject = handleMethod.Invoke(handler, [context, context.Message]);
                var castTask = UnreachableException.ThrowIfNull(taskObject as Task,
                    message: "IHandleQueueMessage<>.Handle must return a not null Task.");
                await castTask;
            }

            if (handlerIsSaga)
            {
                await _queueReader.SaveSaga(handler, queueContext);
                _log.SagaSaved(queueContext.CurrentHandler, context.SagaKey!);
            }
        }
    }
}
