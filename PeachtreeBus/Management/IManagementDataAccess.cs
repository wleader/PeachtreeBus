using PeachtreeBus.Model;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace PeachtreeBus.Management
{
    public interface IManagementDataAccess
    {
        /// <summary>
        /// Retrieves a list of failed Queue messages, most recent messages first.
        /// </summary>
        /// <param name="queueName">Which queue to read from.</param>
        /// <param name="skip">Supports pagination.</param>
        /// <param name="take">Supports pagination.</param>
        Task<List<QueueMessage>> GetFailedQueueMessages(string queueName, int skip, int take);

        /// <summary>
        /// Retrieves a list of completed Queue messages, most recent messages first.
        /// </summary>
        /// <param name="queueName">Which queue to read from.</param>
        /// <param name="skip">Supports pagination.</param>
        /// <param name="take">Supports pagination.</param>
        Task<List<QueueMessage>> GetCompletedQueueMessages(string queueName, int skip, int take);

        /// <summary>
        /// Retrieves a list of pending Queue messages, most recent messages first.
        /// </summary>
        /// <param name="queueName">Which queue to read from.</param>
        /// <param name="skip">Supports pagination.</param>
        /// <param name="take">Supports pagination.</param>
        Task<List<QueueMessage>> GetPendingQueueMessages(string queueName, int skip, int take);

        /// <summary>
        /// Moves the specifed Queue message from Pending to Failed.
        /// </summary>
        /// <param name="queueName">Which Queue to interact with.</param>
        /// <param name="id">The ID of the message to move.</param>
        Task CancelPendingQueueMessage(string queueName, long id);

        /// <summary>
        /// Moves the specifed Queue message from Failed to Pending.
        /// </summary>
        /// <param name="queueName">Which Queue to interact with.</param>
        /// <param name="id">The ID of the message to move.</param>
        Task RetryFailedQueueMessage(string queueName, long id);

        /// <summary>
        /// Retrieves a list of failed Subscribed messages, most recent messages first.
        /// </summary>
        /// <param name="skip">Supports pagination.</param>
        /// <param name="take">Supports pagination.</param>
        Task<List<SubscribedMessage>> GetFailedSubscribedMessages(int skip, int take);

        /// <summary>
        /// Retrieves a list of completed Subscribed messages, most recent messages first.
        /// </summary>
        /// <param name="skip">Supports pagination.</param>
        /// <param name="take">Supports pagination.</param>
        Task<List<SubscribedMessage>> GetCompletedSubscribedMessages(int skip, int take);

        /// <summary>
        /// Retrieves a list of completed Subscribed messages, most recent messages first.
        /// </summary>
        /// <param name="skip">Supports pagination.</param>
        /// <param name="take">Supports pagination.</param>
        Task<List<SubscribedMessage>> GetPendingSubscribedMessages(int skip, int take);

        /// <summary>
        /// Moves the specifed Subscribed message from Pending to Failed.
        /// </summary>
        /// <param name="id">The ID of the message to move.</param>
        Task CancelPendingSubscribedMessage(long id);

        /// <summary>
        /// Moves the specifed Subscribed message from Failed to Pending.
        /// </summary>
        /// <param name="id">The ID of the message to move.</param>
        Task RetryFailedSubscribedMessage(long id);
    }
}
