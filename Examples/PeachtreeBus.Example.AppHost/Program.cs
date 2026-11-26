using System.Diagnostics.CodeAnalysis;
using Aspire.Hosting;

namespace PeachtreeBus.Example.AppHost;

[ExcludeFromCodeCoverage(Justification = "Code is an example.")]
public static class Program
{
    public static void Main(string[] args)
    {
        var builder = DistributedApplication.CreateBuilder(args);

        var password = builder
            .AddParameter("sql-password", secret: true);

        var msSqlServer = builder
            .AddSqlServer("mssql", password)
            .WithDataVolume("peachtreebus-aspire-mssql-data");

        var msSqlDatabase = msSqlServer
            .AddDatabase("PeachtreeBusMsSql");

        var dacpac = builder.AddSqlProject("peachtreebus-example-database")
            .WithDacpac("../PeachtreeBus.Example.Database/bin/Debug/PeachtreeBus.Example.Database.dacpac")
            .WithReference(msSqlDatabase);

        builder.AddDotnetProject("example-endpoint-mssql",
                "../PeachtreeBus.Example.MsSql/PeachtreeBus.Example.MsSql.csproj")
            .WithReference(msSqlDatabase)
            .WaitFor(dacpac);

        var postgresServer = builder
            .AddPostgres("postgres")
            .WithDataVolume("peachtreebus-aspire-postgres-data");

        var postgresDatabase = postgresServer
            .AddDatabase("PeachtreeBusPostgreSql");

        // todo... initialize the DB?
        
        builder.AddDotnetProject("example-endpoint-postgres",
                "../PeachtreeBus.Example.PostgreSql/PeachtreeBus.Example.PostgreSql.csproj")
            .WithReference(postgresDatabase)
            .WithExplicitStart(); // TODO Start the Postgres Endpoint automatically.

        builder.Build().Run();
    }
}