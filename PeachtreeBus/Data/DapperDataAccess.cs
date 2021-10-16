using PeachtreeBus.DatabaseSharing;
using PeachtreeBus.Model;
using Dapper;
using System.Linq;
using System;
using System.Threading.Tasks;
using System.Data;

namespace PeachtreeBus.Data
{
    public class DateTimeHandler : SqlMapper.TypeHandler<DateTime>
    {
        
        public override void SetValue(IDbDataParameter parameter, DateTime value)
        {
            parameter.Value = value;
        }

        public override DateTime Parse(object value)
        {
            return DateTime.SpecifyKind((DateTime)value, DateTimeKind.Utc);
        }
    }

    /// <summary>
    /// An implemenatin of IBusDataAccess that uses Dapper to accees the SQL database.
    /// </summary>
    public class DapperDataAccess : IBusDataAccess
    {
        static DapperDataAccess()
        {
            SqlMapper.AddTypeHandler(new DateTimeHandler());
        }

        private readonly ISharedDatabase _database;
        private readonly IDbSchemaConfiguration _schemaConfig;

        const string SafeChars = "abcdefghijklmnopqrstuvwxyz0123456789";
        const string SchemaUnsafe = "The schema name contains not allowable characters.";
        const string QueueNameUnsafe = "The queue name contains not allowable characters.";
        const string SagaNameUnsafe = "The saga name contains not allowable characters.";

        public DapperDataAccess(ISharedDatabase database, IDbSchemaConfiguration schemaConfig)
        {
            _schemaConfig = schemaConfig;
            _database = database;
        }

        private bool IsUnsafe(string value)
        {
            if (string.IsNullOrEmpty(value)) return true;
            return value.ToLower().Any(c => !SafeChars.Contains(c));
        }
                
        private const string EnqueueMessageStatement =
            "INSERT INTO [{0}].[{1}_PendingMessages] " +
            " ([MessageId], [NotBefore], [Enqueued], [Completed], [Failed], [Retries], [Headers], [Body]) " +
            "VALUES" +
            " ( @MessageId, @NotBefore, SYSUTCDATETIME(), NULL, NULL, 0, @Headers, @Body); " +
            "SELECT SCOPE_IDENTITY()";

        public Task<long> EnqueueMessage(QueueMessage message, string queueName)
        {
            if (IsUnsafe(_schemaConfig.Schema))
                throw new ArgumentException(SchemaUnsafe);
            if (IsUnsafe(queueName))
                throw new ArgumentException(QueueNameUnsafe);
            if (message == null)
                throw new ArgumentNullException(nameof(message));
            if (message.MessageId == null) 
                throw new ArgumentNullException($"{nameof(message)}.{nameof(message.MessageId)}");
            if (message.NotBefore == null) 
                throw new ArgumentNullException($"{nameof(message)}.{nameof(message.NotBefore)}");
            if (message.NotBefore.Kind == DateTimeKind.Unspecified)
                throw new ArgumentException($"{nameof(message)}.{nameof(message.NotBefore)}.{nameof(message.NotBefore.Kind)} cannot be unspecified.");
            if (string.IsNullOrEmpty(message.Headers)) 
                throw new ArgumentException($"{nameof(message)}.{nameof(message.Headers)} must be not null and not empty.");
            if (string.IsNullOrEmpty(message.Body)) 
                throw new ArgumentException($"{nameof(message)}.{nameof(message.Body)} must be not null and not empty.");

            string statement = string.Format(EnqueueMessageStatement, _schemaConfig.Schema, queueName);

            var p = new DynamicParameters();
            p.Add("@MessageId", message.MessageId);
            p.Add("@NotBefore", message.NotBefore.ToUniversalTime());
            p.Add("@Headers", message.Headers);
            p.Add("@Body", message.Body);

           return _database.Connection.QueryFirstAsync<long>(statement, p, _database.Transaction);
        }

