using Npgsql;

namespace PeachtreeBus.DatabaseSharing.PostgreSql;

public interface INpgsqlTransaction : IBaseTransaction<NpgsqlTransaction>;

public sealed class NpgSqlTransactionProxy(NpgsqlTransaction transaction)
    : BaseTransaction<NpgsqlTransaction>(transaction)
    , INpgsqlTransaction;