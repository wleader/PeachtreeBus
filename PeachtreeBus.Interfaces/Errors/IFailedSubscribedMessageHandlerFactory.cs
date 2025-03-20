namespace PeachtreeBus.Errors;

public interface IFailedSubscribedMessageHandlerFactory
{
    IHandleFailedSubscribedMessages GetHandler();
}
