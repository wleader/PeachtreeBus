using SimpleInjector;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;

namespace PeachtreeBus.SimpleInjector
{
    public static partial class SimpleInjectorExtensions
    {
        /// <summary>
        /// Searches the Current App Domain's Assemblies for IRunOnStartup implementations
        /// and registers them with the container.
        /// </summary>
        /// <param name="container"></param>
        /// <returns></returns>
        public static Container RegisterPeachtreeBusStartupTasks(this Container container)
        {
            return RegisterPeachtreeBusStartupTasks(container, AppDomain.CurrentDomain.GetAssemblies());
        }


        /// <summary>
        /// Searches the specified assemblies for IRunOnStartup implementations
        /// and registers them with the container.
        /// </summary>
        /// <param name="container"></param>
        /// <param name="assemblies"></param>
        /// <returns></returns>
        public static Container RegisterPeachtreeBusStartupTasks(this Container container, Assembly[] assemblies)
        {
            // get a list types that impliment the type.
            // We'll instantiate them later.
            var runOnStartupTypes = container.GetTypesToRegister(typeof(IRunOnStartup), assemblies);
            // Register the concrete types. This allows the container to do the DI later.
            foreach (var t in runOnStartupTypes) { container.Register(t, t, Lifestyle.Scoped); }
            return container;
        }

        /// <summary>
        /// Creates intances of IRunOnStartup an runs them in parallel.
        /// </summary>
        /// <param name="container"></param>
        public static void RunPeachtreeBusStartupTasks(this Container container)
        {
            var tasks = new List<Task>();
            var startupTaskTypes = container.FindTypesThatImplement<IRunOnStartup>();
            
            foreach (var t in startupTaskTypes)
            {
                var scope = container.GetInstance<IScopeManager>();
                var startupTask = (IRunOnStartup)scope.GetInstance(t);
                tasks.Add(startupTask.Run());
            }
            Task.WhenAll(tasks).GetAwaiter().GetResult();
        }
    }
}
