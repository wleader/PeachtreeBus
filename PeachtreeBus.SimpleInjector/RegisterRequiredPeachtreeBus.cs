using PeachtreeBus.Data;
using PeachtreeBus.DatabaseSharing;
using PeachtreeBus.Errors;
using PeachtreeBus.Interfaces;
using PeachtreeBus.Queues;
using PeachtreeBus.Subscriptions;
using SimpleInjector;
using System.Data.SqlClient;

namespace PeachtreeBus.SimpleInjector
{
    public static partial class SimpleInjectorExtensions
    {
        /// <summary>
        /// Registers classes with the Container needed to run PeachtreeBus.
        /// </summary>
        /// <param name="container"></param>
        /// <returns></returns>
        public static Container RegisterRequiredPeachtreeBus(this Container container)
        {
            // Data access components are needed to:
            // send messages (queue or subscribed)
            // handle message (queue or susbscribed)
            // Subscribed to messages
            // do cleanups
            // pretty much everything, so always register these things.
            container.Register(typeof(IBusDataAccess), typeof(DapperDataAccess), Lifestyle.Scoped);
            container.Register(typeof(ISharedDatabase), typeof(SharedDatabase), Lifestyle.Scoped);
            container.Register(typeof(SqlConnection), () => container.GetInstance<ISqlConnectionFactory>().GetConnection(), Lifestyle.Scoped);
            container.Register(typeof(ISqlConnectionFactory), typeof(SqlConnectionFactory), Lifestyle.Scoped);

            // All of the worker threads need to operate in a scope,
            // so scope handling is always required.
            container.Register(typeof(IWrappedScopeFactory), () => new SimpleInjectorScopeFactory(container), Lifestyle.Singleton);
            container.Register(typeof(IWrappedScope), typeof(SimpleInjectorScope), Lifestyle.Scoped);

            // enables perf counters.
            container.RegisterSingleton(typeof(IPerfCounters), () => PerfCounters.Instance());

            // a serializer is needed to convert objects such as messages and saga data to and
            // from strings that can be stored in a single database column.
            // this can be replaced in the container if desired, but one is required for the bus 
            // code to function.
            container.RegisterSingleton(typeof(ISerializer), typeof(DefaultSerializer));

            // provide an abstracted access to the system clock 
            // supports unit testable code.
            container.RegisterSingleton(typeof(ISystemClock), typeof(SystemClock));

            // anybody should be able to send messages to a queue,
            // or publish subscribed messages without being a 
            // consumer of either.
            container.Register(typeof(IQueueWriter), typeof(QueueWriter), Lifestyle.Scoped);
            container.Register(typeof(ISubscribedPublisher), typeof(SubscribedPublisher), Lifestyle.Scoped);

            // failed message handlers
            container.Register(typeof(IFailedQueueMessageHandlerFactory), typeof(FailedQueueMessageHandlerFactory), Lifestyle.Scoped);
            container.Register(typeof(IQueueFailures), typeof(QueueFailures), Lifestyle.Scoped);

            container.Register(typeof(IFailedSubscribedMessageHandlerFactory), typeof(FailedSubscribedMessageHandlerFactory), Lifestyle.Scoped);
            container.Register(typeof(ISubscribedFailures), typeof(SubscribedFailures), Lifestyle.Scoped);

            return container;
        }
    }
}
