using System;

namespace PeachtreeBus.DatabaseTesting;

public interface ITestSettings
{
    string ConnectionString { get; set; }
}

public class TestSettings : ITestSettings
{
    public string ConnectionString { get => string.IsNullOrWhiteSpace(field)
            ? throw new ApplicationException("ConnectionString for the test database has not be set.")
            : field; 
        set; }
}