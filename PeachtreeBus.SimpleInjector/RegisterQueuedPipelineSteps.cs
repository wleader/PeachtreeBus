using PeachtreeBus.Queues;
using SimpleInjector;
using System;
using System.Reflection;

namespace PeachtreeBus.SimpleInjector
{
    public static partial class SimpleInjectorExtensions
    {

        /// <summary>
        /// Searches the Current App Domain's Assemblies for classes that implement IQueuePipelineStep
        /// and registers them with the container.
        /// </summary>
        /// <param name="container"></param>
        /// <returns></returns>
        public static Container RegisterPeachtreeBusQueuedPipelineSteps(this Container container)
        {
            return RegisterPeachtreeBusQueuedPipelineSteps(container, AppDomain.CurrentDomain.GetAssemblies());
        }

        /// <summary>
        /// Searches the specified Assemblies for classes that implement IQueuePipelineStep
        /// and registers them with the container.
        /// </summary>
        /// <param name="container"></param>
        /// <param name="assemblies"></param>
        /// <returns></returns>
        public static Container RegisterPeachtreeBusQueuedPipelineSteps(this Container container, Assembly[] assemblies)
        {
            var foundTypes = container.GetTypesToRegister(typeof(IQueuePipelineStep), assemblies);
            container.Collection.Register(typeof(IQueuePipelineStep), foundTypes, Lifestyle.Transient);
            container.RegisterConcreteTypesIfNeeded(foundTypes, Lifestyle.Transient);
            return container;
        }

    }
}
