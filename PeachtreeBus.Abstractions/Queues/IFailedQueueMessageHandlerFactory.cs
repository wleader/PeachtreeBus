namespace PeachtreeBus.Queues;

public interface IFailedQueueMessageHandlerFactory
{
    IHandleFailedQueueMessages GetHandler();
}
