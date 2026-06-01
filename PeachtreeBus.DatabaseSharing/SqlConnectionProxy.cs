using Microsoft.Data.SqlClient;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using System.Threading.Tasks;

namespace PeachtreeBus.DatabaseSharing;

/// <summary>
/// An interface around the SQL Connection
/// to facilitate testing.
/// </summary>
public interface ISqlConnection : IBaseConnection<SqlConnection, SqlTransaction,
    ISqlTransaction>;

/// <summary>
/// Implements the ISqlConnection interface by passing
/// directly though to Microsoft.Data.SqlClient.SqlConnection.
/// </summary>
[ExcludeFromCodeCoverage(Justification =
    "This object requires a real SQL server to properly test.")]
public class SqlConnectionProxy(SqlConnection connection)
    : BaseConnection<SqlConnection, SqlTransaction, ISqlTransaction>(connection), ISqlConnection
{
    public override ISqlTransaction BeginTransaction() => 
        new SqlTransactionProxy(Connection.BeginTransaction());

    public override async Task<ISqlTransaction> BeginTransactionAsync(
        CancellationToken cancellationToken = default)
    {
        var t = await Connection.BeginTransactionAsync(cancellationToken);
        return new SqlTransactionProxy((SqlTransaction)t);
    }
}