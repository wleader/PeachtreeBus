using Microsoft.Data.SqlClient;
using Microsoft.SqlServer.Dac;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace PeachtreeBus.DatabaseTestingShared;

public static class DatabaseManagement
{
    private static bool DbExists(SqlConnection connection, DatabaseName name)
    {
        const string statement =
            """
            SELECT database_id
            FROM sys.databases
            WHERE Name = '{0}'
            """;
        
        var command = new SqlCommand(string.Format(statement, name), connection);
        var result = command.ExecuteScalar();
        return (result != null);
    }

    public static void CreateDatabase(SqlConnection connection, DatabaseName name)
    {
        const string statement = "CREATE DATABASE {0}";
        var command = new SqlCommand(string.Format(statement, name), connection);
        command.ExecuteNonQuery();

        Assert.IsTrue(DbExists(connection, name),
            $"Could not Create Database '{name}'.");
    }

    public static void DropDatabase(SqlConnection connection, DatabaseName name)
    {
        if (!DbExists(connection, name))
            return;

        const string statement =
            """
            ALTER DATABASE [{0}]
            SET SINGLE_USER WITH ROLLBACK IMMEDIATE;
            DROP DATABASE {0}
            """;
        var command = new SqlCommand(string.Format(statement, name), connection);
        command.ExecuteNonQuery();
        
        Assert.IsFalse(DbExists(connection, name),
            $"Could not Drop Database '{name}'.");
    }

    public static void ApplyDacPac()
    {
        var dacpac = DacPackage.Load(TestSettings.DacPacFile);
        var dacService = new DacServices(TestSettings.TestDatabase);
        dacService.Deploy(dacpac, TestSettings.TestDatabase.DatabaseName, true);
    }
}