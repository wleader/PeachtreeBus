using System.Threading;

namespace PeachtreeBus.DatabaseSharing;

public abstract class BaseSharedDatabaseFactory(
    IShareObjectsBetweenScopes shareObjectsBetweenScopes)
{
    private readonly Lock _lockObject = new();
    public ISharedDatabase GetSharedDatabase()
    {
        lock (_lockObject)
        {
            return shareObjectsBetweenScopes.SharedDatabase ??= Create();
        }
    }
    protected abstract ISharedDatabase Create();
}