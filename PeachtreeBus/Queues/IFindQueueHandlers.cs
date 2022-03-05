using System;
using System.Collections.Generic;

namespace PeachtreeBus.Queues
{
    /// <summary>
    /// Defines an interface that the message processor can use to get get
    /// Message Handlers from the Depenency Injection Container.
    /// </summary>
    public interface IFindQueueHandlers
    {
        /// <summary>
        /// Gets an enumerable of Message hander classes from the Dependency Injection container.
        /// </summary>
        /// <typeparam name="T">The Type of the message.</typeparam>
        /// <returns>An Enumberable of message handlers for the given message type.</returns>
        IEnumerable<IHandleQueueMessage<T>> FindHandlers<T>() where T : IQueueMessage;
    }
}
