using Microsoft.Data.SqlClient;

namespace PeachtreeBus.DatabaseSharing;

/// <summary>
/// Defines an interface that creates connections to an SQL database.
/// </summary>
public interface ISqlConnectionFactory : IDbConnectionFactory<ISqlConnection>;

/// <summary>
/// Creates a connection to an SQL database using the Dependency Injected Connection String Provider.
/// </summary>
public class SqlConnectionFactory(
    IProvideDbConnectionString provideConnectionString)
    : ISqlConnectionFactory
{
    public ISqlConnection GetConnection()
    {
        var nativeConnection = new SqlConnection(provideConnectionString.GetDbConnectionString());
        var result = new SqlConnectionProxy(nativeConnection);
        return result;
    }
}