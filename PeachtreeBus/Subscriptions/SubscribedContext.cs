using System;

namespace PeachtreeBus.Subscriptions
{
    /// <summary>
    /// Stores contextual data about the subscription message being handled,
    /// that may be useful to application code.
    /// </summary>
    public class SubscribedContext
    {
        /// <summary>
        /// The Subscriber that the message was sent to.
        /// </summary>
        public Guid SubscriberId { get; set; }

        /// <summary>
        /// The message itself.
        /// </summary>
        public object Message { get; set; } = default!;

        /// <summary>
        /// A unique Id for the message.
        /// </summary>
        public Guid MessageId { get; set; }
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
