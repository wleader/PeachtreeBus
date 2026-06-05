using System.Data;
using Microsoft.Data.SqlClient;

namespace PeachtreeBus.DatabaseTesting.MsSql;

public class MsSqlInitializeDatabase(
    IMsSqlTestSettings settings,
    IDatabaseManagement  dbManagement)
    : IInitializeTestDatabase
{
    public void Initialize()
    {
        var connectionString = settings.TestDatabase.ServerOnlyConnectionString;
        using var connection = new SqlConnection(connectionString);
        connection.Open();
        Assert.AreEqual(ConnectionState.Open, connection.State,
            """
            Failed to open a connection to the server for the test database.
            If you need to customize the connection string for your test environment,
            You can create an appsettings.user.json file and override the test settings.
            appsettings.user.json is ignored by source control, so you can put values in it
            that you do not want to check in such as login credentials.
            """);

        var dbName = settings.TestDatabase.DatabaseName;

        if (settings.RecreateDatabase)
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
}