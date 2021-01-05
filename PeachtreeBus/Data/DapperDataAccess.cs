using PeachtreeBus.DatabaseSharing;
using PeachtreeBus.Model;
using Dapper;

namespace PeachtreeBus.Data
{
    /// <summary>
    /// An implemenatin of IBusDataAccess that uses Dapper to accees the SQL database.
    /// </summary>
    public class DapperDataAccess : IBusDataAccess
    {
        private readonly ISharedDatabase _database;
        private readonly IDbSchema _schema;

        public DapperDataAccess(ISharedDatabase database, IDbSchema schema)
        {
            _schema = schema;
            _database = database;
        }

        public void Insert(QueueMessage message)
        {
            string statement =
                "INSERT INTO [" + _schema.Schema + "].[QueueMessages] " +
                "([MessageId], [QueueId], [NotBefore], [Enqueued], [Completed], [Failed], [Retries], [Headers], [Body])" +
                " VALUES " +
                "( @MessageId, @QueueId, @NotBefore, @Enqueued, @Completed, @Failed, @Retries, @Headers, @Body); " +
                "SELECT SCOPE_IDENTITY()";

            var p = new DynamicParameters();
            p.Add("@MessageId", message.MessageId);
            p.Add("@QueueId", message.QueueId);
            p.Add("@NotBefore", message.NotBefore);
            p.Add("@Enqueued", message.Enqueued);
            p.Add("@Completed", message.Completed);
            p.Add("@Failed", message.Failed);
            p.Add("@Retries", message.Retries);
            p.Add("@Headers", message.Headers);
            p.Add("@Body", message.Body);

           message.Id = _database.Connection.QueryFirst<long>(statement, p, _database.Transaction);
        }

        public void Insert(SagaData data)
        {
            string statement = " INSERT INTO[" + _schema.Schema + "].[SagaData] " +
                "([SagaId], [Class], [Key], [Data])" +
                " VALUES " +
                "(@SagaId, @Class, @Key, @Data); "+
                "SELECT SCOPE_IDENTITY()";

            var p = new DynamicParameters();
            p.Add("@SagaId", data.SagaId);
            p.Add("@Class", data.Class);
            p.Add("@Key", data.Key);
            p.Add("@Data", data.Data);
           
            data.Id = _database.Connection.QueryFirst<long>(statement, p, _database.Transaction);
        }

        public void BeginTransaction()
        {
            _database.BeginTransaction();
        }

        public long CleanQueueMessages()
        {
            const string MessageFields = "[Id], [MessageId], [QueueId], [NotBefore], [Enqueued], [Completed], [Failed], [Retries], [Headers], [Body]";

            string statement = "INSERT INTO [" + _schema.Schema + "].[ErrorMessages] SELECT "
                + MessageFields + " FROM [" + _schema.Schema + "].QueueMessages WITH (UPDLOCK, READPAST, ROWLOCK) WHERE Failed IS NOT NULL; " +
                 "INSERT INTO [" + _schema.Schema + "].[CompletedMessages] SELECT "
                + MessageFields + " FROM [" + _schema.Schema + "].QueueMessages WITH (UPDLOCK, READPAST, ROWLOCK) WHERE Completed IS NOT NULL; " +
                "DELETE FROM [" + _schema.Schema + "].QueueMessages WHERE Failed IS NOT NULL OR Completed IS NOT NULL; " +
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

        public void DeleteSagaData(string className, string key)
        {
            string statement = "DELETE FROM [" + _schema.Schema + "].[SagaData] WHERE [Class] = @Class and [Key] = @Key";
            var p = new DynamicParameters();
            p.Add("@Class", className);
            p.Add("@Key", key);

            _database.Connection.Execute(statement, p, _database.Transaction);
        }

        public QueueMessage GetOneQueueMessage(int queueId)
        {
            // UPDLOCK makes this row unavailable to other connections and transactions.
            // READPAST to skip any rows that are locked by other connections and transactions.
            // ROWLOCK hint to tell the server to lock at the row level instead of the default page lock.

            // NotBefore so we don't get messages that are scheduled for the future.
            // Completed and Failed are null means not previously processed and not previously exceeded retry count.

            var query = 
                "SELECT TOP 1 * FROM[" +
                _schema.Schema +
                "].QueueMessages WITH(UPDLOCK, READPAST, ROWLOCK) WHERE NotBefore < SYSUTCDATETIME() AND Completed IS NULL AND Failed IS NULL and QueueId = @queueId";

            return _database.Connection.QueryFirstOrDefault<QueueMessage>(query, new { queueId }, transaction: _database.Transaction);
        }

        public SagaData GetSagaData(string className, string key)
        {

            // note that we use update locks. This is intentional as only one saga message should be processed at a time.
            // if by chance this row is locked by another thread or process, then the second message will not be able to
            // read the data, and would fail and need to wait and retry again later when the saga is no longer locked.
            // We Do Not use READPAST here because we want to wait until the saga unlocks, if there is another processes handling the message for this saga.

            var query = "SELECT TOP 1 * FROM [" +
                _schema.Schema +
                "].SagaData WITH (UPDLOCK, ROWLOCK) WHERE [Class] = @Class and [Key] = @Key";

            var p = new DynamicParameters();
            p.Add("@Class", className);
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

        public void Update(QueueMessage message)
        {
            var statement = "UPDATE[" + _schema.Schema + "].[QueueMessages] SET " +
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

        public void Update(SagaData data)
        {
            var statement = "UPDATE [" + _schema.Schema + "].[SagaData] SET " +
                "[Data] = @Data " +
                "WHERE [Id] = @Id";

            var p = new DynamicParameters();
            p.Add("@Id", data.Id);
            p.Add("@Data", data.Data);
            
            _database.Connection.Execute(statement, p, _database.Transaction);
        }
    }
}
