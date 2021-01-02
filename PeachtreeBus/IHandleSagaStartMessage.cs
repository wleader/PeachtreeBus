namespace PeachtreeBus
{
    /// <summary>
    /// Defines an interface for a class that handles a message that is allowed to start a saga.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public interface IHandleSagaStartMessage<T> : IHandleMessage<T> where T : IMessage
    {

    }

}
