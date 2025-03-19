using PeachtreeBus.Exceptions;
using PeachtreeBus.Queues;
using System;
using System.Collections.Generic;

namespace PeachtreeBus.Sagas
{

    public class SagaMapException(string message) : PeachtreeBusException(message);

    /// <summary>
    /// Stores a set of functions that are used to get a Saga Key for messages handled by a single saga class.
    /// There will be one instance of this map in memory for each saga class. It is built on demand when
    /// a message for that saga is processed.
    /// </summary>
    public class SagaMessageMap : ISagaMessageMap
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
        public void Add<TMessage>(Func<TMessage, SagaKey> MapFunction) where TMessage : IQueueMessage
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
        public SagaKey GetKey(object message)
        {
            var messageType = message.GetType();

            if (!MapFunctions.TryGetValue(messageType, out object? function))
                throw new SagaMapException($"The SagaMessageMap does not contain a mapping for message type {messageType}.");

            var funcType = typeof(Func<,>).MakeGenericType([messageType, typeof(SagaKey)]);

            var invokeMethod = UnreachableException.ThrowIfNull(funcType.GetMethod("Invoke"),
                message: "Func<,> must have an Invoke method.");

            var result = invokeMethod.Invoke(function, [message]);
            return result is SagaKey sagaKey
                ? sagaKey
                : throw new SagaMapException("Map function did not return a SagaKey.");
        }
    }
}
