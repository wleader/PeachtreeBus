using PeachtreeBus.Pipelines;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PeachtreeBus.Subscriptions
{
    public class SubscribedHandlersPipelineStep : IPipelineStep<SubscribedContext>
    {
        private readonly IFindSubscribedHandlers _findHandlers;

        public SubscribedHandlersPipelineStep(IFindSubscribedHandlers findHandlers)
        {
            _findHandlers = findHandlers;
        }

        public async Task Invoke(SubscribedContext subscribedcontext, Func<SubscribedContext, Task> next)
        {
            var context = (InternalSubscribedContext)subscribedcontext;

            // determine what type of message it is.
            var messageType = Type.GetType(context.Headers.MessageClass);
            if (messageType == null)
            {
                throw new SubscribedMessageClassNotRecognizedException(context.MessageData.MessageId,
                    context.SubscriberId,
                    context.Headers.MessageClass);
            }

            // check that messageType is ISubscribed message, otherwise
            // MakeGenericMethod will throw a nasty hard to debug exception.
            if (!typeof(ISubscribedMessage).IsAssignableFrom(messageType))
                throw new MissingInterfaceException(messageType, typeof(ISubscribedMessage));

            // Get the message handlers for this message type from the Dependency Injection container.
            // the list will contain both regular handlers and sagas.
            // if a message has mulitple handlers, we'll get multiple handlers.
            var method = typeof(IFindSubscribedHandlers).GetMethod("FindHandlers");
            var genericMethod = method.MakeGenericMethod(messageType);
            var handlers = genericMethod.Invoke(_findHandlers, null);
            var castHandlers = (handlers as IEnumerable<object>).ToArray();

            // sanity check that the Depenency Injection container found at least one handler.
            // we shouldn't process a message that has no handlers.
            if (castHandlers.Length < 1)
            {
                throw new SubscribedMessageNoHandlerException(context.MessageData.MessageId,
                    context.SubscriberId,
                    messageType);
            }

            // invoke each of the handlers.
            foreach (var handler in castHandlers)
            {
                // find the right method on the handler.
                var parameterTypes = new[] { typeof(SubscribedContext), messageType };
                var handleMethod = handler.GetType().GetMethod("Handle", parameterTypes);

                // Invoke and await the method.
                // should it have a seperate try-catch around this and treat it differently?
                // that would allow us to tell the difference between a problem in a handler, or if the problem was in the bus code.
                // does that mater for the retry?
                {
                    var taskObject = handleMethod.Invoke(handler, new object[] { context, context.Message });
                    var castTask = taskObject as Task;
                    await castTask;
                }
            }
        }
    }
}
