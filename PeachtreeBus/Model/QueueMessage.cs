﻿using System;

namespace PeachtreeBus.Model
{
    /// <summary>
    /// Represents a row in a queue table.
    /// </summary>
    public class QueueMessage
    {
        /// <summary>
        /// Primary Key Identity
        /// </summary>
        public virtual long Id { get; set; }

        /// <summary>
        /// A Uniuque ID. Maybe redundant, but good for logging.
        /// </summary>
        public virtual Guid MessageId { get; set; }

        /// <summary>
        /// Set to a time in the future to delay processing of the message.
        /// </summary>
        public virtual DateTime NotBefore { get; set; }

        /// <summary>
        /// When the message was enqueued.
        /// </summary>
        public virtual DateTime Enqueued { get; set; }

        /// <summary>
        /// When the message was successfully processed.
        /// </summary>
        public virtual DateTime? Completed { get; set; }

        /// <summary>
        /// When the message exceeded its retry limit.
        /// </summary>
        public virtual DateTime? Failed { get; set; }

        /// <summary>
        /// How many times previously has the message been attempted and failed.
        /// </summary>
        public virtual byte Retries { get; set; }

        /// <summary>
        /// Serialized Message Headers
        /// </summary>
        public virtual string Headers { get; set; } = string.Empty;

        /// <summary>
        /// Serialized Message Body
        /// </summary>
        public virtual string Body { get; set; } = string.Empty;
    }
}
