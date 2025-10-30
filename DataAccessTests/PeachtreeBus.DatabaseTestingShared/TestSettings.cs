using System;
using System.IO;
using Microsoft.Extensions.Configuration;

namespace PeachtreeBus.DatabaseTestingShared;

public static class TestSettings
{
    public static IConfigurationRoot ConfigurationRoot { get; private set; } = null!;
    public static DbConnectionString TestDatabase { get; private set; }
    public static string DacPacFile { get; private set; } = null!;
    
    public static void Initialize()
    {
        var configurationBuilder = new ConfigurationBuilder()
            .AddJsonFile("appsettings.json")
            .AddJsonFile("appsettings.user.json", true);
        ConfigurationRoot = configurationBuilder.Build();

        TestDatabase = ConfigurationRoot.GetConnectionString("TestDatabase");
        DacPacFile = ConfigurationRoot["DacPacFile"]
            ?? throw new ApplicationException("DacPacFile not set in appsettings.json.");
        
        if (!File.Exists(DacPacFile))
            throw new FileNotFoundException($"File not found. {DacPacFile}");
    }
}