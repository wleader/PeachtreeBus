using PeachtreeBus.Cleaners;
using PeachtreeBus.Queues;
using PeachtreeBus.Subscriptions;
using SimpleInjector;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace PeachtreeBus.SimpleInjector
{
    public static partial class SimpleInjectorExtensions
    {
        /// <summary>
        /// Creates instances of registered threads.
        /// </summary>
        /// <param name="container"></param>
        /// <param name="concurrency">The number of copies of the IQueueThread to create.</param>
        /// <returns>A List of Tasks to run.</returns>
        public static IList<Task> GetThreadTasks(this Container container, int concurrency)
        {
            var factory = container.GetInstance<IWrappedScopeFactory>();
            var tasks = new List<Task>();

            var shutdownProvider = container.GetInstance<IProvideShutdownSignal>();
            var token = shutdownProvider.GetCancellationToken();

            if (Configuration.SubscriptionConfiguration is not null)
            {
                tasks.AddIfRegistered<ISubscribedCleanupThread>(container, factory, token);
                tasks.AddIfRegistered<ISubscriptionCleanupThread>(container, factory, token);
                tasks.AddIfRegistered<ISubscriptionUpdateThread>(container, factory, token);
                tasks.AddIfRegistered<ISubscribedThread>(container, factory, token);
            }

            if (Configuration.QueueConfiguration is not null)
            {
                tasks.AddIfRegistered<IQueueCleanupThread>(container, factory, token);
                for (var i = 0; i < concurrency; i++)
                {
                    tasks.AddIfRegistered<IQueueThread>(container, factory, token);
                }
            }

            return tasks;
        }

        private static void AddIfRegistered<T>(
            this List<Task> list,
            Container container,
            IWrappedScopeFactory scopeFactory,
            CancellationToken cancellationToken)
            where T : IThread
        {
            if (container.IsRegistered<T>())
            {
                var scope = scopeFactory.Create();
                var p = (T)scope.GetInstance(typeof(T));
                list.Add(p.Run(cancellationToken));
            }
        }

        /// <summary>
        /// Gets the registered threads, starts them, then waits for all of them to complete.
        /// </summary>
        /// <param name="container"></param>
        /// <param name="concurrency"></param>
        public static void RunPeachtreeBus(this Container container, int? concurrency = null)
        {
            container
                .RunStartupTasks()
                .RunThreadTasks(concurrency);
        }

        private static Container RunThreadTasks(this Container container, int? concurrency = null)
        {

            var c = concurrency ?? System.Environment.ProcessorCount * 2;

            var tasks = container.GetThreadTasks(c).ToArray();

            var threads = new List<System.Threading.Thread>();
            foreach (var task in tasks)
            {
                var thread = new System.Threading.Thread(() => task.GetAwaiter().GetResult());
                thread.Start();
                threads.Add(thread);
            }

            foreach (var thread in threads)
            {
                thread.Join();
            }

            return container;
        }
    }
}
