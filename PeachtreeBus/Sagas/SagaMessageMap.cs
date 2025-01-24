using PeachtreeBus.Queues;
using System;
using System.Collections.Generic;

namespace PeachtreeBus.Sagas
{
    public class SagaMapException(string message) : PeachtreeBusException(message)
    {
    }

    /// <summary>
    /// Stores a set of functions that are used to get a Saga Key for messages handled by a single saga class.
    /// There will be one instance of this map in memory for each saga class. It is built on demand when
    /// a message for that saga is processed.
    /// </summary>
    public class SagaMessageMap
    {
        /// <summary>
        /// A dictionary where the message type is the key, and the key function is the value.
        /// </summary>
        private readonly Dictionary<Type, object> MapFunctions = [];

        /// <summary>
        /// Called in the Saga's ConfigureMessageKeys to tell the bus how to calculate a Saga Key from a given message type.
        /// </summary>
        /// <typeparam name="TMessage"></typeparam>
        /// <param name="MapFunction"></param>
        public void Add<TMessage>(Func<TMessage, string> MapFunction) where TMessage : IQueueMessage
        {
            lock (MapFunction)
            {
                if (MapFunctions.ContainsKey(typeof(TMessage))) throw new SagaMapException($"The SagaMessageMap already contains a mapping for message type {typeof(TMessage)}.");
                MapFunctions.Add(typeof(TMessage), MapFunction);
            }
        }

        /// <summary>
        /// Used when processing a message to get the saga key for that message.
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        public string GetKey(object message)
        {
            var messageType = message.GetType();
            if (!MapFunctions.ContainsKey(messageType))
            {
                throw new SagaMapException($"The SagaMessageMap does not contain a mapping for message type {messageType}.");
            }

            var function = MapFunctions[messageType];
            var funcType = typeof(Func<,>).MakeGenericType([messageType, typeof(string)]);
            var invokeMethod = funcType.GetMethod("Invoke");
            var result = invokeMethod.Invoke(function, [message]);
            return result is string stringResult
                ? stringResult
                : throw new SagaMapException("Map function did not return a string.");
        }
    }
}
