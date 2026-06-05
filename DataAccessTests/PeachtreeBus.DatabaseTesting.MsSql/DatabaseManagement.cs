using Microsoft.Data.SqlClient;
using Microsoft.SqlServer.Dac;

namespace PeachtreeBus.DatabaseTesting.MsSql;

public interface IDatabaseManagement
{
    bool DbExists(SqlConnection connection, DatabaseName name);
    void CreateDatabase(SqlConnection connection, DatabaseName name);
    void DropDatabase(SqlConnection connection, DatabaseName name);
    void ApplyDacPac();
}

public class DatabaseManagement(IMsSqlTestSettings settings) : IDatabaseManagement
{
    public bool DbExists(SqlConnection connection, DatabaseName name)
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

    public void CreateDatabase(SqlConnection connection, DatabaseName name)
    {
        const string statement = "CREATE DATABASE {0}";
        var command = new SqlCommand(string.Format(statement, name), connection);
        command.ExecuteNonQuery();

        Assert.IsTrue(DbExists(connection, name),
            $"Could not Create Database '{name}'.");
    }

    public void DropDatabase(SqlConnection connection, DatabaseName name)
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

    public void ApplyDacPac()
    {
        var dacpac = DacPackage.Load(settings.DacPacFile);
        var dacService = new DacServices(settings.TestDatabase.Value);
        dacService.Deploy(dacpac, settings.TestDatabase.DatabaseName, true);
    }
}