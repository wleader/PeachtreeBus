using System.Data;
using Microsoft.Data.SqlClient;
using Microsoft.SqlServer.Dac;

namespace PeachtreeBus.DatabaseTesting.MsSql;

public class MsSqlDatabaseManagement(IMsSqlTestSettings settings) : IManageTestDatabase
{
    private SqlConnection GetServerConnection()
    {
        var result = new SqlConnection(settings.ServerConnectionString);
        result.Open();
        Assert.AreEqual(ConnectionState.Open , result.State, "Could not connect to SQL Server.");
        return result;
    }

    public bool Exists()
    {
        const string statement =
            """
            SELECT database_id
            FROM sys.databases
            WHERE Name = '{0}'
            """;

        using var connection = GetServerConnection();
        var command = new SqlCommand(string.Format(statement, settings.DatabaseName), connection);
        var result = command.ExecuteScalar();
        return result != null;
    }

    public void Create()
    {
        const string statement = "CREATE DATABASE {0}";

        using (var connection = GetServerConnection())
        {
            var command = new SqlCommand(string.Format(statement, settings.DatabaseName), connection);
            command.ExecuteNonQuery();
        }

        Assert.IsTrue(Exists(), $"Could not Create Database '{settings.DatabaseName}'.");
    }

    public void Drop()
    {
        if (!Exists())
            return;

        const string statement =
            """
            ALTER DATABASE [{0}]
            SET SINGLE_USER WITH ROLLBACK IMMEDIATE;
            DROP DATABASE {0}
            """;

        using (var connection = GetServerConnection())
        {
            var command = new SqlCommand(string.Format(statement, settings.DatabaseName), connection);
            command.ExecuteNonQuery();
        }
        
        Assert.IsFalse(Exists(), $"Could not Drop Database '{settings.DatabaseName}'.");
    }

    public void Initialize()
    {
        var dacpac = DacPackage.Load(settings.DacPacFile);
        var dacService = new DacServices(settings.ConnectionString);
        dacService.Deploy(dacpac, settings.DatabaseName, true);
    }
}