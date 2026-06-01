using Microsoft.Data.SqlClient;

namespace PeachtreeBus.DatabaseSharing;

/// <summary>
/// A mockable interface around the SQL Transaction
/// to facilitate testing.
/// </summary>
public interface ISqlTransaction : IBaseTransaction<SqlTransaction>;

/// <summary>
/// Implements the ISqlTransaction interface by passing
/// directly though to Microsoft.Data.SqlClient.SqlTransaction.
/// </summary>
public class SqlTransactionProxy(SqlTransaction transaction)
    : BaseTransaction<SqlTransaction>(transaction)
    , ISqlTransaction;
