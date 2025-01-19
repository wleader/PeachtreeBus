﻿using System;

namespace PeachtreeBus.Model
{
    /// <summary>
    /// Represents a row in a subscription messages table.
    /// </summary>
    public class SubscribedMessage : QueueMessage
    {
        /// <summary>
        /// The subscriber the message has been published for.
        /// </summary>
        public virtual Guid SubscriberId { get; set; }

        /// <summary>
        /// The time at which the message is considered abandonded
        /// and may be deleted.
        /// </summary>
        public virtual UtcDateTime ValidUntil { get; set; }
    }
}
