using PeachtreeBus.Errors;

namespace PeachtreeBus.SimpleInjector
{
    public class FailedSubscribedMessageHandlerFactory : IFailedSubscribedMessageHandlerFactory
    {
        private readonly IWrappedScope _scope;

        public FailedSubscribedMessageHandlerFactory(IWrappedScope scope)
        {
            _scope = scope;
        }

        public IHandleFailedSubscribedMessages GetHandler()
        {
            return _scope.GetInstance<IHandleFailedSubscribedMessages>();
        }
    }
}
