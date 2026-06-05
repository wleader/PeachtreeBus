namespace PeachtreeBus.DatabaseTesting;

public interface IDbConnectionString
{
    string Value { get; }
    DatabaseName DatabaseName { get; }
    string ServerOnlyConnectionString { get; }
    string ToString();
}