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
        /// Searches the current app domain assemblies for classes that implement IRunOnIntervalTask, 
        /// and registers them with the container.
        /// </summary>
        /// <param name="container"></param>
        /// <returns></returns>
        public static Container RegisterPeachtreeBusIntervalTasks(this Container container)
        {
            return RegisterPeachtreeBusIntervalTasks(container, AppDomain.CurrentDomain.GetAssemblies());
        }

        /// <summary>
        /// Searches the specified assemblies for the classes that implement IRunOnIntervalTask,
        /// and registers them with the container.
        /// </summary>
        /// <param name="container"></param>
        /// <param name="assemblies"></param>
        /// <returns></returns>
        public static Container RegisterPeachtreeBusIntervalTasks(this Container container, Assembly[] assemblies)
        {
            var types = container.GetTypesToRegister(typeof(IRunOnIntervalTask), assemblies);
            foreach (var t in types) { container.Register(t, t, Lifestyle.Scoped); }
            return container;
        }

        /// <summary>
        /// Creates instance of IRunOnIntervalTask, and IIntervalRunner, and runs them.
        /// </summary>
        /// <param name="container"></param>
        /// <returns>A task that completes when the interval runners have all shut down.</returns>
        public static IList<Task> StartPeachtreeBusIntervalTasks(this Container container)
        {
            var tasks = new List<Task>();
            var taskTypes = container.FindTypesThatImplement<IRunOnIntervalTask>();
            foreach (var intervalTask in taskTypes)
            {
                var scope = container.GetInstance<IScopeManager>();
                var runner = scope.GetInstance<IIntervalRunner>();
                var task = (IRunOnIntervalTask)scope.GetInstance(intervalTask);
                tasks.Add(runner.Run(task));
            }
            return tasks;
        }
    }
}
