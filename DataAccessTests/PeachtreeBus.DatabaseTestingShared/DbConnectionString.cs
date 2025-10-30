using System;
using Microsoft.Data.SqlClient;

namespace PeachtreeBus.DatabaseTestingShared;

public readonly record struct DbConnectionString
{
    public string Value { get; }
    public DatabaseName DatabaseName { get; }
    public string ServerOnlyConnectionString { get; }
    public DbConnectionString(string? connectionString)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(connectionString);
        Value = connectionString;

        var builder = new SqlConnectionStringBuilder(connectionString);
        if (string.IsNullOrWhiteSpace(builder.InitialCatalog))
            throw new InvalidOperationException("The connection string did not specify an initial catalog.");

        DatabaseName = builder.InitialCatalog;

        builder.Remove("Initial Catalog");
        builder.Remove("Database");
        ServerOnlyConnectionString = builder.ConnectionString;
    }

    public override string ToString() => Value;

    public static implicit operator string(DbConnectionString value) => value.Value;
    public static implicit operator DbConnectionString(string? value) => new(value);
}