        // UPDLOCK makes this row unavailable to other connections and transactions.
        // READPAST to skip any rows that are locked by other connections and transactions.
        // ROWLOCK hint to tell the server to lock at the row level instead of the default page lock.
        // NotBefore so we don't get messages that are scheduled for the future.
        // Completed and Failed are null means not previously processed and not previously exceeded retry count.
        private const string GetOnePendingMessageStatement =
            "SELECT TOP 1 *" + 
            " FROM[{0}].[{1}_PendingMessages]" +
            " WITH(UPDLOCK, READPAST, ROWLOCK)" + 
            " WHERE NotBefore < SYSUTCDATETIME()" +
            "  AND Completed IS NULL " + 
            "  AND Failed IS NULL";

        public Task<QueueMessage> GetOnePendingMessage(string queueName)
        {
            if (IsUnsafe(_schemaConfig.Schema))
                throw new ArgumentException(SchemaUnsafe);
            if (IsUnsafe(queueName))
                throw new ArgumentException(QueueNameUnsafe);

            var query = string.Format(GetOnePendingMessageStatement, _schemaConfig.Schema, queueName);

            return _database.Connection.QueryFirstOrDefaultAsync<QueueMessage>(query, transaction: _database.Transaction);
        }

        private const string CompleteMessageStatement =
            "DECLARE " +
            " @MessageId UNIQUEIDENTIFIER," +
            " @Enqueued DATETIME2, " +
            " @Body NVARCHAR(MAX), " +
            " @Retries TINYINT, " +
            " @NotBefore DATETIME2; " +
            "SELECT " +
            " @MessageId = [MessageId]," +
            " @Enqueued = [Enqueued]," +
            " @Body = [Body]," +
            " @Retries = [Retries]," +
            " @NotBefore = [NotBefore] " +
            "FROM [{0}].[{1}_PendingMessages]" +
            "WHERE [Id] = @Id; " +
            "INSERT INTO[{0}].[{1}_CompletedMessages]" +
            " ([Id], [MessageId], [NotBefore], [Enqueued], [Completed], [Failed], [Retries], [Headers], [Body])" +
            "VALUES " +
            " (@Id, @MessageId, @NotBefore, @Enqueued, SYSUTCDATETIME(), NULL, @Retries, @Headers, @Body); " +
            "DELETE FROM[{0}].[{1}_PendingMessages]" +
            " WHERE[Id] = @Id; ";

        public Task CompleteMessage(QueueMessage message, string queueName)
        {
            if (IsUnsafe(_schemaConfig.Schema))
                throw new ArgumentException(SchemaUnsafe);
            if (IsUnsafe(queueName))
                throw new ArgumentException(QueueNameUnsafe);
            if (message == null)
                throw new ArgumentNullException(nameof(message));
            if (string.IsNullOrEmpty(message.Headers))
                throw new ArgumentException($"{nameof(message)}.{nameof(message.Headers)} must be not null and not empty.");

            string statement = string.Format(CompleteMessageStatement, _schemaConfig.Schema, queueName);

            var p = new DynamicParameters();
            p.Add("@Id", message.Id);
            p.Add("@Headers", message.Headers);
            
            return _database.Connection.ExecuteAsync(statement, p, _database.Transaction);
        }

        private const string FailMessageStatement =
            "DECLARE " +
            " @MessageId UNIQUEIDENTIFIER," +
            " @Enqueued DATETIME2, " +
            " @Body NVARCHAR(MAX), " +
            " @Retries TINYINT, " +
            " @NotBefore DATETIME2; " +
            "SELECT " +
            " @MessageId = [MessageId]," +
            " @Enqueued = [Enqueued]," +
            " @Body = [Body]," +
            " @Retries = [Retries]," +
            " @NotBefore = [NotBefore] " +
            "FROM [{0}].[{1}_PendingMessages]" +
            "WHERE [Id] = @Id; " +
            "INSERT INTO[{0}].[{1}_ErrorMessages] " +
            "([Id], [MessageId], [NotBefore], [Enqueued], [Completed], [Failed], [Retries], [Headers], [Body]) " +
            "VALUES " +
            "(@Id, @MessageId, @NotBefore, @Enqueued, NULL, SYSUTCDATETIME(), @Retries, @Headers, @Body); " +
            "DELETE FROM[{0}].[{1}_PendingMessages] WHERE[Id] = @Id; ";

