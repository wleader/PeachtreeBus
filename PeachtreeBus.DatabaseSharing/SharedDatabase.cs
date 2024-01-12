using Microsoft.Data.SqlClient;
using System;

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
    public interface ISharedDatabase : IDisposable
    {
        /// <summary>
        /// Starts a transaction. Only one transaction should be started.
        /// Causes the TransactionStarted event.
        /// </summary>
        void BeginTransaction();

        /// <summary>
        /// Commits a transaction.
        /// Causes the TransactionConsumed event.
        /// </summary>
        void CommitTransaction();

        /// <summary>
        /// Rolls back a transaction.
        /// Causes the TransactionConsumed event.
        /// </summary>
        void RollbackTransaction();

        /// <summary>
        /// Closes and reopens the database connection.
        /// </summary>
        void Reconnect();

        /// <summary>
        /// Creates a Savepoint in the Transaction.
        /// Similar to a nested transaction.
        /// </summary>
        /// <param name="name">The name of the savepoint to create.</param>
        void CreateSavepoint(string name);

        /// <summary>
        /// Rolls back to Savepoint in the transaction.
        /// Similar to a nested transaction rollboack.
        /// </summary>
        /// <param name="name">The name of the savepoint to rollback to.</param>
        void RollbackToSavepoint(string name);

        /// <summary>
        /// The current Transaction (Null when there is no transaction).
        /// </summary>
        SqlTransaction Transaction { get; }

        SqlConnection Connection { get; }

        /// <summary>
        /// An event to signal a new transaction so that other DbContext instances can synchronize transaction usage.
        /// </summary>
        event EventHandler TransactionStarted;

        /// <summary>
        /// An event to signal a transaction ended so that other DbContext instances can synchronize transaction usage.
        /// </summary>
        event EventHandler TransactionConsumed;
    }

    /// <summary>
    /// A class that manages sharing a transaction between Data Access code
    /// Intended to live a scoped lifestyle so that all data access classes in the same scope
    /// share the same DB transaction.
    /// </summary>
    public class SharedDatabase : ISharedDatabase
    {
        /// <summary>
        /// used to ensure thread safety.
        /// </summary>
        private readonly object _lock = new();

        /// <inheritdoc/>
        public event EventHandler TransactionStarted;
        /// <inheritdoc/>
        public event EventHandler TransactionConsumed;

        /// <inheritdoc/>
        public SqlTransaction Transaction { get; private set; }

        public SqlConnection Connection { get; private set; }

        private readonly ISqlConnectionFactory _connectionFactory;

        public SharedDatabase(ISqlConnectionFactory connectionFactory)
        {
            _connectionFactory = connectionFactory;
            Connection = _connectionFactory.GetConnection();
            Connection.Open();
            Transaction = null;
        }


        /// <inheritdoc/>
        public void BeginTransaction()
        {
            lock (_lock)
            {
                if (Connection.State != System.Data.ConnectionState.Open)
                {
                    Reconnect();
                }

                if (Transaction != null) throw new SharedDatabaseException("There is already a transaction. Use CreateSavePoint instead of nested transactions.");
                Transaction = Connection.BeginTransaction();
            }
            TransactionStarted?.Invoke(this, null);
        }

        /// <inheritdoc/>
        public void CommitTransaction()
        {
            lock (_lock)
            {
                if (Transaction == null) throw new SharedDatabaseException("There is no transaction to commit.");
                Transaction.Commit();
                Transaction = null;
            }
            TransactionConsumed?.Invoke(this, null);
        }

        /// <inheritdoc/>
        public void CreateSavepoint(string name)
        {
            lock (_lock)
            {
                if (Transaction == null) throw new SharedDatabaseException("There is no transaction to create a save point in.");

                Transaction.Save(name);
            }
        }

        /// <inheritdoc/>
        public void RollbackToSavepoint(string name)
        {
            lock (_lock)
            {
                if (Transaction == null) throw new SharedDatabaseException("There is no transaction to roll back to a save point.");
                Transaction.Rollback(name);
            }
        }

        /// <inheritdoc/>
        public void RollbackTransaction()
        {
            lock (_lock)
            {
                if (Transaction == null) throw new SharedDatabaseException("There is no transaction to roll back.");
                Transaction.Rollback();
                Transaction = null;
            }
            TransactionConsumed?.Invoke(this, null);
        }

        /// <inheritdoc/>
        public void Reconnect()
        {
            lock (_lock)
            {
                if (Transaction is not null)
                {
                    Transaction.Dispose();
                    Transaction = null;
                    TransactionConsumed?.Invoke(this, null);
                }

                Connection?.Close();
                Connection?.Dispose();
                Connection = _connectionFactory.GetConnection();
                Connection.Open();
            }
        }

        public void Dispose()
        {
            lock (_lock)
            {
                if (Transaction is not null)
                {
                    Transaction.Dispose();
                    Transaction = null;
                    TransactionConsumed?.Invoke(this, null);
                }
                Connection?.Close();
                Connection?.Dispose();
            }
            GC.SuppressFinalize(this);  
        }
    }
}
