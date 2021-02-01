using PeachtreeBus.DatabaseSharing;
using PeachtreeBus.Model;
using Dapper;
using System.Linq;
using System;
using System.Threading.Tasks;

namespace PeachtreeBus.Data
{
    /// <summary>
    /// An implemenatin of IBusDataAccess that uses Dapper to accees the SQL database.
    /// </summary>
    public class DapperDataAccess : IBusDataAccess
    {
        private readonly ISharedDatabase _database;
        private readonly IDbSchema _schema;

        const string SafeChars = "abcdefghijklmnopqrstuvwxyz0123456789";
        const string SchemaUnsafe = "The schema name contains not allowable characters.";
        const string QueueNameUnsafe = "The queue name contains not allowable characters.";
        const string SagaNameUnsafe = "The saga name contains not allowable characters.";

        public DapperDataAccess(ISharedDatabase database, IDbSchema schema)
        {
            _schema = schema;
            _database = database;
        }

        private bool IsUnsafe(string value)
        {
            if (string.IsNullOrEmpty(value)) return true;
            return value.ToLower().Any(c => !SafeChars.Contains(c));
        }

        public void Insert(QueueMessage message, string queueName)
        {
            if (IsUnsafe(_schema.Schema)) throw new ArgumentException(SchemaUnsafe);
            if (IsUnsafe(queueName)) throw new ArgumentException(QueueNameUnsafe);

            string statement =
                "INSERT INTO [" + _schema.Schema + "].[" + queueName + "_QueueMessages] " +
                "([MessageId], [NotBefore], [Enqueued], [Completed], [Failed], [Retries], [Headers], [Body])" +
                " VALUES " +
                "( @MessageId, @NotBefore, @Enqueued, @Completed, @Failed, @Retries, @Headers, @Body); " +
                "SELECT SCOPE_IDENTITY()";

            var p = new DynamicParameters();
            p.Add("@MessageId", message.MessageId);
            p.Add("@NotBefore", message.NotBefore);
            p.Add("@Enqueued", message.Enqueued);
            p.Add("@Completed", message.Completed);
            p.Add("@Failed", message.Failed);
            p.Add("@Retries", message.Retries);
            p.Add("@Headers", message.Headers);
            p.Add("@Body", message.Body);

           message.Id = _database.Connection.QueryFirst<long>(statement, p, _database.Transaction);
        }

        public void Insert(SagaData data, string sagaName)
        {
            if (IsUnsafe(_schema.Schema)) throw new ArgumentException(SchemaUnsafe);
            if (IsUnsafe(sagaName)) throw new ArgumentException(SagaNameUnsafe);

            string statement = " INSERT INTO[" + _schema.Schema + "].[" + sagaName + "_SagaData] " +
                "([SagaId], [Key], [Data])" +
                " VALUES " +
                "(@SagaId, @Key, @Data); "+
                "SELECT SCOPE_IDENTITY()";

            var p = new DynamicParameters();
            p.Add("@SagaId", data.SagaId);
            p.Add("@Key", data.Key);
            p.Add("@Data", data.Data);
           
            data.Id = _database.Connection.QueryFirst<long>(statement, p, _database.Transaction);
        }

        public void BeginTransaction()
        {
            _database.BeginTransaction();
        }

        public long CleanQueueMessages(string queueName)
        {
            if (IsUnsafe(_schema.Schema)) throw new ArgumentException(SchemaUnsafe);
            if (IsUnsafe(queueName)) throw new ArgumentException(QueueNameUnsafe);

            const string MessageFields = "[Id], [MessageId], [NotBefore], [Enqueued], [Completed], [Failed], [Retries], [Headers], [Body]";

            string statement = "INSERT INTO [" + _schema.Schema + "].[" + queueName + "_ErrorMessages] SELECT "
                + MessageFields + " FROM [" + _schema.Schema + "].[" + queueName + "_QueueMessages] WITH (UPDLOCK, READPAST, ROWLOCK) WHERE Failed IS NOT NULL; " +
                 "INSERT INTO [" + _schema.Schema + "].[" + queueName + "_CompletedMessages] SELECT "
                + MessageFields + " FROM [" + _schema.Schema + "].[" + queueName + "_QueueMessages] WITH (UPDLOCK, READPAST, ROWLOCK) WHERE Completed IS NOT NULL; " +
                "DELETE FROM [" + _schema.Schema + "].[" + queueName + "_QueueMessages] WHERE Failed IS NOT NULL OR Completed IS NOT NULL; " +
                "SELECT @@ROWCOUNT";

            return _database.Connection.QueryFirst<long>(statement, transaction: _database.Transaction);
        }

        public void CommitTransaction()
        {
            _database.CommitTransaction();
        }

        public void CreateSavepoint(string name)
        {
            _database.CreateSavepoint(name);
        }

        public long DeleteSagaData(string sagaName, string key)
        {
            if (IsUnsafe(_schema.Schema)) throw new ArgumentException(SchemaUnsafe);
            if (IsUnsafe(sagaName)) throw new ArgumentException(SagaNameUnsafe);

            string statement = "DELETE FROM [" + _schema.Schema + "].[" + sagaName + "_SagaData] WHERE [Key] = @Key; " +
                "SELECT @@ROWCOUNT";
            var p = new DynamicParameters();
            p.Add("@Key", key);

            return _database.Connection.QueryFirst<long>(statement, p, _database.Transaction);
        }

