using PeachtreeBus.Errors;
using SimpleInjector;

namespace PeachtreeBus.SimpleInjector
{
    public static partial class SimpleInjectorExtensions
    {
        public static Container UsePeachtreeBusFailedQueueMessageHandler<TQueueErrorHandler>(this Container container)
            where TQueueErrorHandler : IHandleFailedQueueMessages
        {
            container.Register(typeof(IHandleFailedQueueMessages), typeof(TQueueErrorHandler), Lifestyle.Scoped);
            return container;
        }

        public static Container UsePeachtreeBusFailedSubscribedMessageHandler<TSubscribedErrorHandler>(this Container container)
            where TSubscribedErrorHandler : IHandleFailedSubscribedMessages
        {
            container.Register(typeof(IHandleFailedSubscribedMessages), typeof(TSubscribedErrorHandler), Lifestyle.Scoped);
            return container;
        }

        public static Container UsePeachtreeBusDefaultErrorHandlers(this Container container)
        {
            return container
                .UsePeachtreeBusFailedQueueMessageHandler<DefaultFailedQueueMessageHandler>()
                .UsePeachtreeBusFailedSubscribedMessageHandler<DefaultFailedSubscribedMessageHandler>();
        }
    }
}
