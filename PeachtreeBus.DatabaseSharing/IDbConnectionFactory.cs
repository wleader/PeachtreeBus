namespace PeachtreeBus.DatabaseSharing;

public interface IDbConnectionFactory<out TConnectionInterface>
{
    /// <summary>
    /// Creates a new SqlConnection.
    /// </summary>
    /// <returns></returns>
    TConnectionInterface GetConnection();
}