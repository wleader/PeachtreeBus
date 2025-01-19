using PeachtreeBus.Sagas;

namespace PeachtreeBus.Queues
{
    /// <summary>
    /// Holds information about the mesage currently being processed.
    /// Exposes information about the context that the application code
    /// may want to use.
    /// </summary>
    public class QueueContext : BaseContext
    {
        /// <summary>
        /// Which Queue the message was read from.
        /// </summary>
        public QueueName SourceQueue { get; set; } = default!;
    }

    /// <summary>
    /// Extends QueueContext to track information that 
    /// PeachtreeBus cares about, but that application code
    /// shouldn't need to interact with.
    /// </summary>
    public class InternalQueueContext : QueueContext
    {
        /// <summary>
        /// The Model of the message as was stored the database.
        /// </summary>
        public QueueMessage MessageData { get; set; } = default!;

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
        public string SagaKey { get; set; } = string.Empty;

        /// <summary>
        /// What message handler was last used on the message.
        /// Used to generate exceptions and logs that point to specific
        /// handlers.
        /// </summary>
        public string? CurrentHandler { get; set; } = default;

        /// <summary>
        /// Indicates if the SagaData row was in the database was locked
        /// so that the bus can reschedule the message to run again later
        /// when the sage is not already running.
        /// </summary>
        public bool SagaBlocked { get => SagaData?.Blocked ?? false; }
    }
}
