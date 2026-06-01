using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using PeachtreeBus;
using PeachtreeBus.DatabaseSharing;
using PeachtreeBus.Queues;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using PeachtreeBus.MicrosoftDependencyInjection;

[assembly: ExcludeFromCodeCoverage(Justification = "This is example code.")]

internal class Program
{
    private static async Task Main(string[] args)
    {
        // get configuration from appsettings.json
        var configurationBuilder = new ConfigurationBuilder();
        configurationBuilder.AddJsonFile("appsettings.json");
        var configuration = configurationBuilder.Build();

        // read our connection string from the appsettings configuration.
        var connectionString = configuration.GetConnectionString("PeachtreeBus")
            ?? throw new ApplicationException("A PeachtreeBus connection string is not configured.");

        var builder = Host.CreateApplicationBuilder(args);
        builder.ConfigureContainer(new DefaultServiceProviderFactory(new()
        {
            ValidateOnBuild = true,
            ValidateScopes = true
        }));

        builder.Configuration.AddConfiguration(configuration);

        // log to the console window.
        builder.Logging.ClearProviders();
        builder.Logging.AddSimpleConsole();


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

        // registers PeachtreeBus components with the container.
        builder.Services.AddPeachtreeBus(busConfiguration);
        
        using var host = builder.Build();

        SendMessage(host.Services);
    }

    private static void SendMessage(IServiceProvider serviceProvider)
    {
        // There must be a scope because the queue writer and shared database
        // are scoped objects.
        // If you are using a different dependency injection system,
        // you may have to provide your own IScopeFactory,
        // and register manually.
        var scopeFactory = serviceProvider.GetRequiredService<IScopeFactory>();

        // The scope factory gives us an IServiceProviderAccessor.
        // The accessor has a reference to the IServiceProvider for the scope.
        // The IServiceProvider can be used to get scoped objects.
        using var accessor = scopeFactory.Create();

        // Setup our connection to the database and begin a transaction.
        var configuration = accessor.GetRequiredService<IConfiguration>();
        var connectionString = configuration.GetConnectionString("PeachtreeBus");
        using var connection = new SqlConnection(connectionString);
        connection.Open();
        using var transaction = connection.BeginTransaction();

        // Get the Shared Database Object
        // The container will return the same object that the QueueWriter will use.
        var sharedDb = accessor.GetRequiredService<ISharedDatabase>();
        // Replace the connection.
        sharedDb.SetExternallyManagedConnection(connection, transaction);

        // Create the message.
        var message = new PeachtreeBus.Example.Messages.SampleSagaStart()
        {
            AppId = Guid.NewGuid(),
        };
        var queueName = new QueueName("SampleQueue");

        // Get the QueueWriter and send the message.
        var queueWriter = accessor.GetRequiredService<IQueueWriter>();
        queueWriter.WriteMessage(queueName, message).GetAwaiter().GetResult();

        // Commit the transaction.
        transaction.Commit();
    }
}