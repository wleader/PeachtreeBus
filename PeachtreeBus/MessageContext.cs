using PeachtreeBus.Model;
using System;
using System.Collections.Generic;

namespace PeachtreeBus
{
    /// <summary>
    /// Holds information about the mesage currently being processed.
    /// </summary>
    public class MessageContext
    {
        /// <summary>
        /// Headers that were stored with the message.
        /// </summary>
        public Headers Headers { get; set; }

        /// <summary>
        /// The message itself.
        /// </summary>
        public object Message { get; set; }

        /// <summary>
        /// The Model of the message as was stored the database.
        /// </summary>
        public QueueMessage MessageData { get; set; }

        /// <summary>
        /// Which Queue the message was read from.
        /// </summary>
        public string SourceQueue { get; set; }

        /// <summary>
        /// The Model of the saga data related to the message (Null when the message is not part of a saga)
        /// Will be null if the saga is starting and has never persisted to the DB before.
        /// Will be null if the row is locked.
        /// </summary>
        public SagaData SagaData { get; set; }

        /// <summary>
        /// Indicates if the reason SagaData is null is because the row was locked.
        /// </summary>
        public bool SagaLocked { get; set; }

        /// <summary>
        /// The Saga instance Key for the messge.
        /// (Null when the message is not part of a saga.
        /// </summary>
        public string SagaKey { get; set; }

        /// <summary>
        /// Contains a list of messages that have been sent from message handlers.
        /// Holding the messages in a list, allows the context to be inspected easily
        /// from unit tests on the handlers.
        /// </summary>
        public IList<ContextSentMessage> SentMessages { get; private set; }
        
        public MessageContext()
        {
            SentMessages = new List<ContextSentMessage>();
        }

        /// <summary>
        /// A convience function for sending a message from a handler. Adds the message to the SentMessages.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="message"></param>
        /// <param name="queueId"></param>
        /// <param name="notBefore"></param>
        public void Send<T>(T message, string queueName = null, DateTime? notBefore = null)
        {
            SentMessages.Add(new ContextSentMessage
            {
                Type = typeof(T),
                Message = message,
                QueueName = queueName ?? SourceQueue,
                NotBefore = notBefore
            });
        }
    }
}
