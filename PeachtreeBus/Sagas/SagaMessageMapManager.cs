using System;
using System.Collections.Generic;

namespace PeachtreeBus.Sagas
{
    public interface ISagaMessageMapManager
    {
        string GetKey(object saga, object message);
    }


    /// <summary>
    /// Maintains a set of SagaMessageMaps for the message processor to resolve 
    /// Saga Keys when a saga message is processed.
    /// </summary>
    public class SagaMessageMapManager : ISagaMessageMapManager
    {

        private readonly Dictionary<Type, SagaMessageMap> Maps = new Dictionary<Type, SagaMessageMap>();

        /// <summary>
        /// Gets a Saga Key for a given Saga and Mesasge.
        /// </summary>
        /// <param name="saga"></param>
        /// <param name="message"></param>
        /// <returns></returns>
        public string GetKey(object saga, object message)
        {
            var sagaType = saga.GetType();

            // There is a small chance two threads could pick up two messages for the same saga at the same time
            // only one of them needs to build the map and the other should wait. The maps dictionary not containing 
            // the saga should only happen once, so the rest of the time this should be a fast lock and check.
            lock (Maps)
            {
                if (!Maps.ContainsKey(sagaType))
                {
                    // A message for this saga hasn't been processed yet in this process, so we need to
                    // create the map.

                    var mapper = new SagaMessageMap();
                    var parameterTypes = new[] { typeof(SagaMessageMap) };
                    var configureMethod = sagaType.GetMethod("ConfigureMessageKeys", parameterTypes);
                    configureMethod.Invoke(saga, new object[] { mapper });
                    Maps.Add(sagaType, mapper);

                    // todo, check that ConfigureMessageKeys mapped all message types that the saga has interfaces for, 
                    // and only those message types.
                }
            }
            // Get the saga key for the message using the map.
            return Maps[sagaType].GetKey(message);
        }
    }
}