        public QueueMessage GetOneQueueMessage(string queueName)
        {
            if (IsUnsafe(_schema.Schema)) throw new ArgumentException(SchemaUnsafe);
            if (IsUnsafe(queueName)) throw new ArgumentException(QueueNameUnsafe);


            // UPDLOCK makes this row unavailable to other connections and transactions.
            // READPAST to skip any rows that are locked by other connections and transactions.
            // ROWLOCK hint to tell the server to lock at the row level instead of the default page lock.

            // NotBefore so we don't get messages that are scheduled for the future.
            // Completed and Failed are null means not previously processed and not previously exceeded retry count.

            var query = 
                "SELECT TOP 1 * FROM[" +
                _schema.Schema +
                "].[" + queueName + "_QueueMessages] WITH(UPDLOCK, READPAST, ROWLOCK) WHERE NotBefore < SYSUTCDATETIME() AND Completed IS NULL AND Failed IS NULL";

            return _database.Connection.QueryFirstOrDefault<QueueMessage>(query, transaction: _database.Transaction);
        }

        public SagaData GetSagaData(string sagaName, string key)
        {
            if (IsUnsafe(_schema.Schema)) throw new ArgumentException(SchemaUnsafe);
            if (IsUnsafe(sagaName)) throw new ArgumentException(SagaNameUnsafe);

            // note that we use update locks. This is intentional as only one saga message should be processed at a time.
            // if by chance this row is locked by another thread or process, then the second message will not be able to
            // read the data, and would fail and need to wait and retry again later when the saga is no longer locked.
            // We Do Not use READPAST here because we want to wait until the saga unlocks, if there is another processes handling the message for this saga.

            var query = "SELECT TOP 1 * FROM [" +
                _schema.Schema +
                "].[" + sagaName + "_SagaData] WITH (UPDLOCK, READPAST, ROWLOCK) WHERE [Key] = @Key";

            var p = new DynamicParameters();
            p.Add("@Key", key);

            return _database.Connection.QueryFirstOrDefault<SagaData>(query, p, _database.Transaction);
        }

        public void RollbackToSavepoint(string name)
        {
            _database.RollbackToSavepoint(name);
        }

        public void RollbackTransaction()
        {
            _database.RollbackTransaction();
        }

        public void Update(QueueMessage message, string queueName)
        {
            if (IsUnsafe(_schema.Schema)) throw new ArgumentException(SchemaUnsafe);
            if (IsUnsafe(queueName)) throw new ArgumentException(QueueNameUnsafe);

            var statement = "UPDATE[" + _schema.Schema + "].[" + queueName + "_QueueMessages] SET " +
                "[NotBefore] = @NotBefore, " +
                "[Completed] = @Completed, " +
                "[Failed] = @Failed, " +
                "[Retries] = @Retries, " +
                "[Headers] = @Headers, " +
                "[Body] = @Body " +
                "WHERE [Id] = @Id";

            var p = new DynamicParameters();
            p.Add("@Id", message.Id);
            p.Add("@NotBefore", message.NotBefore);
            p.Add("@Completed", message.Completed);
            p.Add("@Failed", message.Failed);
            p.Add("@Retries", message.Retries);
            p.Add("@Headers", message.Headers);
            p.Add("@Body", message.Body);

            _database.Connection.Execute(statement, p, _database.Transaction);
        }

        public void Update(SagaData data, string sagaName)
        {
            if (IsUnsafe(_schema.Schema)) throw new ArgumentException(SchemaUnsafe);
            if (IsUnsafe(sagaName)) throw new ArgumentException(SagaNameUnsafe);

            var statement = "UPDATE [" + _schema.Schema + "].[" + sagaName + "_SagaData] SET " +
                "[Data] = @Data " +
                "WHERE [Id] = @Id";

            var p = new DynamicParameters();
            p.Add("@Id", data.Id);
            p.Add("@Data", data.Data);
            
            _database.Connection.Execute(statement, p, _database.Transaction);
        }

        public async Task<bool> IsSagaLocked(string sagaName, string key)
        {
            if (IsUnsafe(_schema.Schema)) throw new ArgumentException(SchemaUnsafe);
            if (IsUnsafe(sagaName)) throw new ArgumentException(SagaNameUnsafe);

            var statement = "SELECT [SagaId] FROM [" + _schema.Schema + "].[" + sagaName +
                "_SagaData] WITH (NOWAIT) WHERE [Key] = @Key AND [SagaId] NOT IN ( SELECT [SagaId] From [" +
                 _schema.Schema + "].[" + sagaName + "_SagaData] WITH ( READPAST, UPDLOCK, ROWLOCK) WHERE [Key] = @Key )";

            var p = new DynamicParameters();
            p.Add("@Key", key);

            try
            {
                var selected = await _database.Connection.QueryFirstOrDefaultAsync<Guid?>(statement, p, _database.Transaction);
                return selected != null;
            }
            catch(System.Data.SqlClient.SqlException ex)
            {
                // Lock request time out period exceeded. 1222.
                // its safe to assume that its locked, and move on to another message.
                if (ex.Number == 1222) return true; 

                // else we don't know what happened. Throw as normal.
                throw;
            }
        }
    }
}
