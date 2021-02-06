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
        Task<QueueMessage> GetOneQueueMessage(string queueName);

        /// <summary>
        /// Inserts a new message into the database.
        /// </summary>
        /// <param name="message">The message to insert.</param>
        Task<long> EnqueueMessage(QueueMessage message, string queueName);

        /// <summary>
        /// Inserts the Message into the completed table, and removes it from the queue table.
        /// </summary>
        /// <param name="message">The message to move.</param>
        Task CompleteMessage(QueueMessage message, string queueName);

        /// <summary>
        /// Inserts the message into the error table, and removes if from the queue table.
        /// </summary>
        /// <param name="message">The message to move.</param>
        Task FailMessage(QueueMessage message, string queueName);

        /// <summary>
        /// Updates a message.
        /// Only updates message properties that are allowed to change.
        /// </summary>
        /// <param name="message"></param>
        Task Update(QueueMessage message, string queueName);

        /// <summary>
        /// Inserts Saga Data into the database.
        /// </summary>
        /// <param name="data">The saga data to insert.</param>
        Task<long> Insert(SagaData data, string sagaName);

        /// <summary>
        /// Updates the saga data in the database.
        /// </summary>
        /// <param name="data">The Data to update. Only updates properties that are allowed to change.</param>
        Task Update(SagaData data, string sagaName);

        /// <summary>
        /// Reads saga data from the database.
        /// </summary>
        /// <param name="className">The saga's class name.</param>
        /// <param name="key">The saga's key (used to differentiate multiple instances of the same saga.)</param>
        /// <returns>Matching saga data.</returns>
        Task<SagaData> GetSagaData(string sagaName, string key);

        /// <summary>
        /// Deletes data for completed sagas.
        /// </summary>
        /// <param name="className">The saga's class name.</param>
        /// <param name="key">The saga's key (used to differentiate multiple instances of the same saga.)</param>
        Task DeleteSagaData(string sagaName, string key);
    }
}
