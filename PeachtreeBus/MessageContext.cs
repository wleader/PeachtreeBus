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
        public int SourceQueue { get; set; }

        /// <summary>
        /// The Model of the saga data related to the message (Null when the message is not part of a saga)
        /// Will be null if the saga is starting and has never persisted to the DB before.
        /// </summary>
        public SagaData SagaData { get; set; }

        /// <summary>
        /// The Saga instance Key for the messge.
        /// (Null when the message is not part of a saga.
        /// </summary>
        public string SagaKey { get; set; }

        public IList<ContextSentMessage> SentMessages { get; private set; }
        
        public MessageContext()
        {
            SentMessages = new List<ContextSentMessage>();
        }

        public void Send<T>(T message, int? queueId = null, DateTime? notBefore = null)
        {
            SentMessages.Add(new ContextSentMessage
            {
                Type = typeof(T),
                Message = message,
                QueueId = queueId ?? SourceQueue,
                NotBefore = notBefore
            });
        }
    }
}
