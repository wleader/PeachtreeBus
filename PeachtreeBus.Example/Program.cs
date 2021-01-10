using Microsoft.Extensions.Configuration;
using PeachtreeBus.DatabaseSharing;
using PeachtreeBus.Example.Data;
using PeachtreeBus.Services;
using PeachtreeBus.SimpleInjector;
using SimpleInjector;
using SimpleInjector.Lifestyles;

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
            
            // Tell the code which queues we want this process to be responsible for cleaning up.
            // Cleaning involves moving completed and failed messages out of the queue table into
            // respective completed and error tables.
            _container.RegisterSingleton(typeof(IConfigureQueueCleaner), () => new ConfigureQueueCleaner("SampleQueue"));

            // register the data access used by the sample application.
            _container.Register(typeof(IExampleDataAccess), typeof(ExampleDataAccess), Lifestyle.Scoped);

            // Register some services that are needed. 
            // signal shutdown when the process is exiting.
            _container.RegisterSingleton(typeof(IProvideShutdownSignal), typeof(ProcessExitShutdownSignal));
            // log to the console window.
            _container.RegisterSingleton(typeof(ILog<>), typeof(ConsoleLog<>));
            // read the DB connection string from an appsettings.json.
            _container.RegisterSingleton(typeof(IProvideDbConnectionString), typeof(AppSettingsDatabaseConfig));
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
            
            _container.StartPeachtreeBus("SampleQueue", concurrency).GetAwaiter().GetResult();
        }
    }
}
