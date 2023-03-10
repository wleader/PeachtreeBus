namespace PeachtreeBus.Errors
{
    public interface IFailedQueueMessageHandlerFactory
    {
        IHandleFailedQueueMessages GetHandler();
    }
}
