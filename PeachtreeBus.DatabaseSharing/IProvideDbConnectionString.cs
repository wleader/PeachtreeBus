namespace PeachtreeBus.DatabaseSharing;

/// <summary>
/// Defines an interface that provides a DB Connection string.
/// </summary>
public interface IProvideDbConnectionString
{
    string GetDbConnectionString();
}