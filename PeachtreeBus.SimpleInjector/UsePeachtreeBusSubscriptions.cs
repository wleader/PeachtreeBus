using PeachtreeBus.Subscriptions;
using SimpleInjector;

namespace PeachtreeBus.SimpleInjector
{
    public static partial class SimpleInjectorExtensions
    {
        /// <summary>
        /// Enables the handling of subscribed messages.
        /// </summary>
        /// <param name="container"></param>
        /// <param name="config"></param>
        /// <returns></returns>
        public static Container UsePeachtreeBusSubscriptions(this Container container, ISubscriberConfiguration config)
        {
            // register our subscription message handlers
            container.RegisterPeachtreeBusSubscriptionHandlers();

            // register stuff needed to process subscribed messages.
            container.Register(typeof(ISubscribedThread), typeof(SubscribedThread), Lifestyle.Scoped);
            container.Register(typeof(ISubscribedWork), typeof(SubscribedWork), Lifestyle.Scoped);
            container.Register(typeof(ISubscribedReader), typeof(SubscribedReader), Lifestyle.Scoped);
            container.Register(typeof(IFindSubscribedHandlers), typeof(FindSubscribedHandlers), Lifestyle.Scoped);
            container.Register(typeof(IFindSubscribedPipelineSteps), typeof(FindSubscribedPipelineSteps), Lifestyle.Scoped);
            container.Register(typeof(ISubscriptionUpdateThread), typeof(SubscriptionUpdateThread), Lifestyle.Scoped);
            container.Register(typeof(ISubscriptionUpdateWork), typeof(SubscriptionUpdateWork), Lifestyle.Scoped);

            // register our subscription configuration.
            container.Register(typeof(ISubscriberConfiguration), () => config, Lifestyle.Singleton);

            // register pipeline steps
            container.RegisterPeachtreeBusSubscribedPipelineSteps();

            return container;
        }
    }
}
