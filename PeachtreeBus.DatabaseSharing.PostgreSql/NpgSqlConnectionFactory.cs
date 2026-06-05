using Npgsql;

namespace PeachtreeBus.DatabaseSharing.PostgreSql;

public interface INpgSqlConnectionFactory : IDbConnectionFactory<INpgSqlConnection>;

public class NpgSqlConnectionFactory(
    IProvideDbConnectionString provideConnectionString)
    : INpgSqlConnectionFactory
{
    public INpgSqlConnection GetConnection()
    {
        var nativeConnection = new NpgsqlConnection(provideConnectionString.GetDbConnectionString());
        var result = new NpgSqlConnectionProxy(nativeConnection);
        return result;
    }
}