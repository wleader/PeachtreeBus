using Npgsql;

namespace PeachtreeBus.DatabaseSharing.PostgreSql;

public interface INpgSqlTransaction : IBaseTransaction<NpgsqlTransaction>;

public sealed class NpgSqlTransactionProxy(NpgsqlTransaction transaction)
    : BaseTransaction<NpgsqlTransaction>(transaction)
    , INpgSqlTransaction;
    
    public class ExternallyManagedNpgSqlTransaction(NpgsqlTransaction transaction)
    :ExternallyManagedTransaction<NpgsqlTransaction>(transaction)
    , INpgSqlTransaction;