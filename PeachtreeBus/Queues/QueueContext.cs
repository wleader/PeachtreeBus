using PeachtreeBus.Sagas;

namespace PeachtreeBus.Queues;

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
    /// The Saga instance Key for the messge.
    /// (Null when the message is not part of a saga.
    /// </summary>
    public SagaKey SagaKey { get; set; }

    /// <summary>
    /// Keeps track of which handler for a message is being invoked.
    /// Useful mostly to report which handler had a problem.
    /// </summary>
    public string? CurrentHandler { get; set; }

    /// <summary>
    /// Holds the saga data when then message handler is a saga.
    /// </summary>
    public SagaData? SagaData { get; set; }

    /// <summary>
    /// Improves readability
    /// </summary>
    public bool SagaBlocked { get => SagaData?.Blocked ?? false; }
}
