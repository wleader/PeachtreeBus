namespace PeachtreeBus.DatabaseSharing;

public interface IMsSqlSharedDatabaseFactory : ISharedDatabaseFactory;

public class MsSqlSharedDatabaseFactory(
    IShareObjectsBetweenScopes shareObjectsBetweenScopes,
    ISqlConnectionFactory connectionFactory)
    : BaseSharedDatabaseFactory(shareObjectsBetweenScopes)
    , IMsSqlSharedDatabaseFactory
{
    protected override ISharedDatabase Create() => 
        new MsSqlSharedDatabase(connectionFactory);
}