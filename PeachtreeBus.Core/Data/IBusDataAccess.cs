using PeachtreeBus.Queues;
using PeachtreeBus.Sagas;
using PeachtreeBus.Subscriptions;
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
        Task<QueueData?> GetPendingQueued(QueueName queueName);

        /// <summary>
        /// Attempts to count the number of pending messages in a queue.
        /// </summary>
        /// <param name="queueName"></param>
        /// <returns></returns>
        Task<long> EstimateQueuePending(QueueName queueName);

        /// <summary>
        /// Inserts a new message into the database.
        /// </summary>
        /// <param name="message">The message to insert.</param>
        Task<Identity> AddMessage(QueueData message, QueueName queueName);

        /// <summary>
        /// Inserts the Message into the completed table, and removes it from the queue table.
        /// </summary>
        /// <param name="message">The message to move.</param>
        Task CompleteMessage(QueueData message, QueueName queueName);

        /// <summary>
        /// Inserts the message into the error table, and removes if from the queue table.
        /// </summary>
        /// <param name="message">The message to move.</param>
        Task FailMessage(QueueData message, QueueName queueName);

        /// <summary>
        /// Updates a message.
        /// Only updates message properties that are allowed to change.
        /// </summary>
        /// <param name="message"></param>
        Task UpdateMessage(QueueData message, QueueName queueName);

        /// <summary>
        /// Inserts Saga Data into the database.
        /// </summary>
        /// <param name="data">The saga data to insert.</param>
        Task<Identity> InsertSagaData(SagaData data, SagaName sagaName);

        /// <summary>
        /// Updates the saga data in the database.
        /// </summary>
        /// <param name="data">The Data to update. Only updates properties that are allowed to change.</param>
        Task UpdateSagaData(SagaData data, SagaName sagaName);

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
        Task<long> ExpireSubscriptions(int maxCount);

        /// <summary>
        /// Adds or updates subscriptions.
        /// </summary>
        /// <param name="SubscriberId">Which subscriber is subscribing.</param>
        /// <param name="Topic">What topic of messages the subscriber wants.</param>
        /// <param name="until">After what time is the subscription no longer valid.</param>
        /// <returns></returns>
        Task Subscribe(SubscriberId SubscriberId, Topic Topic, UtcDateTime until);

        /// <summary>
        /// Gets one message, locking it for update.
        /// Skips locked messages.
        /// </summary>
        /// <param name="queueId">Which message queue to get the message from.</param>
        /// <returns></returns>
        Task<SubscribedData?> GetPendingSubscribed(SubscriberId subscriberId);

        /// <summary>
        /// Attempts to count the number of pending subscribed messages for a subscriber.
        /// </summary>
        /// <param name="subscriberId"></param>
        /// <returns></returns>
        Task<long> EstimateSubscribedPending(SubscriberId subscriberId);

        /// <summary>
        /// Adds a Subscribed message for each subscriber of the topic.
        /// </summary>
        /// <param name="message"></param>
        /// <param name="topic"></param>
        /// <returns></returns>
        Task<long> Publish(SubscribedData message, Topic topic);

        /// <summary>
        /// Inserts the Message into the completed table, and removes it from the queue table.
        /// </summary>
        /// <param name="message">The message to move.</param>
        Task CompleteMessage(SubscribedData message);

        /// <summary>
        /// Inserts the message into the error table, and removes if from the queue table.
        /// </summary>
        /// <param name="message">The message to move.</param>
        Task FailMessage(SubscribedData message);

        /// <summary>
        /// Updates a message.
        /// Only updates message properties that are allowed to change.
        /// </summary>
        /// <param name="message"></param>
        Task UpdateMessage(SubscribedData message);

        /// <summary>
        /// Moves Subscription Messages from Pending to Error that are not longer valid
        /// </summary>
        /// <param name="maxCount">The maximum number of rows to Expire</param>
        /// <returns></returns>
        Task<long> ExpireSubscriptionMessages(int maxCount);

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
