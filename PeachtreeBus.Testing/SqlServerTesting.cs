using Microsoft.Data.SqlClient;

namespace PeachtreeBus.Testing;

public abstract class SqlServerTesting
{
    /// <summary>
    /// Creates an Uninitialized SqlConnection.
    /// </summary>
    /// <returns>The result a valid reference object, but is not connected to an actual database.</returns>
    public static SqlConnection CreateConnection() => UninitializedObjects.Create<SqlConnection>();


    /// <summary>
    /// Creates an Uninitialized SqlTransaction.
    /// </summary>
    /// <returns>the result is a valid reference object, but is not connected to an actual database.</returns>
    public static SqlTransaction CreateTransaction() => UninitializedObjects.Create<SqlTransaction>();
}
