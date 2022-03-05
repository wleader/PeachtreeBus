using System;

namespace PeachtreeBus.Subscriptions
{
    /// <summary>
    /// stores contextual data about the subscription message being handled.
    /// </summary>
    public class SubscribedContext
    {
        /// <summary>
        /// The message as read from the database.
        /// </summary>
        public Model.SubscribedMessage MessageData { get; set; }

        /// <summary>
        /// Headers that were stored with the message.
        /// </summary>
        public Headers Headers { get; set; }

        /// <summary>
        /// The message itself.
        /// </summary>
        public object Message { get; set; }

        /// <summary>
        /// The Subscriber that the message was sent to.
        /// </summary>
        public Guid SubscriberId { get; set; }
    }
}
