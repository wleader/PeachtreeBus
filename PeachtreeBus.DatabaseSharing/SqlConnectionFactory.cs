using System.Data.SqlClient;

namespace PeachtreeBus.DatabaseSharing
{
    /// <summary>
    /// Defines an interface that provides a DB Connection string.
    /// </summary>
    public interface IProvideDbConnectionString
    {
        string GetDbConnectionString();
    }

    /// <summary>
    /// Defines an interface that creates connections to an SQL database.
    /// </summary>
    public interface ISqlConnectionFactory
    {

        /// <summary>
        /// Creates a new SqlConnection.
        /// </summary>
        /// <returns></returns>
        SqlConnection GetConnection();
    }

    /// <summary>
    /// Creates a connection to an SQL database using the Dependency injected Connection string provider.
    /// </summary>
    public class SqlConnectionFactory : ISqlConnectionFactory
    {

        private readonly IProvideDbConnectionString _provideConnectionString;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="provideConnectionString">Provides the connection string.</param>
        public SqlConnectionFactory(IProvideDbConnectionString provideConnectionString)
        {
            _provideConnectionString = provideConnectionString;
        }

        /// <inheritdoc/>
        public SqlConnection GetConnection()
        {
            var result = new SqlConnection(_provideConnectionString.GetDbConnectionString());
            return result;
        }

    }

}
