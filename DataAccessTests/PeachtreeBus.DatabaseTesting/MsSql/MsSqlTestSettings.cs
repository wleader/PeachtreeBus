using System;
using System.IO;
using Microsoft.Extensions.Configuration;

namespace PeachtreeBus.DatabaseTesting.MsSql;

public interface IMsSqlTestSettings : ITestSettings
{
    string DacPacFile { get; }
}

public class MsSqlTestSettings(IConfigurationRoot config) : TestSettings(config), IMsSqlTestSettings
{
    public string DacPacFile => field ??= GetDacpacFile();

    private string GetDacpacFile()
    {
        var file = Config["DacPacFile"] ?? throw new ApplicationException("DacPacFile not set in appsettings.json.");
        
        return !File.Exists(file) 
            ? throw new FileNotFoundException($"File not found. {file}")
            : file;
    }
}