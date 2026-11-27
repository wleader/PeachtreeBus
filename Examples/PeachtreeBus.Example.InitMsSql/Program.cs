using System;
using Microsoft.Extensions.Configuration;
using Microsoft.SqlServer.Dac;

namespace PeachtreeBus.Example.InitMsSql;

public static class Program
{
    public static void Main(string[] _)
    {
        var configuration = new ConfigurationBuilder()
            .AddJsonFile("appsettings.json")
            .AddEnvironmentVariables()
            .Build();
        
        var connectionString = configuration.GetConnectionString("PeachtreeBusMsSql");
        Console.WriteLine(connectionString);

        var dacpacPath = configuration["DacPacFile"];
        Console.WriteLine(dacpacPath);

        var dacpac = DacPackage.Load(dacpacPath);
        var dacService = new DacServices(connectionString);
        dacService.Deploy(dacpac, "PeachtreeBusMsSql", true);
    }
}