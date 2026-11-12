using System;
using System.IO;
using Microsoft.Extensions.Configuration;

namespace PeachtreeBus.DatabaseTesting.MsSql;

public interface IMsSqlTestSettings : ITestSettings
{
    string DacPacFile { get; }
    string DatabaseName { get; }
}

public class MsSqlTestSettings(IConfigurationRoot config) : TestSettings, IMsSqlTestSettings
{
    public string DacPacFile => field ??= GetDacpacFile();

    public string DatabaseName => "PeachtreeBusTesting";

    private string GetDacpacFile()
    {
        var file = config["DacPacFile"] ?? throw new ApplicationException("DacPacFile not set in appsettings.json.");
        
        return !File.Exists(file) 
            ? throw new FileNotFoundException($"File not found. {file}")
            : file;
    }
}