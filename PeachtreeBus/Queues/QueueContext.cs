using PeachtreeBus.Sagas;

namespace PeachtreeBus.Queues;

public interface IQueueContext : IIncomingContext
{
    public QueueName SourceQueue { get; }
    internal string CurrentHandler { get; set; }
    public SagaKey SagaKey { get; internal set; }
    internal bool SagaBlocked { get; }
    internal SagaData? SagaData { get; set; }
}

/// <summary>
/// Holds information about the mesage currently being processed.
/// Exposes information about the context that the application code
/// may want to use.
/// </summary>
public class QueueContext : IncomingContext<QueueData>, IQueueContext
{
    /// <summary>
    /// Which Queue the message was read from.
    /// </summary>
    public QueueName SourceQueue { get; set; } = default!;

    /// <summary>
    /// The Model of the saga data related to the message (Null when the message is not part of a saga)
    /// Will be null if the saga is starting and has never persisted to the DB before.
    /// Will be null if the row is locked.
    /// </summary>
    public SagaData? SagaData { get; set; }

    /// <summary>
    /// The Saga instance Key for the messge.
    /// (Null when the message is not part of a saga.
    /// </summary>
    public SagaKey SagaKey { get; set; }

    /// <summary>
    /// What message handler was last used on the message.
    /// Used to generate exceptions and logs that point to specific
    /// handlers.
    /// </summary>
    public string CurrentHandler { get; set; } = string.Empty;

    /// <summary>
    /// Indicates if the SagaData row was in the database was locked
    /// so that the bus can reschedule the message to run again later
    /// when the sage is not already running.
    /// </summary>
    public bool SagaBlocked { get => SagaData?.Blocked ?? false; }
}
