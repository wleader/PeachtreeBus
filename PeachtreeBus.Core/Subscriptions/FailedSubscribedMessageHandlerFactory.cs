namespace PeachtreeBus.Subscriptions;

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
