using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using System;

namespace PeachtreeBus.DataAccessTests.Infrastructure;

public static class AppSettings
{
    public static IConfigurationRoot ConfigurationRoot { get; private set; } = default!;

    public static DBConnectionString TestDatabase { get; private set; }
    public static DBConnectionString InvalidDatabase { get; private set; }

    public static void Initialize()
    {
        var configurationBuilder = new ConfigurationBuilder();
        configurationBuilder.AddJsonFile("appsettings.json");
        ConfigurationRoot = configurationBuilder.Build();

        TestDatabase = ConfigurationRoot.GetConnectionString("TestDatabase");
        InvalidDatabase = ConfigurationRoot.GetConnectionString("InvalidDatabase");
    }
}

public readonly record struct DBConnectionString
{
    public string Value { get; }
    public string DatabaseName { get; }
    public string ServerOnlyConnectionString { get; }
    public DBConnectionString(string? connectionString)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(connectionString);
        Value = connectionString;

        var builder = new SqlConnectionStringBuilder(connectionString);
        if (string.IsNullOrWhiteSpace(builder.InitialCatalog))
            throw new InvalidOperationException("The connection string did not specify an initial catalog.");

        DatabaseName = builder.InitialCatalog;

        builder.InitialCatalog = string.Empty;
        ServerOnlyConnectionString = builder.ConnectionString;
    }

    public override string ToString() => Value;

    public static implicit operator string(DBConnectionString value) => value.Value;
    public static implicit operator DBConnectionString(string? value) => new(value);
}
