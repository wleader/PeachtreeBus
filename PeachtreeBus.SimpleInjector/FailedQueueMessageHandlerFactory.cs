using PeachtreeBus.Errors;

namespace PeachtreeBus.SimpleInjector
{
    public class FailedQueueMessageHandlerFactory : IFailedQueueMessageHandlerFactory
    {
        private readonly IWrappedScope _scope;

        public FailedQueueMessageHandlerFactory(IWrappedScope scope)
        {
            _scope = scope;
        }

        public IHandleFailedQueueMessages GetHandler()
        {
            return _scope.GetInstance<IHandleFailedQueueMessages>();
        }
    }
}
