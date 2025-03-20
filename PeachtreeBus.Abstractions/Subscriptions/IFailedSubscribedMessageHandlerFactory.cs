namespace PeachtreeBus.Subscriptions;

public interface IFailedSubscribedMessageHandlerFactory
{
    IHandleFailedSubscribedMessages GetHandler();
}
