using System.Collections.Generic;

namespace PeachtreeBus
{

    /// <summary>
    /// Defines an interface that the message processor can use to get get
    /// Message Handlers from the Depenency Injection Container.
    /// </summary>
    public interface IFindMessageHandlers
    {
        IEnumerable<IHandleMessage<T>> FindHandlers<T>() where T : IMessage;
    }
}
