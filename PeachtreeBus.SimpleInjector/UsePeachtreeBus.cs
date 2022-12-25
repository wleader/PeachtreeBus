using PeachtreeBus.Subscriptions;
using SimpleInjector;
using System;

namespace PeachtreeBus.SimpleInjector
{
    public static partial class SimpleInjectorExtensions
    {
        /// <summary>
        /// Enables Basic PeachtreeBus functionality, such as sending Queue or Subsribed messages.
        /// </summary>
        /// <param name="container"></param>
        /// <param name="dbSchema"></param>
        /// <param name="subscribedLifespan"></param>
        /// <returns></returns>
        public static Container UsePeachtreeBus(this Container container, string dbSchema, TimeSpan? subscribedLifespan = null)
        {
            // register the bus classes.
            container.RegisterRequiredPeachtreeBus();

            // confgure the DB schema we will use.
            container.UsePeachtreeBusDbSchema(dbSchema);

            // register our subscription configuration.
            container.Register(typeof(ISubscribedLifespan),
                () => new SubscribedLifespan(subscribedLifespan ?? TimeSpan.FromHours(1)),
                Lifestyle.Singleton);

            // Register tasks that should be run once at startup.
            container.RegisterPeachtreeBusStartupTasks();

            return container;
        }
    }
}
