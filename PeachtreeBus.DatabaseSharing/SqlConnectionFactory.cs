using Microsoft.Data.SqlClient;

namespace PeachtreeBus.DatabaseSharing;

/// <summary>
/// Defines an interface that provides a DB Connection string.
/// </summary>
public interface IProvideDbConnectionString
{
    string GetDbConnectionString();
}

public interface IDbConnectionFactory<TConnectionInterface>
{
    /// <summary>
    /// Creates a new SqlConnection.
    /// </summary>
    /// <returns></returns>
    TConnectionInterface GetConnection();
}

/// <summary>
/// Defines an interface that creates connections to an SQL database.
/// </summary>
public interface ISqlConnectionFactory : IDbConnectionFactory<ISqlConnection>;

/// <summary>
/// Creates a connection to an SQL database using the Dependency injected Connection string provider.
/// </summary>
/// <remarks>
/// Constructor
/// </remarks>
/// <param name="provideConnectionString">Provides the connection string.</param>
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