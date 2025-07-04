using Microsoft.Data.SqlClient;
using Microsoft.SqlServer.Dac;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Data;
using System.IO;
using System.Linq;

namespace PeachtreeBus.DataAccessTests.Infrastructure;

public static class DbInitialization
{
    public static void Intitialize()
    {
        using var connection = new SqlConnection(AppSettings.TestDatabase.ServerOnlyConnectionString);
        connection.Open();
        Assert.AreEqual(ConnectionState.Open, connection.State,
            "Failed to open a connection to the server for the test database.");

        var dbName = AppSettings.TestDatabase.DatabaseName;

        if (DbExists(connection, dbName))
            DropDatabase(connection, dbName);
        Assert.IsFalse(DbExists(connection, dbName),
            "Unable to remove existing test database.");

        CreateDatabase(connection, dbName);
        Assert.IsTrue(DbExists(connection, dbName),
            "Unable to create test database.");

        // create schema and tables
        // find the newest dacpac (debug or release);
        var searchpath = Path.GetFullPath(Path.Combine(Environment.CurrentDirectory, "..", "..", "..", "..", "..", "PeachtreeBus.Database"));
        var found = Directory.GetFiles(searchpath, "PeachtreeBus.Database.dacpac", SearchOption.AllDirectories)
            .Select(filename => new Tuple<string, DateTime>(filename, File.GetLastWriteTime(filename)))
            .OrderByDescending(t => t.Item2).First().Item1;

        Assert.IsNotNull(found, "Unable to find the file 'PeachtreeBus.Database.dacpac'.");

        var dacpac = DacPackage.Load(found);
        var dacService = new DacServices(AppSettings.TestDatabase);
        dacService.Deploy(dacpac, dbName, true);

        connection.Close();
    }

    private static bool DbExists(SqlConnection connection, string dbName)
    {
        var command = new SqlCommand($"SELECT database_id FROM sys.databases WHERE Name = '{dbName}'", connection);
        var result = command.ExecuteScalar();
        return (result != null);
    }

    private static void CreateDatabase(SqlConnection connection, string dbName)
    {
        var command = new SqlCommand($"CREATE DATABASE {dbName}", connection);
        command.ExecuteNonQuery();
    }

    private static void DropDatabase(SqlConnection connection, string dbName)
    {
        var command = new SqlCommand($"alter database [{dbName}] set single_user with rollback immediate; DROP DATABASE {dbName}", connection);
        command.ExecuteNonQuery();
    }
}
