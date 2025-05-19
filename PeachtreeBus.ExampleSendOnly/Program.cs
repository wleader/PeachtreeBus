using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using PeachtreeBus;
using PeachtreeBus.DatabaseSharing;
using PeachtreeBus.Queues;
using PeachtreeBus.SimpleInjector;
using SimpleInjector;
using SimpleInjector.Lifestyles;
using System;
using System.Diagnostics.CodeAnalysis;

[assembly: ExcludeFromCodeCoverage(Justification = "This is example code.")]

internal class Program
{
    private static readonly Container _container = new();

    private static void Main()
    {
        // setup a scoped lifestyle.
        _container.Options.DefaultScopedLifestyle = new AsyncScopedLifestyle();

        // get configuration from appsettings.json
        var configurationBuilder = new ConfigurationBuilder();
        configurationBuilder.AddJsonFile("appsettings.json");
        var configuration = configurationBuilder.Build();
        _container.RegisterSingleton<IConfiguration>(() => configuration);

        // read our connection string from the appsettings configuration.
        var connectionString = configuration.GetConnectionString("PeachtreeBus")
            ?? throw new ApplicationException("A PeachtreeBus connection string is not configured.");

        // log to the console window.
        using ILoggerFactory loggerFactory = LoggerFactory.Create(builder =>
        {
            builder.AddSimpleConsole();
        });

        var busConfiguration = new BusConfiguration()
        {
            // requried. This must always be configured.
            Schema = new("PeachtreeBus"),

            // this is deliberate to show that we are not
            // creating the connection using this value.
            ConnectionString = string.Empty,

            PublishConfiguration = new()
            {
                // When publishing a message to subscribers, this determins how long the message
                // can stay in the pending messages before it is considered abandoned.
                Lifespan = TimeSpan.FromDays(1),
            },
        };

        // register the PeachtreeBus components with the container.
        // if you are using a different dependency injection system,
        // you may have to register manually.
        _container.UsePeachtreeBus(busConfiguration, loggerFactory);

        SendMessage();
    }

    private static void SendMessage()
    {
        // we must have a scope because the queue writer and shared database
        // are scoped objects.
        // if you are using a different dependency injection system,
        // you may have to provide your own IWrappedScopeFactory,
        // and register manually.
        var scopeFactory = _container.GetInstance<IWrappedScopeFactory>();
        using var scope = scopeFactory.Create();

        // setup our connection to the database and begin a transaction.
        var configuration = scope.GetInstance<IConfiguration>();
        var connectionString = configuration.GetConnectionString("PeachtreeBus");
        using var connection = new SqlConnection(connectionString);
        connection.Open();
        using var transaction = connection.BeginTransaction();

        // get the Shared Database Object
        // the container will return the same object that the queue writer will use.
        var sharedDb = scope.GetInstance<ISharedDatabase>();
        // replace the connection.
        sharedDb.SetExternallyManagedConnection(connection, transaction);

        // create and send our message.
        var message = new PeachtreeBus.Example.Messages.SampleSagaStart()
        {
            AppId = Guid.NewGuid(),
        };
        var queueName = new QueueName("SampleQueue");

        var queueWriter = scope.GetInstance<IQueueWriter>();
        queueWriter.WriteMessage(queueName, message).GetAwaiter().GetResult();

        transaction.Commit();
    }
}