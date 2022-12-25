using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using PeachtreeBus.DatabaseSharing;
using PeachtreeBus.Example.Data;
using PeachtreeBus.Services;
using PeachtreeBus.SimpleInjector;
using SimpleInjector;
using SimpleInjector.Lifestyles;
using System;

namespace PeachtreeBus.Example
{
    /// <summary>
    /// A program that demonstrates using PeachtreeBus with SimpleInjector.
    /// </summary>
    class Program
    {
        /// <summary>
        /// our IOC Container.
        /// </summary>
        private static readonly Container _container = new();

        static void Main()
        {
            // setup a scoped lifestyle.
            _container.Options.DefaultScopedLifestyle = new AsyncScopedLifestyle();

            // This will:
            // Register common types required to run the bus,
            // configure which DB Schema, and which Queue will be used.
            // Register Startup tasks found in loaded assemblies.
            _container.UsePeachtreeBus("PeachtreeBus");

            // Register Queue handlers (and Sagas) found in loaded assemblies.
            _container.UsePeachtreeBusQueue("SampleQueue");

            // This will:
            // Register Subscription Handlers found in loaded assemblies.
            // Register Types needed to use subscriptions.
            // provide subscription configuration.
            // in this case the subscriber ID is always the same, but in a real
            // application each instance will need a different ID. 
            var subscriberId = Guid.Parse("E00E876C-A9F4-46C4-B0E7-2B27C525FA98");
            _container.UsePeachtreeBusSubscriptions(new Subscriptions.SubscriberConfiguration(
                subscriberId, TimeSpan.FromSeconds(60), "Announcements"));

            // this will:
            // setup cleaning of the sample queue.
            // 10 to messages will be cleaned per run
            // completed messages will be cleaned,
            // failed messages will not be cleaned,
            // messages that are 1 day old will be cleaned
            // cleanup code will not run for 1 minue when there is nothing to clean.
            _container.CleanupQueue("SampleQueue", 10, true, false, TimeSpan.FromDays(1), TimeSpan.FromMinutes(1));

            // Same as above, but for subscribed messages.
            _container.CleanupSubscribed(10, true, false, TimeSpan.FromDays(1), TimeSpan.FromMinutes(1));

            // this will keep the subscriptions table clean from subscribers that forgot about themselves.
            _container.CleanupSubscriptions();

            // Register some services that are needed by PeachtreeBus
            // These are things you may want to replace in your application.
            // signal shutdown when the process is exiting.
            _container.RegisterSingleton(typeof(IProvideShutdownSignal), typeof(ProcessExitShutdownSignal));

            // log to the console window.
            using ILoggerFactory loggerFactory = LoggerFactory.Create(builder =>
            {
                builder.AddSimpleConsole();
            });
            _container.RegisterInstance<ILoggerFactory>(loggerFactory);
            _container.RegisterSingleton(typeof(ILogger<>), typeof(Logger<>));

            // read the DB connection string from an appsettings.json.
            _container.RegisterSingleton(typeof(IProvideDbConnectionString), typeof(AppSettingsDatabaseConfig));
            // register an IConfiguration read from appsettings.json.
            _container.RegisterSingleton(typeof(IConfiguration), () =>
            {
                IConfigurationBuilder configurationBuilder = new ConfigurationBuilder();
                configurationBuilder.AddJsonFile("appsettings.json");
                return configurationBuilder.Build();
            });

            // Register things needed by the handlers.
            // register the data access used by the sample application.
            _container.Register(typeof(IExampleDataAccess), typeof(ExampleDataAccess), Lifestyle.Scoped);

            // sanity check that everything that the IOC container needs to create
            // objects has been registered.
            _container.Verify();

            // run startup tasks.
            // this runs anything that implements IRunOnStartup
            _container.RunPeachtreeBusStartupTasks();

            // run!
            // this will run different looping threads based on the above code
            // Since UsePeachtreeBusQueue was called, the message queue will be serviced, etc.
            _container.RunPeachtreeBus();
        }
    }
}
