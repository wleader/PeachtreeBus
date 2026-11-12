using System;
using System.Data;
using DotNet.Testcontainers.Images;
using Microsoft.Data.SqlClient;
using Testcontainers.MsSql;

namespace PeachtreeBus.DatabaseTesting.MsSql;

public class MsSqlInitializeDatabase(
    IMsSqlTestSettings settings,
    IMsSqlDatabaseManagement  dbManagement)
    : IInitializeTestDatabase
{
    private static MsSqlContainer? _container;

    // this could probably be made into configuration?
    private static readonly DockerImage Image = new("mcr.microsoft.com/mssql/server:2022-latest");

    public void Initialize()
    {
        try
        {
            _container = BuildContainer();
            _container.StartAsync().GetAwaiter().GetResult();
        }
        catch (Exception ex)
        {
            throw new ApplicationException(
                "Exception initializing an SQL Server for the tests. " +
                "Check that Docker Desktop is installed and running.",
                ex);
        }

        var serverConnectionString = _container.GetConnectionString();
        var newTestConnection = new SqlConnectionStringBuilder(serverConnectionString)
        {
            InitialCatalog = settings.DatabaseName
        };
        settings.ConnectionString = newTestConnection.ConnectionString;

        using var connection = new SqlConnection(serverConnectionString);
        connection.Open();
        Assert.AreEqual(ConnectionState.Open, connection.State,
            """
            Failed to open a connection to the containerized sql server.
            """);
        var dbName = settings.DatabaseName;

        if(settings.RecreateDatabase)
        {
            dbManagement.DropDatabase(connection, dbName);
        }

        if (!dbManagement.DbExists(connection, dbName))
        {
            dbManagement.CreateDatabase(connection, dbName);
            dbManagement.ApplyDacPac();
        }

        Assert.IsTrue(dbManagement.DbExists(connection, dbName),
            $"Database '{dbName}' does not exist.");

        connection.Close();
    }

    public void Cleanup()
    {
        if (!settings.ReuseContainer)
        {
            _container?.DisposeAsync().Wait();
        }
    }

    private MsSqlContainer BuildContainer()
    {
        var result = new MsSqlBuilder(Image)
            .WithReuse(settings.ReuseContainer)
            .WithLabel("reuse-id", "peachtree-bus-tests-mssql")
            // if you change the password, you will need to remove the old persisted data,
            // because the persisted data will contain the old password.
            // therefore, a hard-coded password avoids changes.
            .WithPassword("YourStrong(!)Password");

        if (settings.PersistContainerData)
        {
            result = result.WithVolumeMount("peachtree-bus-tests-mssql-persistent-data", "/var/opt/mssql");
        }

        return result.Build();
    }
}