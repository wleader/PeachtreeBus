using PeachtreeBus.Model;
using System.Threading.Tasks;

namespace PeachtreeBus.Data
{
    /// <summary>
    /// Defines the interface needed by the bus to interact with the
    /// database.
    /// </summary>
    public interface IBusDataAccess
    {
        /// <summary>
        /// Start a Database Transaction.
        /// </summary>
        void BeginTransaction();

        /// <summary>
        /// Commit the current Database Transaction.
        /// </summary>
        void CommitTransaction();

        /// <summary>
        /// Rollboack the current Database Transaction.
        /// </summary>
        void RollbackTransaction();

        /// <summary>
        /// Create A Transaction savepoint.
        /// </summary>
        /// <param name="name">The name of the save point.</param>
        void CreateSavepoint(string name);

        /// <summary>
        /// Rolls back to a previous save point.
        /// </summary>
        /// <param name="name">The save point to roll back to.</param>
        void RollbackToSavepoint(string name);

        /// <summary>
        /// Gets one message, locking it for update.
        /// Skips locked messages.
        /// </summary>
        /// <param name="queueId">Which message queue to get the message from.</param>
        /// <returns></returns>
        QueueMessage GetOneQueueMessage(string queueName);

        /// <summary>
        /// Inserts a new message into the database.
        /// </summary>
        /// <param name="message">The message to insert.</param>
        void Insert(QueueMessage message, string queueName);

        /// <summary>
        /// Updates a message.
        /// Only updates message properties that are allowed to change.
        /// </summary>
        /// <param name="message"></param>
        void Update(QueueMessage message, string queueName);

        /// <summary>
        /// Inserts Saga Data into the database.
        /// </summary>
        /// <param name="data">The saga data to insert.</param>
        void Insert(SagaData data, string sagaName);

        /// <summary>
        /// Updates the saga data in the database.
        /// </summary>
        /// <param name="data">The Data to update. Only updates properties that are allowed to change.</param>
        void Update(SagaData data, string sagaName);

        /// <summary>
        /// Moves completed messages from the QueueMessages table to the CompletedMessages table.
        /// Moves failed messages from the QueueMessagesTable to the ErrorMessages table.
        /// </summary>
        /// <returns>The number of rows cleaned.</returns>
        long CleanQueueMessages(string queueName);

        /// <summary>
        /// Reads saga data from the database.
        /// </summary>
        /// <param name="className">The saga's class name.</param>
        /// <param name="key">The saga's key (used to differentiate multiple instances of the same saga.)</param>
        /// <returns>Matching saga data.</returns>
        SagaData GetSagaData(string sagaName, string key);

        /// <summary>
        /// Deletes data for completed sagas.
        /// </summary>
        /// <param name="className">The saga's class name.</param>
        /// <param name="key">The saga's key (used to differentiate multiple instances of the same saga.)</param>
        long DeleteSagaData(string sagaName, string key);

        /// <summary>
        /// Reports if a saga is locked by another thread/process
        /// </summary>
        /// <param name="sagaName">The saga's class name.</param>
        /// <param name="key">The saga's key (used to differentiate multiple instances of the same saga.)</param>
        /// <returns>True if the row exists and is locked.</returns>
        Task<bool> IsSagaLocked(string sagaName, string key);
    }
}
