using PeachtreeBus.Data;
using PeachtreeBus.DatabaseSharing;
using SimpleInjector;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Reflection;
using System.Threading.Tasks;

namespace PeachtreeBus.SimpleInjector
{
    public static partial class SimpleInjectorExtensions
    {
        /// <summary>
        /// Registers classes with the Container needed to run PeachtreeBus.
        /// </summary>
        /// <param name="container"></param>
        /// <returns></returns>
        public static Container RegisterPeachtreeBus(this Container container)
        {
            container.RegisterSingleton(typeof(IFindMessageHandlers), () => new FindMessageHandlers(container));
            container.Register(typeof(IMessageProcessor), typeof(MessageProcessor), Lifestyle.Scoped);
            container.Register(typeof(IQueueReader), typeof(QueueReader), Lifestyle.Scoped);
            container.Register(typeof(IQueueWriter), typeof(QueueWriter), Lifestyle.Scoped);
            container.Register(typeof(IBusDataAccess), typeof(DapperDataAccess), Lifestyle.Scoped);
            container.Register(typeof(ISharedDatabase), typeof(SharedDatabase), Lifestyle.Scoped);
            container.Register(typeof(SqlConnection), () => container.GetInstance<ISqlConnectionFactory>().GetConnection(), Lifestyle.Scoped);
            container.Register(typeof(ISqlConnectionFactory), typeof(SqlConnectionFactory), Lifestyle.Scoped);
            container.RegisterSingleton(typeof(IScopeManager), typeof(ScopeManager));
            container.Register(typeof(IIntervalRunner), typeof(IntervalRunner), Lifestyle.Scoped);

            return container;
        }
    }
}
