using PeachtreeBus.Subscriptions;
using SimpleInjector;
using System;
using System.Reflection;

namespace PeachtreeBus.SimpleInjector
{
    public static partial class SimpleInjectorExtensions
    {

        /// <summary>
        /// Searches the Current App Domain's Assemblies for classes that implement ISubscribedPipelineStep
        /// and registers them with the container.
        /// </summary>
        /// <param name="container"></param>
        /// <returns></returns>
        public static Container RegisterPeachtreeBusSubscribedPipelineSteps(this Container container)
        {
            return RegisterPeachtreeBusSubscribedPipelineSteps(container, AppDomain.CurrentDomain.GetAssemblies());
        }

        /// <summary>
        /// Searches the specified Assemblies for classes that implement ISubscribedPipelineStep
        /// and registers them with the container.
        /// </summary>
        /// <param name="container"></param>
        /// <param name="assemblies"></param>
        /// <returns></returns>
        public static Container RegisterPeachtreeBusSubscribedPipelineSteps(this Container container, Assembly[] assemblies)
        {
            var foundTypes = container.GetTypesToRegister(typeof(ISubscribedPipelineStep), assemblies);
            container.Collection.Register(typeof(ISubscribedPipelineStep), foundTypes, Lifestyle.Transient);
            container.RegisterConcreteTypesIfNeeded(foundTypes, Lifestyle.Transient);
            return container;
        }

    }
}
