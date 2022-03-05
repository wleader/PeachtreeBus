using PeachtreeBus.Cleaners;
using SimpleInjector;
using System;

namespace PeachtreeBus.SimpleInjector
{
    public static partial class SimpleInjectorExtensions
    {
        /// <summary>
        /// Enables cleanup of the subscribed messages tables.
        /// </summary>
        /// <param name="container">The Simple Injector container.</param>
        /// <param name="maxDeleteCount">The maximum number of rows to delete in a single DB transaction.</param>
        /// <param name="cleanCompleted">Should completed messages be cleaned up.</param>
        /// <param name="cleanFailed">Should failed messages be cleaned up.</param>
        /// <param name="ageLimit">How long after completion or failure for a message to be eligible for cleanup.</param>
        /// <param name="interval">How long to wait between searches for data to clean up.</param>
        /// <returns></returns>
        public static Container CleanupSubscribed(this Container container,
            int maxDeleteCount, bool cleanCompleted, bool cleanFailed,
            TimeSpan ageLimit, TimeSpan interval)
        {
            CleanupSubscribed(container, new SubscribedCleanupConfiguration(
                maxDeleteCount,
                cleanCompleted,
                cleanFailed,
                ageLimit,
                interval));
            return container;
        }

        /// <summary>
        /// Enables cleanup of the subscribed messages tables.
        /// </summary>
        /// <param name="container">The Simple Injector container.</param>
        /// <param name="config">The configuration for the cleanup.</param>
        /// <returns></returns>
        public static Container CleanupSubscribed(this Container container, ISubscribedCleanupConfiguration config)
        {
            container.Register<ISubscribedCleanupThread, SubscribedCleanupThread>(Lifestyle.Scoped);
            container.Register<ISubscribedCleanupWork, SubscribedCleanupWork>(Lifestyle.Scoped);
            container.Register<ISubscribedCleaner, SubscribedCleaner>(Lifestyle.Scoped);
            container.RegisterSingleton(() => config);
            return container;
        }
    }
}
