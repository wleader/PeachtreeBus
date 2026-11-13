using System;
using System.IO;
using Microsoft.Extensions.Configuration;

namespace PeachtreeBus.DatabaseTesting.PostgreSql;

public interface IPostgreSqlTestSettings : ITestSettings
{
    string InitializeScript { get; }
}

public class PostgreSqlTestSettings(IConfigurationRoot config) : TestSettings(config), IPostgreSqlTestSettings
{
    public string InitializeScript => field ??= GetInitializeScript();

    private string GetInitializeScript()
    {
        var file = Config["InitializeScript"] ?? throw new ApplicationException("InitializeScript not set in appsettings.json.");

        Console.WriteLine(Environment.CurrentDirectory + file);
        file = Path.GetFullPath(file);
        Console.WriteLine(file);

        return !File.Exists(file)
            ? throw new FileNotFoundException($"File not found. {file}")
            : file;
    }
}
