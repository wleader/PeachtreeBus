using PeachtreeBus.Model;
using PeachtreeBus.DatabaseSharing;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.CompilerServices;

namespace PeachtreeBus.Data
{
    /// <summary>
    /// An Implementionat of IBusDataAccess that uses Entity Framework Core.
    /// </summary>
    public class EFBusDataAccess : IBusDataAccess
    {
        private readonly IEFContext _context;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="context">The EntityFramework DBContext</param>
        public EFBusDataAccess(IEFContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Adds a queue message.
        /// </summary>
        /// <param name="message">A queue message to insert.</param>
        public void Add(QueueMessage message)
        {
            _context.QueueMessages.Add(message);
        }

        /// <summary>
        /// Causes the underlying context to forget changes so that it can be
        /// reused without having to recreate it.
        /// </summary>
        public void ClearChangeTracker()
        {
            _context.ClearChangeTracker();
        }

        /// <summary>
        /// Retrieves one message from the queue that is suitable for processing,
        /// And leaves that row in an update lock state so that othter DB transactions will not
        /// get this message.
        /// </summary>
        /// <param name="queueId">Which queue to read from.</param>
        /// <returns></returns>
        public QueueMessage GetOneQueueMessage(int queueId)
        {
            // UPDLOCK makes this row unavailable to other connections and transactions.
            // READPAST to skip any rows that are locked by other connections and transactions.
            // ROWLOCK hint to tell the server to lock at the row level instead of the default page lock.

            // NotBefore so we don't get messages that are scheduled for the future.
            // Completed and Failed are null means not previously processed and not previously exceeded retry count.

            var formattableSQL = FormattableStringFactory.Create(
                "SELECT TOP 1 * FROM[" + 
                _context.Schema + 
                "].QueueMessages WITH(UPDLOCK, READPAST, ROWLOCK) WHERE NotBefore < SYSUTCDATETIME() AND Completed IS NULL AND Failed IS NULL and QueueId = {0}"
                , queueId);

            return _context.QueueMessages.FromSqlInterpolated(formattableSQL).FirstOrDefault();
        }

        /// <summary>
        /// Sends EF Core tracked changes to the database.
        /// </summary>
        public void Save()
        {
            _context.SaveChanges();
        }

        /// <summary>
        /// Start a Database Transaction.
        /// </summary>
        public void BeginTransaction()
        {
            _context.BeginTransaction();
        }

        /// <summary>
        /// Commit the current database transaction.
        /// </summary>
        public void CommitTransaction()
        {
            _context.CommitTransaction();
        }

        /// <summary>
        /// Rollback the current database transaction.
        /// </summary>
        public void RollbackTransaction()
        {
            _context.RollbackTransaction();
        }

        /// <summary>
        /// Create a named save point.
        /// </summary>
        /// <param name="name">The name of the save point to create.</param>
        public void CreateSavepoint(string name)
        {
            _context.CreateSavepoint(name);
        }

        /// <summary>
        /// Rollback to a named save point.
        /// </summary>
        /// <param name="name">The save point to roll back to.</param>
        public void RollbackToSavepoint(string name)
        {
            _context.RollbackToSavepoint(name);
        }

        /// <summary>
        /// Copies completed messages to the completed messages table.
        /// Copies failed messages to the error messages table.
        /// Deletes completed and failed messages from the queue messages table.
        /// </summary>
        /// <returns>The number of complted and failed messages removed from the queue table.</returns>
        public long CleanQueueMessages()
        {
            // Probably want to limit this to N rows at some point.

            const string MessageFields = "[Id], [MessageId], [QueueId], [NotBefore], [Enqueued], [Completed], [Failed], [Retries], [Headers], [Body]";

            string statement = "INSERT INTO [" + _context.Schema  + "].[ErrorMessages] SELECT "
                + MessageFields + " FROM [" + _context.Schema + "].QueueMessages WITH (UPDLOCK, READPAST, ROWLOCK) WHERE Failed IS NOT NULL; " +
                 "INSERT INTO [" + _context.Schema + ".[CompletedMessages] SELECT "
                + MessageFields + " FROM [" + _context.Schema + "].QueueMessages WITH (UPDLOCK, READPAST, ROWLOCK) WHERE Completed IS NOT NULL; " +
                "DELETE FROM [" + _context.Schema + "].QueueMessages WHERE Failed IS NOT NULL OR Completed IS NOT NULL; " +
                "SET @result = @@ROWCOUNT";

            var result = _context.ExectueScalar(statement);
            return long.Parse((string)result);
        }

        /// <summary>
        /// Reads the Saga Data.
        /// </summary>
        /// <param name="className">The Saga class.</param>
        /// <param name="key">The instance key of the saga.</param>
        /// <returns>Matching Saga Data or Null</returns>
        public SagaData GetSagaData(string className, string key)
        {
            // note that we use update locks. This is intentional as only one saga message should be processed at a time.
            // if by chance this row is locked by another thread or process, then the second message will not be able to
            // read the data, and would fail and need to wait and retry again later when the saga is no longer locked.

            var formattableSQL = FormattableStringFactory.Create(
                "SELECT TOP 1 * FROM [" + _context.Schema + "].SagaData WITH (UPDLOCK, READPAST, ROWLOCK) WHERE [Class] = {0} and [Key] = {1}",
                className, key);
            return _context.SagaData.FromSqlInterpolated(formattableSQL).FirstOrDefault();
        }

        /// <summary>
        /// Adds Saga Data to the EF Tracked Entities.
        /// </summary>
        /// <param name="data"></param>
        public void Add(SagaData data)
        {
            _context.SagaData.Add(data);
        }

        /// <summary>
        /// Deletes Saga data that is no longer needed (When Saga is complete).
        /// </summary>
        /// <param name="className">The Saga class</param>
        /// <param name="key">The instance key of the sage</param>
        public void DeleteSagaData(string className, string key)
        {
            string statement = "DELETE FROM [" + _context.Schema + "].[SagaData] WHERE [Class] = @class and [Key] = @key";
            _context.ExectueNonQuery(statement, new[]
            {
                new Parameter("@class", System.Data.DbType.String, className),
                new Parameter("@key", System.Data.DbType.String, key)
            });
        }
    }
}
