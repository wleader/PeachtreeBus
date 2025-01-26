using PeachtreeBus.Queues;
using PeachtreeBus.Sagas;
using PeachtreeBus.Subscriptions;
using System.Collections.Generic;
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
        /// Kills the existing transaction and disconnects and reconnects
        /// with the DB Server.
        /// </summary>
        void Reconnect();

        /// <summary>
        /// Gets one message, locking it for update.
        /// Skips locked messages.
        /// </summary>
        /// <param name="queueId">Which message queue to get the message from.</param>
        /// <returns></returns>
        Task<QueueMessage?> GetPendingQueued(QueueName queueName);

        /// <summary>
        /// Inserts a new message into the database.
        /// </summary>
        /// <param name="message">The message to insert.</param>
        Task<Identity> AddMessage(QueueMessage message, QueueName queueName);

        /// <summary>
        /// Inserts the Message into the completed table, and removes it from the queue table.
        /// </summary>
        /// <param name="message">The message to move.</param>
        Task CompleteMessage(QueueMessage message, QueueName queueName);

        /// <summary>
        /// Inserts the message into the error table, and removes if from the queue table.
        /// </summary>
        /// <param name="message">The message to move.</param>
        Task FailMessage(QueueMessage message, QueueName queueName);

        /// <summary>
        /// Updates a message.
        /// Only updates message properties that are allowed to change.
        /// </summary>
        /// <param name="message"></param>
        Task Update(QueueMessage message, QueueName queueName);

        /// <summary>
        /// Inserts Saga Data into the database.
        /// </summary>
        /// <param name="data">The saga data to insert.</param>
        Task<Identity> Insert(SagaData data, SagaName sagaName);

        /// <summary>
        /// Updates the saga data in the database.
        /// </summary>
        /// <param name="data">The Data to update. Only updates properties that are allowed to change.</param>
        Task Update(SagaData data, SagaName sagaName);

        /// <summary>
        /// Reads saga data from the database.
        /// </summary>
        /// <param name="className">The saga's class name.</param>
        /// <param name="key">The saga's key (used to differentiate multiple instances of the same saga.)</param>
        /// <returns>Matching saga data.</returns>
        Task<SagaData?> GetSagaData(SagaName sagaName, SagaKey key);

        /// <summary>
        /// Deletes data for completed sagas.
        /// </summary>
        /// <param name="className">The saga's class name.</param>
        /// <param name="key">The saga's key (used to differentiate multiple instances of the same saga.)</param>
        Task DeleteSagaData(SagaName sagaName, SagaKey key);

        /// <summary>
        /// Deletes Expired Subscriptions
        /// </summary>
        /// <returns></returns>
        Task ExpireSubscriptions();

        /// <summary>
        /// Adds or updates subscriptions.
        /// </summary>
        /// <param name="SubscriberId">Which subscriber is subscribing.</param>
        /// <param name="Category">What category of messages the subscriber wants.</param>
        /// <param name="until">After what time is the subscription no longer valid.</param>
        /// <returns></returns>
        Task Subscribe(SubscriberId SubscriberId, Category Category, UtcDateTime until);

        /// <summary>
        /// Gets one message, locking it for update.
        /// Skips locked messages.
        /// </summary>
        /// <param name="queueId">Which message queue to get the message from.</param>
        /// <returns></returns>
        Task<SubscribedMessage?> GetPendingSubscribed(SubscriberId subscriberId);

        /// <summary>
        /// Inserts a new message into the database.
        /// </summary>
        /// <param name="message">The message to insert.</param>
        Task<Identity> AddMessage(SubscribedMessage message);

        /// <summary>
        /// Inserts the Message into the completed table, and removes it from the queue table.
        /// </summary>
        /// <param name="message">The message to move.</param>
        Task CompleteMessage(SubscribedMessage message);

        /// <summary>
        /// Inserts the message into the error table, and removes if from the queue table.
        /// </summary>
        /// <param name="message">The message to move.</param>
        Task FailMessage(SubscribedMessage message);

        /// <summary>
        /// Updates a message.
        /// Only updates message properties that are allowed to change.
        /// </summary>
        /// <param name="message"></param>
        Task Update(SubscribedMessage message);

        /// <summary>
        /// Moves Subscription Messages from Pending to Error that are not longer valid
        /// </summary>
        /// <returns></returns>
        Task ExpireSubscriptionMessages();

        /// <summary>
        /// Gets the current subscribers for a category
        /// </summary>
        /// <param name="category"></param>
        /// <returns></returns>
        Task<IEnumerable<SubscriberId>> GetSubscribers(Category category);

        /// <summary>
        /// removes old data from a queue's failed messages.
        /// </summary>
        /// <param name="queueName"></param>
        /// <param name="olderthan"></param>
        /// <param name="maxCount"></param>
        /// <returns></returns>
        Task<long> CleanQueueFailed(QueueName queueName, UtcDateTime olderthan, int maxCount);

        /// <summary>
        /// removes old data from a queues completed messages
        /// </summary>
        /// <param name="queueName"></param>
        /// <param name="olderthan"></param>
        /// <param name="maxCount"></param>
        /// <returns></returns>
        Task<long> CleanQueueCompleted(QueueName queueName, UtcDateTime olderthan, int maxCount);

        /// <summary>
        /// removes old data from subscribed completed messages.
        /// </summary>
        /// <param name="olderthan"></param>
        /// <param name="maxCount"></param>
        /// <returns></returns>
        Task<long> CleanSubscribedCompleted(UtcDateTime olderthan, int maxCount);

        /// <summary>
        /// removes old data from subscribed failed messages.
        /// </summary>
        /// <param name="olderthan"></param>
        /// <param name="maxCount"></param>
        /// <returns></returns>
        Task<long> CleanSubscribedFailed(UtcDateTime olderthan, int maxCount);
    }
}
