using PeachtreeBus.Tasks;
using SimpleInjector;
using SimpleInjector.Lifestyles;

namespace PeachtreeBus.SimpleInjector
{
    public static partial class SimpleInjectorExtensions
    {
        /// <summary>
        /// Gets the registered threads, starts them, then waits for all of them to complete.
        /// </summary>
        /// <param name="container"></param>
        /// <param name="concurrency"></param>
        public static void RunPeachtreeBus(this Container container)
        {
            container
                .RunStartupTasks()
                .RunTaskManager();
        }

        private static Container RunTaskManager(this Container container)
        {
            using var scope = AsyncScopedLifestyle.BeginScope(container);
            var manager = scope.GetInstance<ITaskManager>();
            var shutdown = scope.GetInstance<IProvideShutdownSignal>();
            // This blocks until it completes
            manager.Run(shutdown.GetCancellationToken()).GetAwaiter().GetResult();
            return container;
        }
    }
}
