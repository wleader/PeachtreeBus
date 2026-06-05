using System.Data.Common;
using Npgsql;

namespace PeachtreeBus.DatabaseSharing.PostgreSql;

public interface INpgSqlSharedDatabase : IBaseSharedDatabase<NpgsqlConnection, NpgsqlTransaction>;

public class NpgSqlSharedDatabase(INpgSqlConnectionFactory connectionFactory)
    : BaseSharedDatabase<NpgsqlConnection, INpgSqlConnection, NpgsqlTransaction, INpgSqlTransaction>(connectionFactory)
    , INpgSqlSharedDatabase
{
    protected override INpgSqlConnection CreateExternalConnection(DbConnection connection)
    {
        var npgsqlConnection = connection as NpgsqlConnection
                            ?? throw new ExternallyManagedConnectionException(
                                "The provided connection object is not an NpgsqlConnection.");
        return new ExternallyManagedNpgSqlConnection(npgsqlConnection);
    }

    protected override INpgSqlTransaction CreateExternalTransaction(DbTransaction transaction)
    {
        var npgsqlTransaction = transaction as NpgsqlTransaction
                             ?? throw new ExternallyManagedTransactionException(
                                 "The provided transaction object is not an NpgsqlTransaction.");
        return new ExternallyManagedNpgSqlTransaction(npgsqlTransaction);
    }
}