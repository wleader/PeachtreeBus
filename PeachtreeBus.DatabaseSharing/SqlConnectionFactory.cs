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
        ISqlConnection GetConnection();
    }

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
        private readonly IProvideDbConnectionString _provideConnectionString = provideConnectionString;

        public ISqlConnection GetConnection()
        {
            var result = new SqlConnectionProxy(_provideConnectionString.GetDbConnectionString());
            return result;
        }
    }
}
