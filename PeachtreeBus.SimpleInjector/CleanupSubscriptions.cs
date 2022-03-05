using PeachtreeBus.Cleaners;
using SimpleInjector;

namespace PeachtreeBus.SimpleInjector
{
    public static partial class SimpleInjectorExtensions
    {
        /// <summary>
        /// Enables cleanup of the Subscriptions table. 
        /// </summary>
        /// <param name="container"></param>
        /// <returns></returns>
        public static Container CleanupSubscriptions(this Container container)
        {
            container.Register<ISubscriptionCleanupThread, SubscriptionCleanupThread>(Lifestyle.Scoped);
            container.Register<ISubscriptionCleanupWork, SubscriptionCleanupWork>(Lifestyle.Scoped);
            return container;
        }
    }
}
