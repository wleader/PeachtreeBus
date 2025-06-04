using PeachtreeBus.Queues;

namespace PeachtreeBus.SimpleInjector
{
    public class FailedQueueMessageHandlerFactory(
        IWrappedScope scope)
        : IFailedQueueMessageHandlerFactory
    {
        private readonly IWrappedScope _scope = scope;

        public IHandleFailedQueueMessages GetHandler()
        {
            return _scope.GetInstance<IHandleFailedQueueMessages>();
        }
    }
}
