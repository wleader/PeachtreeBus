using System;

namespace PeachtreeBus.Subscriptions
{
    /// <summary>
    /// Stores contextual data about the subscription message being handled,
    /// that may be useful to application code.
    /// </summary>
    public class SubscribedContext : BaseContext
    {
        /// <summary>
        /// The Subscriber that the message was sent to.
        /// </summary>
        public Guid SubscriberId { get; set; }
    }

    /// <summary>
    /// Stores contextual data about the subscription message being handled,
    /// that is useful to the library.
    /// </summary>
    public class InternalSubscribedContext : SubscribedContext
    {
        /// <summary>
        /// The message as read from the database.
        /// </summary>
        public Model.SubscribedMessage MessageData { get; set; } = default!;

        /// <summary>
        /// Headers that were stored with the message.
        /// </summary>
        public Headers Headers { get; set; } = new();
    }

}