        public Task FailMessage(QueueMessage message, string queueName)
        {
            if (IsUnsafe(_schemaConfig.Schema))
                throw new ArgumentException(SchemaUnsafe);
            if (IsUnsafe(queueName))
                throw new ArgumentException(QueueNameUnsafe);
            if (message == null)
                throw new ArgumentNullException(nameof(message));
            if (string.IsNullOrEmpty(message.Headers))
                throw new ArgumentException($"{nameof(message)}.{nameof(message.Headers)} must be not null and not empty.");

            string statement = string.Format(FailMessageStatement, _schemaConfig.Schema, queueName);

            var p = new DynamicParameters();
            p.Add("@Id", message.Id);
            p.Add("@Headers", message.Headers);
            
            return _database.Connection.ExecuteAsync(statement, p, _database.Transaction);
        }

        private const string UpdateMessageStatement =
            "UPDATE[{0}].[{1}_PendingMessages] SET " +
            "[NotBefore] = @NotBefore, " +
            "[Retries] = @Retries, " +
            "[Headers] = @Headers " +
            "WHERE [Id] = @Id";

        public Task Update(QueueMessage message, string queueName)
        {
            if (IsUnsafe(_schemaConfig.Schema))
                throw new ArgumentException(SchemaUnsafe);
            if (IsUnsafe(queueName))
                throw new ArgumentException(QueueNameUnsafe);
            if (message == null)
                throw new ArgumentNullException(nameof(message));
            if (message.NotBefore == null)
                throw new ArgumentNullException($"{nameof(message)}.{nameof(message.NotBefore)}");
            if (message.NotBefore.Kind == DateTimeKind.Unspecified)
                throw new ArgumentException($"{nameof(message)}.{nameof(message.NotBefore)}.{nameof(message.NotBefore.Kind)} cannot be unspecified.");
            if (string.IsNullOrEmpty(message.Headers))
                throw new ArgumentException($"{nameof(message)}.{nameof(message.Headers)} must be not null and not empty.");

            var statement = string.Format(UpdateMessageStatement, _schemaConfig.Schema, queueName);

            var p = new DynamicParameters();
            p.Add("@Id", message.Id);
            p.Add("@NotBefore", message.NotBefore.ToUniversalTime());
            p.Add("@Retries", message.Retries);
            p.Add("@Headers", message.Headers);

            return _database.Connection.ExecuteAsync(statement, p, _database.Transaction);
        }

        private const string InsertSagaStatement =
            "INSERT INTO[{0}].[{1}_SagaData]" +
            " ([SagaId], [Key], [Data]) " +
            "VALUES" +
            " (@SagaId, @Key, @Data); " +
            "SELECT SCOPE_IDENTITY()";

        public Task<long> Insert(SagaData data, string sagaName)
        {
            if (IsUnsafe(_schemaConfig.Schema)) 
                throw new ArgumentException(SchemaUnsafe);
            if (IsUnsafe(sagaName)) 
                throw new ArgumentException(SagaNameUnsafe);
            if (data == null)
                throw new ArgumentNullException(nameof(data));
            if (data.SagaId == null)
                throw new ArgumentNullException($"{nameof(data)}.{nameof(data.SagaId)}");
            if (string.IsNullOrEmpty(data.Key))
                throw new ArgumentException($"{nameof(data)}.{nameof(data.Key)} must be not null and not empty.");
            if (string.IsNullOrEmpty(data.Data))
                throw new ArgumentException($"{nameof(data)}.{nameof(data.Data)} must be not null and not empty.");

            string statement = string.Format(InsertSagaStatement, _schemaConfig.Schema, sagaName);

            var p = new DynamicParameters();
            p.Add("@SagaId", data.SagaId);
            p.Add("@Key", data.Key);
            p.Add("@Data", data.Data);
           
            return _database.Connection.QueryFirstAsync<long>(statement, p, _database.Transaction);
        }

        private const string UpdateSagaStatement =
            "UPDATE [{0}].[{1}_SagaData] SET" +
            " [Data] = @Data " +
            "WHERE [Id] = @Id";

