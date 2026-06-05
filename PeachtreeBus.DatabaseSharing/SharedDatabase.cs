using Microsoft.Data.SqlClient;
using System;
using System.Data.Common;
using System.Diagnostics;

namespace PeachtreeBus.DatabaseSharing
{
    /// <summary>
    /// The interface for a class that shares a database connection and transaction.
    /// </summary>
    /// <remarks>
    /// The intention here is that bus will be using the database as its message store, but other application code
    /// will also want to interact with the database using the same connection inside the same DB Transaction. So this
    /// interface defines an object that holds the transaction for use by both the bus and the application's data access,
    /// allowing the bus code to start the transaction, the application code to enlist the transaction, then return to the
    /// bus code who commits or rolls back. The object that impliments this interface needs to be a scoped lifestyle in the
    /// dependency Injection container. This allows all the classes that interact with the database within the same message 
    /// handling scope to use the same connection and transaction.
    /// </remarks>
    public interface ISqlSharedDatabase : IBaseSharedDatabase<SqlConnection, SqlTransaction>, IDisposable
    {
    }

    /// <summary>
    /// A class that manages sharing a transaction between Data Access code
    /// Intended to live a scoped lifestyle so that all data access classes in the same scope
    /// share the same DB transaction.
    /// </summary>
    [DebuggerDisplay("SharedDatabase [{InstanceId}]")]
    public class SharedDatabase(ISqlConnectionFactory connectionFactory)
        : BaseSharedDatabase<SqlConnection, ISqlConnection, SqlTransaction, ISqlTransaction>(connectionFactory),
            ISqlSharedDatabase
    {
        protected override ISqlConnection CreateExternalConnection(DbConnection connection)
        {
            var sqlConnection = connection as SqlConnection
                                ?? throw new ExternallyManagedConnectionException(
                                    "The provided connection object is not an SqlConnection.");
            return new ExternallyManagedSqlConnection(sqlConnection);
        }

        protected override ISqlTransaction CreateExternalTransaction(DbTransaction transaction)
        {
            var sqlTransaction = transaction as SqlTransaction
                                 ?? throw new ExternallyManagedTransactionException(
                                     "The provided transaction object is not an SqlTransaction.");
            return new ExternallyManagedSqlTransaction(sqlTransaction);
        }
    }
}