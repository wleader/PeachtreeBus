using Npgsql;
using System.Data;
using System.IO;

namespace PeachtreeBus.DatabaseTesting.PostgreSql;

public class PostgresManageTestDatabase(IPostgreSqlTestSettings settings) : IManageTestDatabase
{
    private NpgsqlConnection GetServerConnection() => GetConnection(settings.ServerConnectionString);
    private NpgsqlConnection GetDbConnection() => GetConnection(settings.ConnectionString);

    private NpgsqlConnection GetConnection(string connectionString)
    {
        var result = new NpgsqlConnection(connectionString);
        result.Open();
        Assert.AreEqual(ConnectionState.Open, result.State, $"Could not connect to PostgreSQL server. {connectionString}");
        return result;
    }

    public bool Exists()
    {
        const string statement =
            """
            SELECT 1
            FROM pg_database
            WHERE datname = '{0}'
            """;

        using var connection = GetServerConnection();
        var command = new NpgsqlCommand(string.Format(statement, settings.DatabaseName), connection);
        var result = command.ExecuteScalar();
        return result != null;
    }

    public void Create()
    {
        const string statement = "CREATE DATABASE {0}";

        using (var connection = GetServerConnection())
        {
            var command = new NpgsqlCommand(string.Format(statement, settings.DatabaseName), connection);
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
            DROP DATABASE IF EXISTS {0};
            """;

        using (var connection = GetServerConnection())
        {
            var command = new NpgsqlCommand(string.Format(statement, settings.DatabaseName), connection);
            command.ExecuteNonQuery();
        }

        Assert.IsFalse(Exists(), $"Could not Drop Database '{settings.DatabaseName}'.");
    }

    public void Initialize()
    {
        var script = File.ReadAllText(settings.InitializeScript);
        using var connection = GetDbConnection();
        var command = new NpgsqlCommand(script, connection);
        command.ExecuteNonQuery();
    }
}