        public Task Update(SagaData data, string sagaName)
        {
            if (IsUnsafe(_schemaConfig.Schema))
                throw new ArgumentException(SchemaUnsafe);
            if (IsUnsafe(sagaName))
                throw new ArgumentException(SagaNameUnsafe);
            if (data == null)
                throw new ArgumentNullException(nameof(data));
            if (string.IsNullOrEmpty(data.Data))
                throw new ArgumentException($"{nameof(data)}.{nameof(data.Data)} must be not null and not empty.");

            var statement = string.Format(UpdateSagaStatement, _schemaConfig.Schema, sagaName);

            var p = new DynamicParameters();
            p.Add("@Id", data.Id);
            p.Add("@Data", data.Data);

            return _database.Connection.ExecuteAsync(statement, p, _database.Transaction);
        }

        private const string DeleteSagaStatement =
            "DELETE FROM [{0}].[{1}_SagaData] WHERE [Key] = @Key";

        public Task DeleteSagaData(string sagaName, string key)
        {
            if (IsUnsafe(_schemaConfig.Schema))
                throw new ArgumentException(SchemaUnsafe);
            if (IsUnsafe(sagaName))
                throw new ArgumentException(SagaNameUnsafe);
            if (string.IsNullOrEmpty(key))
                throw new ArgumentException($"{nameof(key)} must be not null and not empty.");

            string statement = string.Format(DeleteSagaStatement, _schemaConfig.Schema, sagaName);
            var p = new DynamicParameters();
            p.Add("@Key", key);

            return _database.Connection.ExecuteAsync(statement, p, _database.Transaction);
        }

        private const string GetSagaDataStatement =
            "DECLARE" +
            " @Id bigint," +
            " @SagaId uniqueidentifier," +
            " @Data nvarchar(max) " +
            "BEGIN TRY" +
            // First Select into variables, and try to get an update lock.
            // NOWAIT hint means it will go straight to CATCH if the row is locked.
            "  SELECT" +
            "    @Id = [Id], @SagaId = [SagaId], @Data = [Data]" +
            "  FROM [{0}].[{1}_SagaData] WITH (NOWAIT, UPDLOCK, ROWLOCK)" +
            "  WHERE[Key] = @Key" +

            "  IF @@ROWCOUNT > 0" +
            //   A row was selected, so we got the data and locked it for ourselves
            //   return the data and note that the saga is not blocked.
            "    SELECT @Id as [Id], @SagaId as [SagaId], @Key as [Key], @Data as [Data], 0 as [Blocked]" +
            "  ELSE" +
            //   A row was not selected. but we can't be sure if it was because of a lock (NOWAIT is screwy)
            //   So select the data without trying to get an update lock, either we will select the data, but blocked will be true,
            //   or we will get no rows (because the row doesn't exist, which is ok if the saga is about to be started.)
            "    SELECT [Id], [SagaId], [Key], [Data], 1 as [Blocked]" +
            "    FROM [{0}].[{1}_SagaData] WITH (NOWAIT)" +
            "    WHERE [Key] = @Key " +
            "END TRY " +
            "BEGIN CATCH" +
            // If any of the above selects failed, we can assume the saga is locked and return a row with blocked true.
            // Which will signal the message processor to delay and retry.
            "  SELECT -1 as [Id], CONVERT(uniqueidentifier, '00000000-0000-0000-0000-000000000000') as [SagaId], @Key as [Key], '' as [Data], 1 as [Blocked] " +
            "END CATCH ";

        public Task<SagaData> GetSagaData(string sagaName, string key)
        {
            if (IsUnsafe(_schemaConfig.Schema))
                throw new ArgumentException(SchemaUnsafe);
            if (IsUnsafe(sagaName))
                throw new ArgumentException(SagaNameUnsafe);
            if (string.IsNullOrEmpty(key))
                throw new ArgumentException($"{nameof(key)} must be not null and not empty.");

            var query = string.Format(GetSagaDataStatement, _schemaConfig.Schema, sagaName);
            
            var p = new DynamicParameters();
            p.Add("@Key", key);

            return _database.Connection.QueryFirstOrDefaultAsync<SagaData>(query, p, _database.Transaction);
        }

        public void BeginTransaction()
        {
            _database.BeginTransaction();
        }

        public void CommitTransaction()
        {
            _database.CommitTransaction();
        }

        public void CreateSavepoint(string name)
        {
            _database.CreateSavepoint(name);
        }

        public void RollbackToSavepoint(string name)
        {
            _database.RollbackToSavepoint(name);
        }

        public void RollbackTransaction()
        {
            _database.RollbackTransaction();
        }
    }
}
