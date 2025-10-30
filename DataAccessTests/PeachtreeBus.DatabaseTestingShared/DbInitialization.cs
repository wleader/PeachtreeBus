using System.Data;
using Microsoft.Data.SqlClient;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace PeachtreeBus.DatabaseTestingShared;

public static class DbInitialization
{
    public static void Initialize()
    {
        var connectionString = TestSettings.TestDatabase.ServerOnlyConnectionString;
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

        var dbName = TestSettings.TestDatabase.DatabaseName;

        DatabaseManagement.DropDatabase(connection, dbName);
        DatabaseManagement.CreateDatabase(connection, dbName);
        DatabaseManagement.ApplyDacPac();

        connection.Close();
    }
}