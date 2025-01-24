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
        public static IList<Task> PeachtreeBusThreadTasks(this Container container, int concurrency)
        {
            var factory = container.GetInstance<IWrappedScopeFactory>();
            var tasks = new List<Task>();

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
            container.CheckRequirements();

            var c = concurrency ?? System.Environment.ProcessorCount * 2;

            var tasks = container.PeachtreeBusThreadTasks(c).ToArray();

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
        }

        private static void CheckRequirements(this Container container)
        {
            // check for things that Container.Verify can miss because they aren't obtained via constructor injection.

            MissingRegistrationException.ThrowIfNotRegistered<IHandleFailedQueueMessages>(container,
                $"Use {nameof(UsePeachtreeBusFailedQueueMessageHandler)} or {nameof(UsePeachtreeBusDefaultErrorHandlers)}.");

            MissingRegistrationException.ThrowIfNotRegistered<IHandleFailedSubscribedMessages>(container,
                $"Use {nameof(UsePeachtreeBusFailedSubscribedMessageHandler)} or {nameof(UsePeachtreeBusDefaultErrorHandlers)}.");

            MissingRegistrationException.ThrowIfNotRegistered<IShareObjectsBetweenScopes>(container,
                $"Use {nameof(UsePeachtreeBus)}.");

            MissingRegistrationException.ThrowIfNotRegistered<IWrappedScopeFactory>(container,
                $"Use {nameof(UsePeachtreeBus)}.");

            MissingRegistrationException.ThrowIfNotRegistered<ISqlConnectionFactory>(container,
                $"Use {nameof(UsePeachtreeBus)}.");
        }
    }
}
