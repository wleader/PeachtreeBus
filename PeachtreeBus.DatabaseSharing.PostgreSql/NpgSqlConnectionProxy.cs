using System.Diagnostics.CodeAnalysis;
using System.Threading;
using System.Threading.Tasks;
using Npgsql;

namespace PeachtreeBus.DatabaseSharing.PostgreSql;

/// <summary>
/// An interface around the NpgsqlConnection
/// to facilitate testing.
/// </summary>
public interface INpgSqlConnection : IBaseConnection<NpgsqlConnection, NpgsqlTransaction, INpgSqlTransaction>;

/// <summary>
/// Implements the INpgSqlConnection Interface by passing
/// calls directly through to Npgsql.NpgsqlConnection.
/// </summary>
[ExcludeFromCodeCoverage(Justification =
    "This object requires a real PostgreSQL server to properly test.")] 
public sealed class NpgSqlConnectionProxy(NpgsqlConnection connection)
    : BaseConnection<NpgsqlConnection, NpgsqlTransaction, INpgSqlTransaction>(connection)
    , INpgSqlConnection
{
    public override INpgSqlTransaction BeginTransaction() => 
        new NpgSqlTransactionProxy(Connection.BeginTransaction());

    public override async Task<INpgSqlTransaction> BeginTransactionAsync(
        CancellationToken cancellationToken = default)
    {
        var t = await Connection.BeginTransactionAsync(cancellationToken);
        return new NpgSqlTransactionProxy(t);
    }
}

public class ExternallyManagedNpgSqlConnection(NpgsqlConnection connection)
    :ExternallyManagedConnection<NpgsqlConnection, NpgsqlTransaction, INpgSqlTransaction>(connection)
    , INpgSqlConnection;