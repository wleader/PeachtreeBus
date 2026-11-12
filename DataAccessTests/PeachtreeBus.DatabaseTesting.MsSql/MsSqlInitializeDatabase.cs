using System;
using System.Data;
using DotNet.Testcontainers.Images;
using Microsoft.Data.SqlClient;
using Testcontainers.MsSql;

namespace PeachtreeBus.DatabaseTesting.MsSql;

public class MsSqlInitializeDatabase(
    IMsSqlTestSettings settings,
    IDatabaseManagement  dbManagement)
    : IInitializeTestDatabase
{
    private static MsSqlContainer? _container;

    // this could probably be made into configuration?
    private static readonly DockerImage Image = new("mcr.microsoft.com/mssql/server:2022-latest");

    public void Initialize()
    {
        try
        {
            _container = new MsSqlBuilder(Image)
                // TODO enable re-use and volume mount so that the DB doesn't need recreating between test runs.
                //.WithPassword("YourStrong(!)Password")
                //.WithVolumeMount("pbus-tests-mssql-persistent-data", "/var/opt/mssql")
                //.WithReuse(true)
                //.WithLabel("reuse-id", "pbus-tests-persistent-mssql")
                .Build();
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

        using var connection = new SqlConnection(serverConnectionString);
        connection.Open();
        Assert.AreEqual(ConnectionState.Open, connection.State,
            """
            Failed to open a connection to the containerized sql server.
            """);

        var dbName = settings.DatabaseName;

        // TODO Re-add the drop and recreate behavior if configured.

        dbManagement.CreateDatabase(connection, dbName);
        
        var newTestConnection = new SqlConnectionStringBuilder(serverConnectionString)
        {
            InitialCatalog = settings.DatabaseName
        };
        settings.ConnectionString = newTestConnection.ConnectionString;

        dbManagement.ApplyDacPac();

        Assert.IsTrue(dbManagement.DbExists(connection, dbName),
            $"Database '{dbName}' does not exist.");

        connection.Close();
    }

    public void Cleanup()
    {
        // TODO don't dispose when Re-using the container between runs.
        if (_container is null) return;
        var t = _container.DisposeAsync();
        if (t.IsCompletedSuccessfully) return;
        t.AsTask().Wait();
    }
}