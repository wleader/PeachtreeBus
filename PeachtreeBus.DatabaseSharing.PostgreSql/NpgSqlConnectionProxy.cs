using System.Diagnostics.CodeAnalysis;
using System.Threading;
using System.Threading.Tasks;
using Npgsql;

namespace PeachtreeBus.DatabaseSharing.PostgreSql;

/// <summary>
/// An interface around the NpgsqlConnection
/// to facilitate testing.
/// </summary>
public interface INpgsqlConnection : IBaseConnection<NpgsqlConnection, NpgsqlTransaction, INpgsqlTransaction>;

/// <summary>
/// Implements the INpgsqlcConnection Interface by passing
/// calls directly through to Npgsql.NpgsqlConnection.
/// </summary>
[ExcludeFromCodeCoverage(Justification =
    "This object requires a real PostgreSQL server to properly test.")] 
public sealed class NpgSqlConnectionProxy(NpgsqlConnection connection)
    : BaseConnection<NpgsqlConnection, NpgsqlTransaction, INpgsqlTransaction>(connection)
    , INpgsqlConnection
{
    public override INpgsqlTransaction BeginTransaction() => 
        new NpgSqlTransactionProxy(Connection.BeginTransaction());

    public override async Task<INpgsqlTransaction> BeginTransactionAsync(
        CancellationToken cancellationToken = default)
    {
        var t = await Connection.BeginTransactionAsync(cancellationToken);
        return new NpgSqlTransactionProxy(t);
    }
}