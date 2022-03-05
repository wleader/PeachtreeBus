using System.Collections.Generic;

namespace PeachtreeBus.Subscriptions
{
    public interface IFindSubscribedHandlers
    {
        /// <summary>
        /// Gets an enumerable of Message hander classes from the Dependency Injection container.
        /// </summary>
        /// <typeparam name="T">The Type of the message.</typeparam>
        /// <returns>An Enumberable of message handlers for the given message type.</returns>
        IEnumerable<IHandleSubscribedMessage<T>> FindHandlers<T>() where T : ISubscribedMessage;
    }
}
