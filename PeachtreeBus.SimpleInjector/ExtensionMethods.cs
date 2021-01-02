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
    public static class ExtensionMethods
    {
        public static Container RegisterPeachtreeBusMessageHandlers(this Container container)
        {
            return RegisterPeachtreeBusMessageHandlers(container, AppDomain.CurrentDomain.GetAssemblies());
        }

        public static Container RegisterPeachtreeBusMessageHandlers(this Container container, Assembly[] assemblies)
        {
            // the interface for any message handler.
            var messsageHandlerType = typeof(IHandleMessage<>);
            // find all of the messages.
            var messageTypes = container.GetTypesToRegister(typeof(IMessage), assemblies);
            foreach (var mt in messageTypes)
            {
                // determine the generic interface for the IHandleMessage<mt>
                var genericMessageHandlerType = messsageHandlerType.MakeGenericType(mt);
                // find types that impliment IHandleMessage<mt>
                var concreteMessageHandlerTypes = container.GetTypesToRegister(genericMessageHandlerType, assemblies);
                // collection register them so the Message Processor can find the handlers.
                container.Collection.Register(genericMessageHandlerType, concreteMessageHandlerTypes, Lifestyle.Scoped);
            }

            return container;
        }

        public static Container RegisterPeachtreeBusIntervalTasks(this Container container, out IEnumerable<Type> tasks)
        {
            return RegisterPeachtreeBusIntervalTasks(container, AppDomain.CurrentDomain.GetAssemblies(), out tasks);
        }

        public static Container RegisterPeachtreeBusIntervalTasks(this Container container, Assembly[] assemblies, out IEnumerable<Type> tasks)
        {
            // get a list types that impliment the type.
            // We'll instantiate them later.
            tasks = container.GetTypesToRegister(typeof(IRunOnIntervalTask), assemblies);
            // Register the concrete types. This allows the container to do the DI later.
            foreach (var it in tasks) { container.Register(it, it, Lifestyle.Scoped); }

            return container;
        }

        public static Container RegisterPeachtreeBus(this Container container)
        {
            container.RegisterSingleton(typeof(IFindMessageHandlers), () => new SimpleInjectorFindMessageHandlers(container));
            container.Register(typeof(IMessageProcessor), typeof(MessageProcessor), Lifestyle.Scoped);
            container.Register(typeof(IQueueReader), typeof(QueueReader), Lifestyle.Scoped);
            container.Register(typeof(IQueueWriter), typeof(QueueWriter), Lifestyle.Scoped);
            container.Register(typeof(IBusDataAccess), () => container.GetInstance<IBusDataAccessFactory>().GetBusDataAccess(), Lifestyle.Scoped);
            container.Register(typeof(ISharedDatabase), () => container.GetInstance<ISharedDatabaseFactory>().GetSharedDatabase(), Lifestyle.Scoped);
            container.Register(typeof(IBusDataAccessFactory), typeof(EFBusDataAccessFactory), Lifestyle.Scoped);
            container.Register(typeof(SqlConnection), () => container.GetInstance<ISqlConnectionFactory>().GetConnection(), Lifestyle.Scoped);
            container.Register(typeof(ISqlConnectionFactory), typeof(SqlConnectionFactory), Lifestyle.Scoped);
            container.RegisterSingleton(typeof(IScopeManager), typeof(SimpleInjectorScopeManager));
            container.Register(typeof(ISharedDatabaseFactory), typeof(SharedDatabaseFactory), Lifestyle.Scoped);

            return container;
        }

        public static Container UsePeachtreeBusDbSchema(this Container container, string schema = "PeachtreeBus")
        {
            container.RegisterSingleton(typeof(IDbContextSchema), () => new DefaultDbContextSchema(schema));

            return container;
        }

        public static IList<Task> StartPeachtreeBusMessageProcessors(this Container container, int queueId, int concurrency)
        {
            var tasks = new List<Task>();


            var scopeManager = container.GetInstance<IScopeManager>();

            for (var i = 0; i < concurrency; i++)
            {
                scopeManager.Begin();
                tasks.Add(container.GetInstance<IMessageProcessor>().Run(queueId));
            }

            return tasks;
        }

        public static IList<Task> StartPeachtreeBusIntervalTasks(this Container container, IEnumerable<Type> taskTypes)
        {
            var tasks = new List<Task>();

            var scopeManager = container.GetInstance<IScopeManager>();
            foreach (var intervalTask in taskTypes)
            {
                scopeManager.Begin();
                var runner = container.GetInstance<IIntervalRunner>();
                var task = (IRunOnIntervalTask)container.GetInstance(intervalTask);
                tasks.Add(runner.Run(task));
            }

            return tasks;
        }

        public static void DisposePeachtreeBus(this Container container)
        {
            var scopeManager = container.GetInstance<IScopeManager>();
            scopeManager.DisposeAll();
        }
    }
}
