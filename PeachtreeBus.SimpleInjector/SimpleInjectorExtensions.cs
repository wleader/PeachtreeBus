using Microsoft.Extensions.Logging;
using PeachtreeBus.Tasks;
using SimpleInjector;
using SimpleInjector.Lifestyles;
using System.Collections.Generic;
using System.Reflection;

namespace PeachtreeBus.SimpleInjector
{
    public static class SimpleInjectorExtensions
    {
        /// <summary>
        /// Enables Basic PeachtreeBus functionality.
        /// Registers needed services with the Container.
        /// </summary>
        public static Container UsePeachtreeBus(this Container container, IBusConfiguration busConfiguration, ILoggerFactory loggerFactory, List<Assembly>? assemblies = null)
        {
            var provider = new SimpleInjectorRegistrationProvider(container, loggerFactory);
            var components = new RegisterComponents(provider);
            components.Register(busConfiguration, assemblies);
            return container;
        }

        /// <summary>
        /// Gets the registered threads, starts them, then waits for all of them to complete.
        /// </summary>
        /// <param name="container"></param>
        /// <param name="concurrency"></param>
        public static void RunPeachtreeBus(this Container container)
        {
            using var scope = AsyncScopedLifestyle.BeginScope(container);
            var runner = container.GetInstance<IRunStartupTasks>();
            runner.RunStartupTasks();

            var manager = scope.GetInstance<ITaskManager>();
            var shutdown = scope.GetInstance<IProvideShutdownSignal>();
            // This blocks until it completes
            manager.Run(shutdown.GetCancellationToken()).GetAwaiter().GetResult();
        }
    }
}
