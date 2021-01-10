using System;

namespace PeachtreeBus
{
    /// <summary>
    /// Holds a message sent from a handler for the message context.
    /// </summary>
    public class ContextSentMessage
    {
        /// <summary>
        /// The type of the message.
        /// </summary>
        public Type Type { get; set; }
        
        /// <summary>
        /// The message itself.
        /// </summary>
        public object Message { get; set; }

        /// <summary>
        /// What Queue the message is destined for.
        /// </summary>
        public string QueueName { get; set; }

        /// <summary>
        /// The time to wait until to processs the message.
        /// </summary>
        public DateTime? NotBefore { get; set; }
    }
}
