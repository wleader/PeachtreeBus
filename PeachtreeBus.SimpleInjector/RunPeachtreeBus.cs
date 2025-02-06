using PeachtreeBus.Cleaners;
using PeachtreeBus.DatabaseSharing;
using PeachtreeBus.Errors;
using PeachtreeBus.Queues;
using PeachtreeBus.Subscriptions;
using SimpleInjector;
using System.Collections.Generic;
using System.Linq;
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

            // Todo, make this configuration driven.
            tasks.AddIfRegistered<IQueueCleanupThread>(container, factory);
            tasks.AddIfRegistered<ISubscribedCleanupThread>(container, factory);
            tasks.AddIfRegistered<ISubscriptionCleanupThread>(container, factory);
            tasks.AddIfRegistered<ISubscriptionUpdateThread>(container, factory);
            tasks.AddIfRegistered<ISubscribedThread>(container, factory);

            for (var i = 0; i < concurrency; i++)
            {
                tasks.AddIfRegistered<IQueueThread>(container, factory);
            }

            return tasks;
        }

        private static void AddIfRegistered<T>(this List<Task> list, Container container, IWrappedScopeFactory scopeFactory)
            where T : IThread
        {
            if (container.IsRegistered<T>())
            {
                var scope = scopeFactory.Create();
                var p = (T)scope.GetInstance(typeof(T));
                list.Add(p.Run());
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
                .CheckRequirements()
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

        private static Container CheckRequirements(this Container container)
        {
            // check for things that Container.Verify can miss because they aren't obtained via constructor injection.
            if (Configuration.QueueConfiguration is not null)
                MissingRegistrationException.ThrowIfNotRegistered<IHandleFailedQueueMessages>(container,
                    $"Use when QueueConfiguration.UseDefaultFailedHandler is false, you must register your own IHandleFailedQueueMessages.");

            if (Configuration.SubscriptionConfiguration is not null)
                MissingRegistrationException.ThrowIfNotRegistered<IHandleFailedSubscribedMessages>(container,
                    $"Use when SubscriptionConfiguration.UseDefaultFailedHandler is false, you must register your own IHandleFailedSubscribedMessages.");

            // these are always required, but the user doesn't have to register them.
            // if they throw, one of our registration helpers missed them.
            MissingRegistrationException.ThrowIfNotRegistered<IShareObjectsBetweenScopes>(container);
            MissingRegistrationException.ThrowIfNotRegistered<IWrappedScopeFactory>(container);
            MissingRegistrationException.ThrowIfNotRegistered<ISqlConnectionFactory>(container);

            return container;
        }
    }
}
