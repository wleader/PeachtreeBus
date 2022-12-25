using PeachtreeBus.Subscriptions;
using SimpleInjector;
using System;
using System.Linq;
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

            // Register the concrete types. This allows the container to do the DI later.
            foreach (var t in foundTypes)
            {
                if (container.GetCurrentRegistrations().Any(ip => ip.ImplementationType == t)) continue;
                container.Register(t, t, Lifestyle.Transient);
            }
            return container;
        }

    }
}
