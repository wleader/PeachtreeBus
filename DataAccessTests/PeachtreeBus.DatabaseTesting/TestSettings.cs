using System;
using Microsoft.Extensions.Configuration;

namespace PeachtreeBus.DatabaseTesting;

public interface ITestSettings
{
    string ConnectionString { get; set; }
    bool RecreateDatabase { get; }
    bool ReuseContainer { get; }
    bool PersistContainerData { get; }
    string DatabaseName { get; }
}

public class TestSettings(IConfigurationRoot config) : ITestSettings
{
    protected IConfigurationRoot Config = config;

    public string ConnectionString { get => string.IsNullOrWhiteSpace(field)
            ? throw new ApplicationException("ConnectionString for the test database has not be set.")
            : field; 
        set; }

    public bool RecreateDatabase => Config
        .GetSection("TestSettings")
        .GetValue<bool>("RecreateDatabase");
    public bool ReuseContainer => Config
        .GetSection("TestSettings")
        .GetValue<bool>("ReuseContainer");
    public bool PersistContainerData => Config
        .GetSection("TestSettings")
        .GetValue<bool>("PersistContainerData");
    public string DatabaseName => Config
        .GetSection("TestSettings")
        .GetValue<string>("DatabaseName")
        ?? throw new ApplicationException("DatabaseName is not configured.");
}