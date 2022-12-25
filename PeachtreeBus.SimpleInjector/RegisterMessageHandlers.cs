using PeachtreeBus.Queues;
using SimpleInjector;
using System;
using System.Linq;
using System.Reflection;

namespace PeachtreeBus.SimpleInjector
{
    public static partial class SimpleInjectorExtensions
    {

        /// <summary>
        /// Searches the Current App Domain's Assemblies for classes that implement IHandleQueueMessage
        /// and registers them with the container.
        /// </summary>
        /// <param name="container"></param>
        /// <returns></returns>
        public static Container RegisterPeachtreeBusMessageHandlers(this Container container)
        {
            return RegisterPeachtreeBusMessageHandlers(container, AppDomain.CurrentDomain.GetAssemblies());
        }

        /// <summary>
        /// Searches the specified Assemblies for classes that implement IHandleQueueMessage
        /// and registers them with the container.
        /// </summary>
        /// <param name="container"></param>
        /// <param name="assemblies"></param>
        /// <returns></returns>
        public static Container RegisterPeachtreeBusMessageHandlers(this Container container, Assembly[] assemblies)
        {
            // the interface for any message handler.
            var messsageHandlerType = typeof(IHandleQueueMessage<>);
            // find all of the messages.
            var messageTypes = container.GetTypesToRegister(typeof(IQueueMessage), assemblies);
            foreach (var mt in messageTypes)
            {
                // determine the generic interface for the IHandleMessage<mt>
                var genericMessageHandlerType = messsageHandlerType.MakeGenericType(mt);
                // find types that impliment IHandleMessage<mt>
                var concreteMessageHandlerTypes = container.GetTypesToRegister(genericMessageHandlerType, assemblies);
                // collection register them so the Message Processor can find the handlers.
                container.Collection.Register(genericMessageHandlerType, concreteMessageHandlerTypes, Lifestyle.Transient);

                foreach (var ct in concreteMessageHandlerTypes)
                {
                    if (container.GetCurrentRegistrations().Any(ip => ip.ImplementationType == ct)) continue;
                    container.Register(ct, ct, Lifestyle.Transient);
                }
            }

            return container;
        }

    }
}
