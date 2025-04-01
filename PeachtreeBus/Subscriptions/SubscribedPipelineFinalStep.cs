using PeachtreeBus.Exceptions;
using PeachtreeBus.Pipelines;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace PeachtreeBus.Subscriptions
{
    public interface ISubscribedPipelineFinalStep : IPipelineFinalStep<ISubscribedContext> { }

    public class SubscribedPipelineFinalStep(
        IFindSubscribedHandlers findHandlers)
        : PipelineFinalStep<ISubscribedContext>
        , ISubscribedPipelineFinalStep
    {
        private readonly IFindSubscribedHandlers _findHandlers = findHandlers;

        public override async Task Invoke(ISubscribedContext subscribedcontext, Func<ISubscribedContext, Task>? next)
        {
            var context = (SubscribedContext)subscribedcontext;

            // determine what type of message it is.
            var messageType = Type.GetType(context.MessageClass)
                ?? throw new SubscribedMessageClassNotRecognizedException(context.Data.MessageId,
                    context.SubscriberId,
                    context.MessageClass);

            // check that messageType is ISubscribed message, otherwise
            // MakeGenericMethod will throw a nasty hard to debug exception.
            TypeIsNotISubscribedMessageException.ThrowIfMissingInterface(messageType);

            // Get the message handlers for this message type from the Dependency Injection container.
            // the list will contain both regular handlers and sagas.
            // if a message has mulitple handlers, we'll get multiple handlers.
            var method = typeof(IFindSubscribedHandlers).GetMethod("FindHandlers");
            method = UnreachableException.ThrowIfNull(method,
                message: " IFindSubscribedHandlers must have a FindHandlers method.");

            var genericMethod = method.MakeGenericMethod(messageType);
            var handlers = genericMethod.Invoke(_findHandlers, null);

            if (handlers is not IEnumerable<object> castHandlers)
                throw new IncorrectImplementationException("The FindHandlers method should not return null.",
                    _findHandlers.GetType(), typeof(IFindSubscribedHandlers));

            int handlerCount = 0;
            // invoke each of the handlers.
            foreach (var handler in castHandlers)
            {
                handlerCount++;

                // find the right method on the handler.
                Type[] parameterTypes = [typeof(ISubscribedContext), messageType];
                var handleMethod = handler.GetType().GetMethod("Handle", parameterTypes);
                // A handler must have a Handle method, so GetMethod should never ruturn null
                // unless the interface changed.
                handleMethod = UnreachableException.ThrowIfNull(handleMethod,
                    message: "IHandleSubscribedMessage<> must have a Handle method.");

                // Invoke and await the method.
                // should it have a seperate try-catch around this and treat it differently?
                // that would allow us to tell the difference between a problem in a handler, or if the problem was in the bus code.
                // does that mater for the retry?
                {
                    var taskObject = handleMethod.Invoke(handler, [context, context.Message]);
                    var castTask = taskObject as Task;
                    // The handle method must return a task.
                    castTask = IncorrectImplementationException.ThrowIfNull(castTask,
                        handler.GetType(),
                        typeof(IHandleSubscribedMessage<>),
                        message: "Handle must return a not null Task.");
                    await castTask;
                }
            }

            if (handlerCount < 1)
            {
                // sanity check that the Depenency Injection container found at least one handler.
                // we shouldn't process a message that has no handlers.
                throw new SubscribedMessageNoHandlerException(context.Data.MessageId,
                    context.SubscriberId,
                    messageType);
            }
        }
    }
}
