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

        var sqlServer = builder
            .AddSqlServer("mssql", password)
            .WithDataVolume("peachtreebus-aspire-mssql-data");

        var database = sqlServer
            .AddDatabase("PeachtreeBus");

        var dacpac = builder.AddSqlProject("peachtreebus-example-database")
            .WithDacpac("../PeachtreeBus.Example.Database/bin/Debug/PeachtreeBus.Example.Database.dacpac")
            .WithReference(database);

        builder.AddDotnetProject("example-endpoint-mssql",
                "../PeachtreeBus.Example.MicrosoftDI/PeachtreeBus.Example.MicrosoftDI.csproj")
             .WithReference(database)
             .WaitFor(dacpac);

        builder.Build().Run();
    }
}