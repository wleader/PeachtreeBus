using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using PeachtreeBus.DatabaseSharing;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;

namespace PeachtreeBus.DatabaseSharing
{
    /// <summary>
    /// Defines the methods available on an SharedContext.
    /// </summary>
    public interface ISharedContext
    {
        /// <summary>
        /// Begins a DB Transaction.
        /// </summary>
        void BeginTransaction();

        /// <summary>
        /// Commits the Current DB Transaction.
        /// </summary>
        void CommitTransaction();

        /// <summary>
        /// Rolls back the current DB tranasaction.
        /// </summary>
        void RollbackTransaction();

        /// <summary>
        /// Creates a Save Point 
        /// </summary>
        /// <param name="name">The name of the save point to create.</param>
        void CreateSavepoint(string name);

        /// <summary>
        /// Rolls back to a named save point.
        /// </summary>
        /// <param name="name">The name of the save point to roll backk to.</param>
        void RollbackToSavepoint(string name);

        /// <summary>
        /// A method for executing scalar queries (returns one value.)
        /// </summary>
        /// <param name="statement">The SQL Statement.</param>
        /// <param name="parameters">The Parameters.</param>
        /// <returns>The object result of the query. </returns>
        object ExectueScalar(string statement, IEnumerable<Parameter> parameters = null);

        /// <summary>
        /// A Method for executing a statement that returns no value.
        /// </summary>
        /// <param name="statement">The SQL Statement</param>
        /// <param name="parameters">The Parameters.</param>
        void ExectueNonQuery(string statement, IEnumerable<Parameter> parameters = null);

        /// <summary>
        /// Tells EF to forget changes, so that the context can be reused without recreating.
        /// </summary>        
        void ClearChangeTracker();

        /// <summary>
        /// Sends tracked changes to the database.
        /// </summary>
        /// <returns>The number of state entries written to the underlying database.</returns>
        int SaveChanges();
    }

    /// <summary>
    /// A DB Context that automatically shares the transaction and connection with other
    /// instances of itself within the same Dependency Injection Scope. Includes
    /// some helper functions for Scalar and Non-Query statements.
    /// </summary>
    public abstract class SharedContext : DbContext, ISharedContext
    {
        protected readonly ISharedDatabase _sharedDB;

        public SharedContext(ISharedDatabase transactionHolder)
        {
            _sharedDB = transactionHolder;
            _sharedDB.TransactionStarted += SharedDb_TransactionStarted;
            _sharedDB.TransactionConsumed += SharedDb_TransactionConsumed;

            if (transactionHolder.Transaction != null)
            {
                // if a transaction was started on the connection before this instance was created,
                // enlist that transaction.
                Database.UseTransaction(transactionHolder.Transaction.GetDbTransaction());
            }
        }

        private void SharedDb_TransactionConsumed(object sender, EventArgs e)
        {
            // The transaction was committed or rolled back, we no longer need to track it.
            Database.UseTransaction(null);
        }

        private void SharedDb_TransactionStarted(object sender, EventArgs e)
        {
            // a transaction was started on a context that shares this connection.
            // if it wasn't this intance we need to enlist it.
            var dbTransaction = _sharedDB.Transaction.GetDbTransaction();
            if (!ReferenceEquals(Database.CurrentTransaction?.GetDbTransaction(), dbTransaction))
            {
                Database.UseTransaction(dbTransaction);
            }
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            // Makes EF use the existing SQL connection instead of creating its own.
            optionsBuilder.UseSqlServer(_sharedDB.Connection);
        }

        private DbCommand CreateCommand(string statement, IEnumerable<Parameter> parameters = null)
        {
            // Create a Command object that uses the current connection,
            var connection = Database.GetDbConnection();
            if (connection.State != System.Data.ConnectionState.Open) connection.Open();
            var cmd = connection.CreateCommand();
            cmd.CommandText = statement;
            cmd.CommandType = System.Data.CommandType.Text;

            // enlists the current transaction (if there is one)
            if (_sharedDB.Transaction != null)
            { cmd.Transaction = _sharedDB.Transaction.GetDbTransaction(); }

            if (parameters != null)
            {
                // convert the parameters to the appropriate underlying type.
                // CreateParameter is defined as DbParameter, but its really a SqlParameter behind the scenes.
                foreach (var p in parameters)
                {
                    var dbparam = cmd.CreateParameter();
                    dbparam.ParameterName = p.Name;
                    dbparam.DbType = p.Type;
                    dbparam.Value = p.Value;
                    cmd.Parameters.Add(dbparam);
                }
            }
            return cmd;
        }


        /// <inheritdoc/>
        public object ExectueScalar(string statement, IEnumerable<Parameter> parameters = null)
        {
            // parameters can not contain @result. We need to set it up as an output parameter.
            if (parameters != null && parameters.Any(p => p.Name.ToLower() == "@result"))
            {
                throw new ApplicationException("@result parameter will be setup automatically.");
            }

            // create the command.
            var cmd = CreateCommand(statement, parameters);

            // add out output parameter.
            var result = cmd.CreateParameter();
            result.ParameterName = "@result";
            result.Direction = System.Data.ParameterDirection.Output;
            result.Size = 1000; // no idea if this makes sense.
            cmd.Parameters.Add(result);

            // execute the query
            // returns the number of rows affected but we can discard that.
            _ = cmd.ExecuteNonQueryAsync().GetAwaiter().GetResult();

            // return the result of the query.
            return result?.Value;
        }

        /// <inheritdoc/>
        public void ExectueNonQuery(string statement, IEnumerable<Parameter> parameters = null)
        {
            var cmd = CreateCommand(statement, parameters);
            cmd.ExecuteNonQuery();
        }

        /// <inheritdoc/>
        public void BeginTransaction()
        {
            _sharedDB.BeginTransaction(this);
        }

        /// <inheritdoc/>
        public void CommitTransaction()
        {
            _sharedDB.CommitTransaction();
        }

        /// <inheritdoc/>
        public void RollbackTransaction()
        {
            _sharedDB.RollbackTransaction();
        }

        /// <inheritdoc/>
        public void CreateSavepoint(string name)
        {
            _sharedDB.CreateSavepoint(name);
        }

        /// <inheritdoc/>
        public void RollbackToSavepoint(string name)
        {
            _sharedDB.RollbackToSavepoint(name);
        }

        /// <inheritdoc/>
        public void ClearChangeTracker()
        {
            ChangeTracker.Clear();
        }
    }
}
