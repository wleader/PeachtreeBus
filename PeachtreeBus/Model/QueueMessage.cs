using System;

namespace PeachtreeBus.Model;

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
    /// Used to prioritize which messages are processed first.
    /// Higher numbers are are processed before lower numbers.
    /// </summary>
    public virtual int Priority { get; set; }

    /// <summary>
    /// Set to a time in the future to delay processing of the message.
    /// </summary>
    public virtual UtcDateTime NotBefore { get; set; }

    /// <summary>
    /// When the message was enqueued.
    /// </summary>
    public virtual UtcDateTime Enqueued { get; set; }

    /// <summary>
    /// When the message was successfully processed.
    /// </summary>
    public virtual UtcDateTime? Completed { get; set; }

    /// <summary>
    /// When the message exceeded its retry limit.
    /// </summary>
    public virtual UtcDateTime? Failed { get; set; }

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
