using PeachtreeBus.Subscriptions;

namespace PeachtreeBus.SimpleInjector
{
    public class FailedSubscribedMessageHandlerFactory(
        IWrappedScope scope)
        : IFailedSubscribedMessageHandlerFactory
    {
        private readonly IWrappedScope _scope = scope;

        public IHandleFailedSubscribedMessages GetHandler()
        {
            return _scope.GetInstance<IHandleFailedSubscribedMessages>();
        }
    }
}
