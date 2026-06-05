using Microsoft.Extensions.Configuration;

namespace PeachtreeBus.DatabaseTesting;

public interface ITestSettings
{
    bool RecreateDatabase { get; }
}

public class TestSettings(IConfigurationRoot config) : ITestSettings
{
    protected IConfigurationRoot Config => config;
    
    public bool RecreateDatabase => Config
        .GetSection("TestSettings")
        .GetValue<bool>("RecreateDatabase");
}