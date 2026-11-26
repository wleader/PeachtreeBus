namespace PeachtreeBus.DatabaseSharing;

public interface IMsSqlSharedDatabaseFactory : ISharedDatabaseFactory;

public class NpgSqlSharedDatabaseFactory(
    IShareObjectsBetweenScopes shareObjectsBetweenScopes,
    INpgSqlConnectionFactory connectionFactory)
    : BaseSharedDatabaseFactory(shareObjectsBetweenScopes)
    , IMsSqlSharedDatabaseFactory
{
    protected override ISharedDatabase Create() => 
        new NpgSqlSharedDatabase(connectionFactory);
}