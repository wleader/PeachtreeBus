﻿using Microsoft.Extensions.Configuration;
using PeachtreeBus.DatabaseSharing;
using PeachtreeBus.Example.Data;
using PeachtreeBus.Services;
using PeachtreeBus.SimpleInjector;
using SimpleInjector;
using SimpleInjector.Lifestyles;
using System.Linq;
using System.Threading.Tasks;

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

            // Register tasks that should be run once at startup.
            _container.RegisterPeachtreeBusStartupTasks();
            
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
            Task.WaitAll(_container.PeachtreeBusStartupTasks().ToArray());
            
            // decide how many message processors to run, this could be Environment.ProcessorCount, or some function thereof.
            var concurrency = System.Environment.ProcessorCount * 2;

            Task.WaitAll(_container.PeachtreeBusMessageProcessorTasks("SampleQueue", concurrency).ToArray());
        }
    }
}
