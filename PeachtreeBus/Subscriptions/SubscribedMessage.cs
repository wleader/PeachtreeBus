using PeachtreeBus.Data;
using PeachtreeBus.Queues;

namespace PeachtreeBus.Subscriptions
{
    /// <summary>
    /// Represents a row in a subscription messages table.
    /// </summary>
    public class SubscribedMessage : QueueMessage
    {
        /// <summary>
        /// The subscriber the message has been published for.
        /// </summary>
        public required virtual SubscriberId SubscriberId { get; set; }

        /// <summary>
        /// The time at which the message is considered abandonded
        /// and may be deleted.
        /// </summary>
        public required virtual UtcDateTime ValidUntil { get; set; }
    }
}
