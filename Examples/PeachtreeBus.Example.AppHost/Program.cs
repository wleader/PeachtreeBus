using System.Diagnostics.CodeAnalysis;
using System.IO;
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

        // because the AppHost project references the .SqlProj the .dacpac will have
        // been built before the AppHost, so we can assume the .dacpac is ready.
        var dacpacPath = Path.Combine(builder.AppHostDirectory,
            "../PeachtreeBus.Example.Database/bin/Debug/PeachtreeBus.Example.Database.dacpac");
        dacpacPath = Path.GetFullPath(dacpacPath);
        
        var applyDacPac = builder.AddDotnetProject("example-apply-dacpac",
                "../PeachtreeBus.Example.InitMsSql/PeachtreeBus.Example.InitMsSql.csproj")
            .WithReference(msSqlDatabase)
            .WithEnvironment("DacPacFile", dacpacPath);

        builder.AddDotnetProject("example-endpoint-mssql",
                "../PeachtreeBus.Example.MsSql/PeachtreeBus.Example.MsSql.csproj")
            .WithReference(msSqlDatabase)
            .WaitForCompletion(applyDacPac);

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