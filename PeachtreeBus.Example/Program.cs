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
            _container.RegisterPeachtreeBusIntervalTasks();

            // Register tasks that should be run once at startup.
            _container.RegisterPeachtreeBusStartupTasks();

            // Register some services that are needed. 
            // signal shutdown when the process is exiting.
            _container.RegisterSingleton(typeof(IProvideShutdownSignal), typeof(ProcessExitShutdownSignal));
            // log to the console window.
            _container.RegisterSingleton(typeof(ILog<>), typeof(ConsoleLog<>));
            // read the DB connection string from an appsettings.json.
            _container.RegisterSingleton(typeof(IProvideDbConnectionString), typeof(AppSettingsDatabaseConfig));
            // register a host for IRunOnIntervalTasks


            // register an IConfiguration read from appsettings.json.
            _container.RegisterSingleton(typeof(IConfiguration), () =>
            {
                IConfigurationBuilder configurationBuilder = new ConfigurationBuilder();
                configurationBuilder.AddJsonFile("appsettings.json");
                return configurationBuilder.Build();
            });

            // sanity check.
            _container.Verify();
            
            // run startup tasks.
            _container.RunPeachtreeBusStartupTasks();
            
            // decide how many message processors to run, this could be Environment.ProcessorCount, or some function thereof.
            var concurrency = 2;
            const int queueId = 1; // it is possible to have different queues. For this process we'll just use 1.

            _container.StartPeachtreeBus(queueId, concurrency).GetAwaiter().GetResult();
        }
    }
}
