using SimpleInjector;
using SimpleInjector.Lifestyles;
using PeachtreeBus.SimpleInjector;
using Microsoft.Extensions.Configuration;
using PeachtreeBus.DatabaseSharing;
using System.Collections.Generic;
using System.Threading.Tasks;
using PeachtreeBus.Services;

namespace PeachtreeBus.Example
{
    class Program
    {
        private static readonly Container _container = new Container();

        static void Main()
        {
            // setup a scoped lifestyle.
            _container.Options.DefaultScopedLifestyle = new AsyncScopedLifestyle();

            // register message handlers and sagas.
            // this finds types that impliment IHandleMessage<>.
            _container.RegisterPeachtreeBusMessageHandlers();

            // register the bus classes.
            _container.RegisterPeachtreeBus();
            // confgure the DB schema we will use.
            _container.UsePeachtreeBusDbSchema("PeachtreeBus");

            // register tasks that should be run every so often without a message
            // Peachtree bus uses this to clean up its message queues, but application code could 
            // implement an IRunOnIntervalTask to piggy back on this.
            _container.RegisterPeachtreeBusIntervalTasks(out var intervalTasks);

            // Register some services that are needed. 
            // signal shutdown when the process is exiting.
            _container.RegisterSingleton(typeof(IProvideShutdownSignal), typeof(ProcessExitShutdownSignal));
            // log to the console window.
            _container.RegisterSingleton(typeof(ILog<>), typeof(ConsoleLog<>));
            // read the DB connection string from an appsettings.json.
            _container.RegisterSingleton(typeof(IProvideDbConnectionString), typeof(AppSettingsDatabaseConfig));
            // register a host for IRunOnIntervalTasks
            _container.Register(typeof(IIntervalRunner), typeof(IntervalRunner), Lifestyle.Scoped);

            // register an IConfiguration read from appsettings.json.
            _container.RegisterSingleton(typeof(IConfiguration), () =>
            {
                IConfigurationBuilder configurationBuilder = new ConfigurationBuilder();
                configurationBuilder.AddJsonFile("appsettings.json");
                return configurationBuilder.Build();
            });

            // sanity check.
            _container.Verify();

            // decide how many message processors to run, this could be Environment.ProcessorCount, or some function thereof.
            var concurrency = 2;
            const int QueueId = 1; // it is possible to have different queues. For this process we'll just use 1.

            // hold a list of tasks.
            var tasks = new List<Task>();

            // create message processors.
            tasks.AddRange(_container.StartPeachtreeBusMessageProcessors(QueueId, concurrency));
            // create interval runners.
            tasks.AddRange(_container.StartPeachtreeBusIntervalTasks(intervalTasks));

            // wait for all the tasks to complete (Which shouldn't happen until the shutdown signal.
            Task.WhenAll(tasks).GetAwaiter().GetResult();

            // Releases Instances that were cached in various scopes.
            _container.DisposePeachtreeBus();

        }
    }
}
