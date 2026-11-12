namespace PeachtreeBus.DatabaseSharing;

public interface ISharedDatabaseFactory
{
    public ISharedDatabase GetSharedDatabase();
}